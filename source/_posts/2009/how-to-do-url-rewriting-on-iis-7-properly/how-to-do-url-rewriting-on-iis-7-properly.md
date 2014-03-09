permalink: how-to-do-url-rewriting-on-iis-7-properly
title: How To Do URL Rewriting on IIS 7 Properly
date: 2009-10-14
tags: [.NET, IIS]
---
One of my earlier blog posts, and the all time most popular one, was about [how to make URL rewriting on IIS 7 work like IIS 6](http://improve.dk/blog/2006/12/11/making-url-rewriting-on-iis7-work-like-iis6). While my method did provide a means to the goal, it's humiliatingly far from what I should've done. Since the old post is still the most visited post on my blog I feel obligated to write a followup on how to do proper url rewriting in IIS 7.

## The scenario

I'll assume a completely vanilla IIS 7 setup, contrary to the old post, there's no IIS tampering required.

I've setup a simple web application solution structure like so:

iis7urlrewritingdoneproperly_1_2.jpg

As in the original post my goal is to accept a URL like http://localhost/blog/2006/12/08/missing-windows-mobile-device-center and map it to the BlogPost.aspx file in the root of my application. During the rewrite process I want to make the year, month, day and title available for the BlogPost.aspx file in an easily accessible way.

## Rewriting using Global.asax

The easiest way of rewriting URL's is to add a new Global.asax file to the root of your solution. Now paste in the following code:

```csharp
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace IIS7UrlRewritingDoneProperly
{
	public class Global : HttpApplication
	{
		// Runs at the beginning of each request to the server
		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			// Match the specific blog post URL path as well as pull out variables in regex groups
			Match m = Regex.Match(Request.Url.LocalPath, @"^/blog/(?<year>d{4})/(?<month>d{2})/(?<day>d{2})/(?<title>.*)/?$");

			// If we match a blog posts URL, save the URL variables in Context.Items and rewrite to /BlogPost.aspx
			if (m.Success)
			{
				Context.Items["Title"] = m.Groups["title"].Value;
				Context.Items["Year"] = m.Groups["year"].Value;
				Context.Items["Month"] = m.Groups["month"].Value;
				Context.Items["Day"] = m.Groups["day"].Value;

				HttpContext.Current.RewritePath("/BlogPost.aspx");
			}
		}
	}
}
```

Now all you need is a single change in your web.config file:

<pre lang="xml" escaped="true">&lt;configuration&gt;
	&lt;system.webServer&gt;
		&lt;modules runAllManagedModulesForAllRequests="true"&gt;
	&lt;/system.webServer&gt;
&lt;/configuration&gt;</pre>

The web.config change basically does the same as adding the wildcard map in IIS6. It ensures ASP.NET will run our Application_BeginRequest function for all requests - both those that match .aspx files as well as those for static files.

## Rewriting using an HttpModule

As an alternative to putting the rewriting logic into Global.asax, you might want to write it into a distributable HttpModule. If your URL rewriting functionality is common for multiple sites, generic or for any other reason may be usable on multiple sites, we don't want to replicate the functionality in Global.asax.

If you added the Global.asax file from before, make sure you remove it again so it doesn't conflict with the HttpModule we're about to write. Add a new class project to the solution - I've called mine MyUrlRewriter. Add a reference to System.Web and add a single new class file to the project called UrlRewriter. Your solution should look like this:

iis7urlrewritingdoneproperly_2_2.jpg

Now paste the following code into the UrlRewriter.cs class file:

```csharp
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace MyUrlRewriter
{
	public class UrlRewriter : IHttpModule
	{
		// We've got nothing to dispose in this module
		public void Dispose()
		{ }

		// In here we can hook up to any of the ASP.NET events we use in Global.asax
		public void Init(HttpApplication context)
		{
			context.BeginRequest += new EventHandler(context_BeginRequest);
		}

		// This method does exactly the same as in Global.asax
		private void context_BeginRequest(object sender, EventArgs e)
		{
			// Match the specific blog post URL path as well as pull out variables in regex groups
			Match m = Regex.Match(HttpContext.Current.Request.Url.LocalPath, @"^/blog/(?<year>d{4})/(?<month>d{2})/(?<day>d{2})/(?<title>.*)/?$");

			// If we match a blog posts URL, save the URL variables in Context.Items and rewrite to /BlogPost.aspx
			if (m.Success)
			{
				HttpContext.Current.Items["Title"] = m.Groups["title"].Value;
				HttpContext.Current.Items["Year"] = m.Groups["year"].Value;
				HttpContext.Current.Items["Month"] = m.Groups["month"].Value;
				HttpContext.Current.Items["Day"] = m.Groups["day"].Value;

				HttpContext.Current.RewritePath("/BlogPost.aspx");
			}
		}
	}
}
```

Notice that the context_BeginRequest function is identical to the one we had in Global.asax, except we have to reference HttpContext.Current explicitly since it's not implicitly available as in Global.asax.

Now add a reference from the original web application project to the MyUrlRewriter class project. Once this is done we just need to ensure our HttpModule is included in our web application by modifying the web.config:

<pre lang="xml" escaped="true">&lt;configuration&gt;
    &lt;system.webServer&gt;
        &lt;modules runAllManagedModulesForAllRequests="true"&gt;
            &lt;add name="UrlRewriter" type="MyUrlRewriter.UrlRewriter, MyUrlRewriter"/&gt;
        &lt;/modules&gt;
    &lt;/system.webServer&gt;
&lt;/configuration&gt;</pre>

At this point you should be able to run the website with the exact same URL rewriting functionality as we had before - though this time in a redistributable assembly called MyUrlRewriter.dll which can easily be included into any website by adding a single line to the section of the web.config file.

## Not Invented Here Syndrome

If you have basic requirements to your URL rewriting solution you may often be able to settle with one of the many readymade HttpModules that you can simply plug into your application. IIS 7 also has a [URL Rewrite Module](http://learn.iis.net/page.aspx/461/creating-rewrite-rules-for-the-url-rewrite-module/) that you can install and easily configure through the IIS manager.
