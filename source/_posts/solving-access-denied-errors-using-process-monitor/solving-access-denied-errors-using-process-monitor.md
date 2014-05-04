---
permalink: solving-access-denied-errors-using-process-monitor
title: Solving Access Denied Errors Using Process Monitor
date: 2009-10-21
tags: [.NET, IIS]
---
[Access](http://stackoverflow.com/questions/809144/could-not-load-file-or-assembly-someproject-or-one-of-its-dependencies-access) [denied](http://stackoverflow.com/questions/574411/system-error-5-access-is-denied-when-starting-a-net-service) [errors](http://stackoverflow.com/questions/951259/could-not-load-file-or-assembly-or-one-of-its-dependencies-access-is-denied) are not [uncommon](http://stackoverflow.com/questions/1166490/could-not-load-file-or-assembly-applicenses-or-one-of-its-dependencies-access) when deploying new websites / features that interact with the filesystem. While it might work in local testing, it suddenly doesn't anymore when deployed. Using [Process Monitor](http://technet.microsoft.com/en-us/sysinternals/bb896645.aspx) I'll show how to easily debug these issues.

<!-- more -->

I've made a very simple web application project with a Default.aspx file that has the following codebehind code:

```cs
using System;
using System.IO;
using System.Web.UI;

namespace FileWritingWebsite
{
	public partial class _Default : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			File.WriteAllText(@"C:\Test.txt", "Hello world!");

			Response.Write("Done!");
		}
	}
}
```

After deploying this to my webserver we receive the archetypical access denied error:

procmon1_2.jpg

In this case it's rather obvious where the error stems from, but the cause isn't as obvious. We're running under IIS, but we may be impersonating a user profile, running under a non-standard user account for the application pool (that is, not NETWORK SERVICE) or explicitly writing the file on a thread that's running on a different user account (which we are not in this case, however).

procmon2_2.jpg

Looking at the user permissions for C: it's clear that no special permissions have been granted for the web user. Thus our task is first and foremost to identify the user that's trying to write the file.

procmon3_2.jpg

Once you startup Process Monitor you'll quickly be swamped with input data that's irrelevant to the task at hand. The first filter we'll apply is the overall event type filter. There's five standard types, of which the first four are enabled by default: Registry, File, Network, Process & Threads and Profiling. As we're having an access denied issue with the file system, disable all but the File System events.

At this point the number of events should already be filtered down a lot - down to 32% in my case. Now click the cyan funnel icon to open up the filter editor window.

procmon4_2.jpg

Since we know IIS is running under the w3wp.exe process, we can add a filter that includes all events with a process name of w3wp.exe. As soon as we add an Include filter, all event that do not match an include filter are excluded.

procmon5_2.jpg

At this point th event list is somewhat more manageable. The important event is clearly the one with a result of "ACCESS DENIED". That event shows we're trying to write (CreateFile) the C:Test.txt file and we're receving an ACCESS DENIED error from the file system.

procmon6_2.jpg

Now right click the ACCESS DENIED event and go to Properties. Once you've opened the properties window, switch to the Process tab. At this point you'll be ableto see the exact user account that tried to perform the denied action. As can be seen from the screenshot, it was the NETWORK SERVICE user in this case - the default IIS user.

procmon7_2.jpg

Once we've identified the necessary user account, it's a simple matter of granting it NTFS write rights to the C: directory.

procmon8_2.jpg

And finally we can run the website again and verify that we've now got proper permissions for writing the Test.txt file to the C: directory.

## Not just for web applications

While I gave an example of a web application security issue, Process Monitor can be used to solve any kind of permission issues. You can use it for debugging why Windows Services won't start properly, why Outlook is suddenly complaining about access denied issues etc. Note that Process Monitor will also allow you to monitor the registry and can thus be used to solve security issues just as simple as with the file system.

Process Monitor is also a great tool for monitoring 3rd party applications to discover their exact usage of the file system and registry.
