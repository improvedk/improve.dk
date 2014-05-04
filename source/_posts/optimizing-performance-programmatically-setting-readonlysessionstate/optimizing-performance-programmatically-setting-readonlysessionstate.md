---
permalink: optimizing-performance-programmatically-setting-readonlysessionstate
title: Optimizing Performance by Programmaticaly Setting ReadOnlySessionState
date: 2013-10-08
tags: [.NET, IIS, Performance, Web]
---
One of the main culprits when it comes to ASP.NET concurrency is caused by the fact that default sesion state has been implemented using a pessimistic locking pattern. Basically, any standard handler, whether that be an ASPX page, a generic handler or an ASMX web service, goes through the following steps:

<!-- more -->

* Retrieve & exclusively lock session
* Execute request handler
* Save & unlock updated session (whether updates have been made or not)

What this means is that, for a given session, *only one request can execute concurrently*. Any other requests, from that same session, will block, waiting for the session to be released. For the remainder of this post I'll concentrate on generic HttpHandlers, but this problem & solution is common to for ASPX and ASMX pages as well.

## Disabling Session State

If your handler doesn't require session state, all you have to do is to *not* implement the IRequiresSessionState interface, given that HttpHandlers by default do not have access to session state:

```cs
public class MyHandler : IHttpHandler
{
	public void ProcessRequest(HttpContext context)
	{
		// Perform some task
	}
	
	public bool IsReusable { get { return false; } }
}
```

By not enabling session state, no session will be locked and you can execute as many concurrent requsts as your server can handle.

## Enabling Session State

If you *do* need session state, simply implement the IRequiresSessionState interface, like so:

```cs
public class MyHandler : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		// Perform some task
	}
	
	public bool IsReusable { get { return false; } }
}
```

The IRequiresSessionState interface carries no functionality at all, it's simply a marker interface that tells the ASP.NET request pipeline to acquire session state for the given request. By implementing this interface you now have read+write access to the current session.

## Read-Only Session State

If all you need is to read session state, while not having to be able to write it, you should implement the IReadOnlySessionState interface instead, like so:

```cs
public class MyHandler : IHttpHandler, IReadOnlySessionState
{
	public void ProcessRequest(HttpContext context)
	{
		// Perform some task
	}
	
	public bool IsReusable { get { return false; } }
}
```

Implementing this interface changes the steps performed by the page slightly:

* Retrieve session, without locking
* Execute request handler
* <del>Save & unlock updated session (whether updates have been made or not)</del>

While session is still read as usual, it's just not persisted back after the request is done. This means you can actually update the session, without causing any exceptions. However, as the session is never persisted, your changes won't be saved after the request is done. For read-only use this also saves the superfluous save operation which can be costly if you're using out-of-process session state like State or SQL Server.

## Switching Between Read+Write and Read-Only Session State Programmatically

While this is great, we sometimes need something in between. Consider the following scenario:<7p>

* You've got a single handler that's heavily requested.
* On the first request you need to perform some expensive lookup to load some data that will be used in all further requests, but is session specific, and will thus be stored in session state.
* If you implement IRequiresSessionState, you can easily detect the first request (Session["MyData"] == null), load the data, store it in session and then reuse it in all subsequent requests. However, this ensures only one request may execute at a time, due to the session being exclusively locked while the handler executes.
* If you instead implement IReadOnlySessionState, you can execute as many handlers concurrently as you please, but you'll have to do that expensive data loading on each request, seeing as you can't store it in session.

Imagine if you could dynamically decide whether to implement the full read+write enabled IRequiresSessionState or just the read enabled IReadOnlySession state. That way you could implement IRequiresSessionState for the first request and just implement IReadOnlySessionState for all of the subsequent requests, once a session has been established.

And guess what, from .NET 4.0 onwards, that's possible!

## Enter HttpContext.SetSessionStateBehavior

Looking at the [ASP.NET request pipeline](http://msdn.microsoft.com/En-US/library/bb470252.aspx), session state is loaded in the "Acquire state" event. At any point, before this event, we can set the session behavior programmatically by calling HttpContext.SetSessionStateBehavior. Setting the session programmatically through HttpContext.SetSessionStateBehavior will override any interfaces implemented by the handler itself.

Here's a full example of an HttpModule that runs on each request. In the PostMapRequestHandler event (which fires just before the AcquireState event), we inspect the HttpHandler assigned to the request. If it implements the IPreferReadOnlySessionState interface (a custom marker interface), the SessionStateBehavior is set to ReadOnly, provided there already is an active session (which the presence of an ASP.NET_SessionId cookie indicates). If there is no session cookie present, or if the handler doesn't implement IPreferReadOnlySessionState, then it's left up to the handler default - that is, the implemented interface, to decide.

```cs
public class RequestHandler : IHttpModule
{
	public void Init(HttpApplication context)
	{
		context.PostMapRequestHandler += context_PostMapRequestHandler;
	}
	
	void context_PostMapRequestHandler(object sender, EventArgs e)
	{
		var context = HttpContext.Current;
		
		if (context.Handler is IPreferReadOnlySessionState)
		{
			if (context.Request.Headers["Cookie"] != null && context.Request.Headers["Cookie"].Contains("ASP.NET_SessionId="))
				context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
		}
	}
}
```

Now all we need to do is to also implement the IPreferReadOnlySessionState interface in the handlers that can do with read-only sesion state, provided a session is already present:

```cs
public interface IPreferReadOnlySessionState
{ }
```

```cs
public class MyHandler : IHttpHandler, IRequiresSessionState, IPreferReadOnlySessionState
{
	public void ProcessRequest(HttpContext context)
	{
		// Perform some task
	}
	
	public bool IsReusable { get { return false; } }
}
```

And just like that, the first request has read+write access to the session state, while all subsequent requests only have read access, greatly increasing the concurrency of the handler.
