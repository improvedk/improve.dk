permalink: controlling-sqlconnection-timeouts
title: Controlling SqlConnection Timeouts
date: 2008-03-10
tags: [.NET]
---
When performing queries against a SQL Server database, there are a couple of methods readily available. However, an option is missing.

<!-- more -->

The primary timeout value is that of SqlConnection.ConnectionTimeout. This specifies how long time the SQL Server service has to respond to a connection attempt. You cannot set this value directly, you'll have to set it as part of the connection string:

```
Data Source=server;Initial Catalog=databaseUser Id=username;Password=password;Connect Timeout=30
```

Note that the value is expressed in seconds, not milliseconds. The default value is 30 seconds. Secondly, we can use the [SqlCommand.CommandTimeout](http://msdn2.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout.aspx) value. This sets the timeout value of a specific query running on SQL Server. The problem with these two is that we're missing a point in the pipeline, which goes:

TCP Connection to SQL Server -> SqlConnection.Open -> SqlCommand.Execute

The last two are covered, but if for some reason the SQL Server is dead, taken off the network, totally overloaded, we may get a timeout on the TCP level - and this could take a while. We currently have no way of controlling this timeout besides a server wide network level setting. Often, it's not desirable to have your application potentially spending several minutes before receiving a TCP timeout - or sometimes simply wait indefinitely. We need some way to control this.

What I present below is an example of a SqlConnection extension method called QuickOpen (in lack of a better name, it isn't quicker, it simply fails quicker). It'll take a timeout parameter in milliseconds, after which it'll throw a simple Exception. You can modify this to a more proper exception, this is just to show the point. Overall, using this method will introduce a slight delay (a couple of ms), so use it only when necessary, or when a couple of ms per SqlConnection.Open doesn't matter.

```csharp
public static class SqlExtensions
{
	public static void QuickOpen(this SqlConnection conn, int timeout)
	{
		// We'll use a Stopwatch here for simplicity. A comparison to a stored DateTime.Now value could also be used
		Stopwatch sw = new Stopwatch();
		bool connectSuccess = false;

		// Try to open the connection, if anything goes wrong, make sure we set connectSuccess = false
		Thread t = new Thread(delegate()
		{
			try
			{
				sw.Start();
				conn.Open();
				connectSuccess = true;
			}
			catch { }
		});

		// Make sure it's marked as a background thread so it'll get cleaned up automatically
		t.IsBackground = true;
		t.Start();

		// Keep trying to join the thread until we either succeed or the timeout value has been exceeded
		while (timeout > sw.ElapsedMilliseconds)
			if (t.Join(1))
				break;

		// If we didn't connect successfully, throw an exception
		if (!connectSuccess)
			throw new Exception("Timed out while trying to connect.");
	}
}
```
