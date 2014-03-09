permalink: iis7-the-object-identifier-does-not-represent-a-valid-object
title: The Object Identifier Does Not Represent a Valid Object
date: 2009-07-08
tags: [.NET, IIS]
---
When adding sites to IIS7 either by script or by editing the config files directly, you may receive an error in the sites list that says:

<blockquote>Unknown: The object identifier does not represent a valid object. (Exception from HRESULT: 0x800710D8)</blockquote>

iisvalidobject_2.jpg

I'm running multiple servers in an NLB setup, using the shared configuration feature of IIS7 (config files are stored on a SAN exposed through a CIFS share). My first thoughts were that this was probably related to the shared configuration / network access, but I'm able to reproduce the problem even with location configuration. An interesting observation is that all IIS's on all servers using the shared config will display the same errors on the same sites.

Restarting the sites, app pool or even IIS does not help on the issue, neither does restarting the IIS Manager itself. I have not tried restarting the servers, and I'm not going to.

The only relevant info I could find on Google was [this thread](http://forums.iis.net/t/1151841.aspx) on forums.iis.net. Besides various mutations of the restart option, the thread mentions it might be a permissions issue. I'm running IIS Manager under an administrative account and all application pools run under processes with access to the configuration directory. Running Process Monitor confirms that there are no permission issues.

What I've found since then is that a simple file tocuh will fix the problem. That is, if I open the file in notepad and make/undo a change and save the file, all IIS's will reload the configuration and all sites will have loaded correctly on all servers. Using Process Monitor I've verified that change notifications are being sent out to all servers, thereby notifying them of an update to the configuration, causing the reload in IIS. The aforementioned thread does note that IIS will basically start a timer with an interval of a few ms before it'll update the site list of a config change - if reading the config file takes longer than this timeout, we may be in trouble. However, this issue should be fixed by a refresh and most certainly by a restart of the IIS. Also, since this is happening on all servers, it would seem weird that they all exceed the timeout.

To add confusion, this bug is temporal. If I add three sites programmaticaly (three transactions - they're not committed all at once, but in succession) using code like this:

```csharp
using (ServerManager mgr = new ServerManager())
{
	if (mgr.Sites[name] != null)
		mgr.Sites.Remove(mgr.Sites[name]);

	// Create site
	Site site = mgr.Sites.Add(name, "http", "*:80:" + domain, physicalPath);

	// Set app pool
	foreach (var app in site.Applications)
		app.ApplicationPoolName = pool;

	// Add extra bindings
	foreach (string hostname in extraBindings)
		site.Bindings.Add("*:80:" + hostname, "http");

	mgr.CommitChanges();
}
```

Sometimes all sites work, sometimes 1-3 of them don't. Usually two of them will have failed while one will be working. Running the exact code again will fix the problem. The code is made so it'll just recreate a website if it already exists and thus running the code again will basically just touch the file, causing reloads of the config files across the servers. If I try to access the sites programmatically, I'll receive the same exception as is displayed in the IIS Manager and as a result I might actually be able to detect this issue and just keep retrying until all sites work.

As an alternative, I've also tried just generating the complete applicationHost.config file from scratch so that all changes are definitely made at the same time. By creating the applicationHost.config file separately and then replacing the old one, I don't get the "valid object" error any more. However, a random number of websites &amp; pools will be in the "stopped" state for no apparent reason. All sites &amp; pools have the auto start attributes set to true. I can start the sites manully without problems, it's not a good solution though seeing as there's hundreds of sites and it'll take a considerably amount of time to start half of them manually. Fortunately that part is easy to script:

```csharp
using (ServerManager mgr = new ServerManager())
{
	// Start all app pools
	foreach (var pool in mgr.ApplicationPools)
		pool.Start();

	log("Pools started - Done");

	// Start all sites
	foreach (var site in mgr.Sites)
		site.Start();

	log("Sites started - Done");
}
```

Anyone else experiencing this issue and have found the cause? Or do you have a better workaround than what I'm doing?
