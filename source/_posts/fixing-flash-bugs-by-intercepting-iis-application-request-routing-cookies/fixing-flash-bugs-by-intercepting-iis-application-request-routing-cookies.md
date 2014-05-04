---
permalink: fixing-flash-bugs-by-intercepting-iis-application-request-routing-cookies
title: Fixing Flash Bugs and Intercepting IIS Application Request Routing Cookies
date: 2009-12-09
tags: [.NET, AS/Flex/Flash, IIS]
---
What does Flash, upload, cookies, IIS load balancing and cookies have to do with each others? More than I'd like :(

<!-- more -->

When users need to upload files I often use the Flash based [SWFUpload](http://swfupload.org/) component. It allows for multiple file selection and progress display during upload. Handling the uploaded files on the .NET side is rather easy:

```cs
for (int i = 0; i < Request.Files.Count; i++)
{
    HttpPostedFile hpf = Request.Files[i];

    // ... Save / process the HttpPostedFile
}
```

One of the arguments for using Flash for web designs is that it'll look the same in all browsers. While that is literally true, there are a number of functionality differences when it comes to Flash and cross browser support.

There's a bug in all current Flash players that causes the Flash player to send persistent cookies from Internet Explorer, no matter what browser you're currently using. That is, if you've visited a given website in IE previously and you're nor visiting it in Chrome/Firefox - yup, your IE cookies will be sent to the website instead of the Firefox/Chrome cookies! There's a good [description and discussion](http://swfupload.org/forum/generaldiscussion/383) at the SWFUpload site.

This bug poses a number of problems if you're using SWFUpload on a password protected site that relies on [cookie based forms authentication](http://support.microsoft.com/kb/910443). Whenever the file is uploaded, the users will appear to not be logged in. This is because the forms authentication ticket is stored in a cookie (which is correctly stored by Firefox/Chrome), but whenever the request is made IE's cookies are sent and those do not contain a valid forms authentication ticket cookie.

Luckily there's a workaround for this. Basically we'll need to tell our upload SWF the current SessionID as well as the contents of the forms authentication ticket cookie:

```cs
var flashVars = {
    ASPSESSID: "<%= Session.SessionID %>",
    AUTHID: "<%= Request.Cookies[FormsAuthentication.FormsCookieName] == null ? "" : Request.Cookies[FormsAuthentication.FormsCookieName].Value %>"
}
```

Now we need to modify our SWFUpload code so it sends the SessionID and ticket values in the query string to the upload file, so instead of calling:

```cs
UploadFile_Upload.aspx
```

We'll call:

```cs
UploadFile_Upload.aspx?ASPSESSID=e2u35jfs0pvevfugkfnmm045&AUTHID=E7BA5BDD2D6E9FBBC7CF613352EF10E01E0E8B0AD9920F62A465BC0CA20FB9CC2BA67F95D5A82F5D30B3162D6DFB3EA7FD505456E5EA5407094D03C1D48E6EE0B80F85F1B6AFD5F52FDC14C2ED6D77A8
```

Now that we have the SessionID and ticket value we can manually restore those cookies in Global.asax (or an HttpModule, doesn't matter). We'll be doing the fix in Application_BeginRequest as this allows us to fix the cookies before ASP.NET will perform its validation and thereby notice the missing session and forms authentication cookies.

```cs
public class Global : HttpApplication
{
    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        fixCookie("ASP.NET_SessionId", "ASPSESSID");
        fixCookie(FormsAuthentication.FormsCookieName, "AUTHID");
    }

    private void fixCookie(string cookieName, string queryStringKey)
    {
        // Did we get a querystring value to override the cookie value?
        if (HttpContext.Current.Request.QueryString[queryStringKey] != null)
        {
            // Try to get the current cookie value
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(cookieName);

            if (cookie == null)
            {
                /* If there's no cookie, add a new one and add it to the Response.Cookies collection.
                   Note that it HAS to be put in the Response.Cookies collection even though Request.Cookies
                   makes more sense.
                */ 
                cookie = new HttpCookie(cookieName, HttpContext.Current.Request.QueryString[queryStringKey]);
                Response.Cookies.Add(cookie);
            }
            else
            {
                /* If there's already a cookie (one from IE perhaps), overwrite its value with the querystring
                   provided value.
                */
                cookie.Value = HttpContext.Current.Request.QueryString[queryStringKey];
                HttpContext.Current.Request.Cookies.Set(cookie);
            }
        }
    }
}
```

Note that there is a security implication in doing this as it allows for session hijacking if you're able to fake another users SessionID and forms authentication ticket! Thus, make sure you handle this or at least know the risks in not doing so.

OK, so that fixes the SWFUpload issue. This ran perfectly for some time. However, once i placed an [IIS7 Application Request Routing based load balancer](http://forums.iis.net/1154.aspx) in front of the machine serving the upload applications, the issue from before reappeared, even though my original cookie handling code was still in place.

The reason for the resurrection of the cookie bug was to be found in the way ARR maintains client affinity:

arrswfupload_1_2.jpg

IIS ARR will set a cookie on the client that basically contains a hash of the content server to which the client is bound. This is a very simple and neat client affinity solution as there's no shared state on the IIS ARR machine itself. Thus, it's easy to combine a number of IIS ARR servers using NLB and let IIS ARR handle client affinity and thus simplify the NLB setup.

However, since the client affinity is handled by a cookie - that cookie was now suffering from the same bug as before. Basicaly the IIS ARR load balancer thought it received a completely new client request and thus assigned the request to a random content server, giving a 1/[num_machines] chance of succeeding in case it randomly hit the correct content server.

The solution is similar, though there is one major difference. The previous problem occurred on the actual content machines because those were missing a cookie value, in this case it's the load balancer itself. Thus, deploying a fix on the content servers won't do any good.

We'll create a new HttpModule that performs the fix in Application_BeginRequest - which occurs before IIS ARR assigns the request to a content server. To ensure this fix does not in any way affect normal requests in case something goes wrong, exceptions are being silently ignored. This is generally a bad practice, but in this case I really do not want to affect the load balancer as that'll put down the website for all users if an error occurs. Note that while the handling is very similar to the previous bit of code, this time we're modifying the actual Cookie header directly. If we don't do this, IIS ARR won't pick up the overwritten cookie values and thus still send the user to a random content server.

```cs
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace iPaper.Web.ArrCookieRestorer
{
    public class ArrCookieRestorer : IHttpModule
    {
        public void Dispose()
        { }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
        }

        private void context_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                HttpContext context = HttpContext.Current;
                string serverHash = context.Request.QueryString["ARRIPARRAffinity"];

                if (serverHash != null)
                {
                    string cookieHeader = context.Request.Headers["Cookie"];

                    if (cookieHeader != null)
                    {
                        if (cookieHeader.Contains("IPARRAffinity="))
                            cookieHeader = Regex.Replace(cookieHeader, "IPARRAffinity=[0-9a-f]+;?", "IPARRAffinity=" + serverHash + ";");
                        else
                            cookieHeader += "; IPARRAffinity=" + serverHash;

                        context.Request.Headers["Cookie"] = cookieHeader;
                    }
                    else
                        context.Request.Headers.Add("Cookie", "IPARRAffinity=" + serverHash);
                }
            }
            catch
            {}
        }
    }
}
```

Once you've compiled the HttpModule we need to install it on the IIS ARR machine. On a default installation of IIS ARR you'll have your rewrite rules as global rules at the IIS-level. However, if you install the HttpModule at the IIS level you'll get the following exception on all requests:

> The virtual path 'null' maps to another application, which is not allowed root

Apparently [it's a bug](http://forums.iis.net/t/1162754.aspx) in IIS 7.0 on Windows Server 2008 which has been fixed in IIS 7.5 on Windows Server 2008 R2. As I'm still running a vanilla 2008 and IIS 7.0, I had to get around it by moving the rewrite rules into the default website - which code runs for all requests.

Make sure there's a bin folder in the default website root and place your HttpModule in there. Then setup your web.config like so:

```xml
<?xml version="1.0" encoding="UTF-8"?>
```

This adds our HttpModule so it'll run for all requests - fixing any missing ARR client affinity cookies. Note that your rewrite rules will likely differ from mine.
