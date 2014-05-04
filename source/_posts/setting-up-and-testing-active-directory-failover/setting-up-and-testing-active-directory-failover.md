---
permalink: setting-up-and-testing-active-directory-failover
title: Setting Up and Testing Active Directory Failover
date: 2008-03-02
tags: [.NET, Windows]
---
I spend a lot of time architecting for scalability, availability and security during my daily work. Currently I've got a distributed system consisting of several windows services communicating across machines using WCF and authenticating through Active Directory.

<!-- more -->

In such a situation, if the Active Directory Domain Controller (let's just call it DC from now on) dies, everything more or less dies as no clients/servers are able to authenticate incoming requests anymore. Security is paramount, so the services are not allowed to simply cache the domain logon, thus the logon has the occur at each service call - requiring a fully working DC.

In this post I'll attempt to implement a secondary failover DC and investigate how it affects a downtime situation. I'll be using a couple of simple WCF based applications to test the DC. I will be using three virtual machines. Luxor is the primary DC. MGM is part of the domain, and this is the machine hosting the WCF server. Later on I'll add the third virtual machine, Excalibur, being the failover DC. The WCF client will be running from my own machine. Note that I will not show how to install the primary DC, [there are plenty other great guides on how to setup the primary DC](http://www.petri.co.il/how_to_install_active_directory_on_windows_2003.htm). I also won't be going into [how to install the primary DNS server](http://support.microsoft.com/kb/814591).

This is the WCF server interface:

```cs
using System;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Contracts
{
	public class WCFHelper
	{
		/// <summary>
		/// Creates an instance of the specified interface type by channeling to the service host.
		/// </summary>
		/// <typeparam name="T">The interface type to create.</typeparam>
		/// <param name="binding">The binding protocol to use.</param>
		/// <param name="endpointAddress">The complete address for the endpoint.</param>
		/// <returns>An instance of type T.</returns>
		public static T CreateChannel<T>(Binding binding, string endpointAddress)
		{
			// Create an endpoint for the specified binding & address
			ServiceEndpoint endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(T)), binding, new EndpointAddress(endpointAddress));
			binding.SendTimeout = new TimeSpan(0, 0, 1);
			binding.ReceiveTimeout = new TimeSpan(0, 0, 1);
			binding.OpenTimeout = new TimeSpan(0, 0, 1);
			binding.CloseTimeout = new TimeSpan(0, 0, 1);

			// Create a channel factory of type T
			ChannelFactory<T> factory = new ChannelFactory<T>(endpoint);
			factory.Credentials.Windows.ClientCredential.UserName = "RedundancyCheck";
			factory.Credentials.Windows.ClientCredential.Password = "RedundancyCheck";
			factory.Credentials.Windows.ClientCredential.Domain = "IPAPER";
			factory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

			// Return the created channel
			return factory.CreateChannel();
		}

		/// <summary>
		/// Creates a ServiceHost hosting the specific implementation TImplementationType of interface TInterfaceType at the specified endpointAddress.
		/// </summary>
		/// <typeparam name="TInterfaceType">The interface type to host.</typeparam>
		/// <typeparam name="TImplentationType">The implementation type to host.</typeparam>
		/// <param name="binding">The binding protocol to use.</param>
		/// <param name="endpointAddress">The endpoint where the service should be hosted at.</param>
		/// <param name="mexEndpointAddress">The endpoint where the service metadata should be hosted at.</param>
		/// <returns>An instance of ServiceHost.</returns>
		public static ServiceHost CreateServiceHost<TInterfaceType, TImplentationType>(Binding binding, string endpointAddress, string mexEndpointAddress)
		{
			// Create new service host
			ServiceHost host = new ServiceHost(typeof(TImplentationType));

			// Create endpoints
			host.AddServiceEndpoint(typeof(TInterfaceType), binding, endpointAddress);

			// Create metadata endpoint if it doesn't exist
			ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
			if (smb == null)
			{
				smb = new ServiceMetadataBehavior();
				smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
				host.Description.Behaviors.Add(smb);
			}
			if (binding is NetTcpBinding)
				host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), mexEndpointAddress);
			else if (binding is BasicHttpBinding)
				host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), mexEndpointAddress);
			else
				throw new ArgumentOutOfRangeException("Invalid binding: " + binding);

			return host;
		}
	}
}
```

Notice that I've hardcoded the user "IPAPERRedundancyCheck" with a password of "RedundancyCheck" - this is for test purposes only, don't even bother commenting on password security :) Also notice that I've set a timeout of 1 second - when the DC fails, I don't want to spend 30 seconds before knowing if it's down, I want to know about it right away. And since all machines are running locally, 1 second is plenty.

This is the server that'll be running on the MGM machine on port 8000:

```cs
using System;
using System.Security;
using System.Security.Permissions;
using System.ServiceModel;
using Contracts;

namespace WcfServer
{
	class Program
	{
		static void Main(string[] args)
		{
			// Create service host
			ServiceHost host = WCFHelper.CreateServiceHost<IServer, Server>(new NetTcpBinding(SecurityMode.Transport), "net.tcp://localhost:8000", "net.tcp://localhost:8000/mex");

			// Open host
			host.Open();

			Console.Read();
		}
	}

	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class Server : IServer
	{
		public string Ping()
		{
			// Security check
			try
			{
				new PrincipalPermission(null, "ADTest").Demand();
			}
			catch
			{
				throw new SecurityException();
			}

			// Let caller know that we're alive
			return "Pong!";
		}
	}
}
```

We've got an implementation of the IServer interface with the single method "Ping". We test that the user is part of the "ADTest" role by demanding it on the current principal. If something fails we throw a SecurityException which will let the client now authentication failed. If the client is authenticated, we return a pong.

And finally we have the client that'll be running on my own machine:

```cs
using System;
using System.ServiceModel;
using System.Threading;
using Contracts;

namespace WcfClient
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				try
				{
					IServer server = WCFHelper.CreateChannel(new NetTcpBinding(SecurityMode.Transport), "net.tcp://192.168.0.35:8000");

					using (server as IDisposable)
					{
						try
						{
							Console.WriteLine(server.Ping());
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
				}
				catch (CommunicationObjectFaultedException)
				{ }

				Thread.Sleep(250);
			}
		}
	}
}
```

We create a channel to the MGM machine (static IP of 192.168.0.35). We'll continue to call the IServer service every 250ms, writing either the result of the Ping function, or a message explaining any problems that have occurred. Note that in real life situations we'd not create a new channel each time, but in this case we have to, so we really do authenticate on each call (so we're affected immediatly when the DC dies).

The following clip will show what happens when we run the server & client while the DC goes down (by pausing the virtual machine). Note that we're receiving a timeout exception, not a SecurityException. This is because I'm using SecurityMode.Transport which requires us to authenticate before even reaching the service, thus the method is never invoked, and the PrincipPermission.Demand() call is NOT the one failing us, it's the WCF security layer trying to open a TCP transport. As soon as the DC (running on LUXOR) fails, we lose connectivity with our service (running on MGM).


{% youtube P7ufIfZlfjU %}


The goal obviously is to prevent this from happening, we cannot have all our services brought to a standstill if the DC fails. The first step in installing a failover AD DC is to get a DNS secondary server up and running (on the soon to be secondary DC machine) so we have redundant DNS functionality.

A quick recap of the servers:

```
LUXOR = Primary DC, primary DNS
EXCALIBUR = To be secondary DC, secondary DNS
MGM = Client server
```


{% youtube BE5mB417BNs %}


Now that we've got the secondary DNS set up, we're ready to install Active Directory on the secondary AD server (EXCALIBUR). The following video shows how easy it is to install a failover DC:


{% youtube x5qTxr-pglg %}


That's it! After the server reboots, it now functions as a failover DC in case the primary one kicks the bucket. I'll end this post post by running my WcfServer application on the MGM server whilst both LUXOR and EXCALIBUR are running. You'll se a fluent stream of "Pong"s returning. After shutting down LUXOR, the WCF client will immediately start reporting connection problems, but after a short while it automatically starts returning Pongs again - it got a hold of the second DC! Now, if I shut down the second DC aswell, we'll get errors in our client again. If I then restart the primary DC, after a short while, the client starts Ponging again - we got a hold of the primary DC. So we haven't eliminated downtime completely, but we've reduced it to a 5-30 sec period before everything automatically switches over to the failover DC.


{% youtube KWkEjiulJu8 %}


This is my first blog post utilizing videos - does it work? Do you prefer seeing live video like this, or a long series of screenshots? I know what I prefer :)
