permalink: spawning-threads-in-aspnet-can-be-dangerous
title: Spawning Threads in ASP.NET Can Be Dangerous
date: 2008-04-07
tags: [.NET]
---
In my [earlier blog post](http://improve.dk/blog/2008/03/29/response-transmitfile-close-will-kill-your-application) about the dangers of using Response.TransmitFile, I gave an example of a workaround involving spawning a new thread in the ASP.NET page. While this does solve the issue at hand, it presents us with a new way to kill our application even quicker than last.

<!-- more -->

Usually when an uncaught exception occurs in an ASP.NET application, we will be presented with a "friendly" error message like the one below:

caughtexception.jpg

While there is an overhead of exceptions being thrown, they're not directly dangerous and will at worst affect scalability (ignoring the actual reason of the exception being thrown). The problem is that ASP.NET will *only catch exceptions on the processing thread*. That means, if you spawn a new thread and an exception is thrown (and is not caught inside the thread itself), it will propagate and eventually *crash the w3wp.exe process*.

### Safe

```csharp
protected void Page_Load(object sender, EventArgs e)
{
	Response.Write("CAN HAZ W3WP.EXE?");

	throw new Exception("I'll will be caught by ASP.NET :D");
}
```

### Unsafe, will crash w3wp.exe

```csharp
protected void Page_Load(object sender, EventArgs e)
{
	Response.Write("CAN HAZ W3WP.EXE?");

	new Thread(delegate()
	{
		throw new Exception("I'll not be caught by ASP.NET :(");
	}).Start();
}
```

There are several repercussions of the w3wp.exe crashing. There's a major overhead in spawning a new w3wp.exe process on the next request, you will loose all session (if you're using inprocess session storage), application and cache state. If you have error reporting turned on, you furthermore also see the "DW20.exe" process running and taking up 100% CPU for a significant amount of time (depending on w3wp.exe memory usage, etc) - if this happens often, you might have a large amount of DW20.exe error reporting processes running, effectively crashing your server.

So how do we avoid this? Simple, make sure *all code* in spawned threads is handling exceptions:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
	Response.Write("CAN HAZ W3WP.EXE?");

	new Thread(delegate()
	{
		try
		{
			throw new Exception("I'll be caught by our own exception handler :)");
		}
		catch
		{
			Response.Write("What doesn't kill me will make me stronger!");
		}
	}).Start();
}
```

If you're experiencing this issue, you will see errors in the System event log like this one:

> A process serving application pool 'DefaultAppPool' terminated unexpectedly. The process id was '708'. THe process exit code was '0xe0434f4d'.

And like this one in the Application log:

> EventType clr20r3, P1 w3wp.exe, P2 6.0.3790.3959, P3 45d6968e, P4 crashw3wp, P5 1.0.0.0, P6 47f94ca4, P7 3, P8 b, P9 system.exception, P10 NIL.

[Tess](http://blogs.msdn.com/tess) has a really great post on [how to debug an unknown cause of the crash](http://blogs.msdn.com/tess/archive/2006/04/27/584927.aspx).

This issue is relevant to all flavors of Windows and all versions of IIS & .NET.

## Downloads

[CrashW3WP.zip - Sample code](http://improve.dk/wp-content/uploads/2008/04/CrashW3WP.zip)
