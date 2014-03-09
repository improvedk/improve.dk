permalink: making-the-aspnet-state-service-work-across-network
title: Making the ASP.NET State Service Work Across Network
date: 2009-08-24
tags: [.NET, IIS]
---
Once you start distributing your ASP.NET website across multiple webservers, you're going to need a way to share session state. That is, unless your app is stateless, in which case scaling it should be a breeze!

<!-- more -->

One of the easiest ways to provide common session state for a small cluster (very dependant on load and hardware specs, but ~10 servers max, per state server), is to use the built-in ASP.NET State Service. It's a free service that's installed alongside the .NET Framework on all Windows servers.

While the InProc session storage is stored directly in the w3wp process, the ASP.NET State Service is an independant process that runs alongside your IIS w3wp processes. The State Service does not have to run on a machine with IIS installed - it can run on a machine dedicated to serving session state for other web servers running IIS.

## Performance

Switching from InProc to State Service *will* have an impact on performance. We now have to cross not only process boundaries, but also machine boundaries. Furthermore, once we go out-of-process, all objects will have to be serialized, requiring extra work and requires all objects to be marked with the [[Serializable]](http://msdn.microsoft.com/en-us/library/system.serializableattribute.aspx) attribute.

The State Service performance is heavily reliant on memory. Once physical memory has been exhausted it'll start paging to disk which will kill performance quickly. Make sure to monitor the memory load on your State Service machine(s) and adjust memory accordingly.

## Enabling remote connectivity

By default, the State Service will only allow local-to-machine connections. To allow remote connections you'll have to set the HKLMSYSTEMCurrentControlSetServicesaspnet_stateParametersAllowRemoteConnection key to a value of '1'. After changing the AllowRemoveConnection key value, you'll have to restart the State Service service for the change to take effect. Also make sure your firewall allows connectivity to the State Service port (TCP 42424 by default).

## Requirements


* All session objects must be serializable.
* All IIS websites that are to share session state must have a common IIS application path (the [ID column in the sites list](http://improve.dk/viewimage/iisvalidobject)). I strongly recommend you look into the IIS7 [Shared Configuration](http://learn.iis.net/page.aspx/264/shared-configuration/) feature as it'll help you keep all the web servers IIS7 configuration in synch, including the application path.
* All websites that are to share session state must have the same [<machineKey />](http://msdn.microsoft.com/en-us/library/ms998288.aspx) values so they're able to read one anothers sessions. You can [generate the keys online](http://aspnetresources.com/tools/keycreator.aspx).


## Scalability

If you start saturating your dedicated State Service machine, it's possible to implement session state partitioning by implementing the[System.Web.IPartitionResolver](http://msdn.microsoft.com/en-us/library/system.web.ipartitionresolver.aspx) interface. By creating your own implementating, you can route new requests to different state servers and perhaps even check whether the state servers are available or not, to add redundancy. Note however that this will not give you redundancy in case the State Service crashes either due to software or hardware.
