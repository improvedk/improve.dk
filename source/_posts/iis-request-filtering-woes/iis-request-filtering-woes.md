permalink: iis-request-filtering-woes
title: IIS Request Filtering Woes
date: 2009-09-23
tags: [IIS]
---
I recently put a number of load balanced websites in production by using the newly released [IIS7 Application Request Routing](http://www.iis.net/extensions/ApplicationRequestRouting) v2 Beta extension. Everything seemed to run perfectly both performance and functionality wise. There was a slight problem however.

<!-- more -->

Some users were reporting mysterious errors when uploading files to the website, apparently seeming like a timeout. When I tried to reproduce, all smallish files when through, though larger files did fail. I checked out the responses in [Fiddler](http://www.fiddler2.com/fiddler2/) and to my surprise the ones working returned 200 while the failing ones returned a 404 error after a while. To the trained eye, the problem might already be apparent - unfortunately it wasn't apparent to me at the time. I'd expect a status 200 for working uploads and a 500 for failed uploads. A 404 should only happen when the URL is wrong, which certainly shouldn't vary depending on file size.

Circumventing the ARR load balancing server fixed the issue, so I quickly pinpointed that the addition of the ARR load balancer was the root cause. Enabling IIS logging on the content servers revealed that the failing requests never reached the content servers, hinting that the actual problem occurred on the ARR machine before even being proxied on to the content servers.

Checking out the IIS log of the ARR server revealed the following crucial line (unimportant parts abbreviated):

```
[DATETIME] [USER_IP] GET / - 80 - [USER_IP] [UserAgent] 404 13 0 1
```

The HTTP status code is 404 as shown by Fiddler. The interesting part however is the HTTP substatus code of 13. Checking up on the HTTP substatus codes utilized by the [IIS7 Request Filtering module](http://www.iis.net/ConfigReference/system.webServer/security/requestFiltering) revals that 404.13 is caused by a too large content length. If the ARR IIS had spat out a detailed IIS error page instead of a generic 404, the problem would have been apparent much quicker since the substatus code would've been included. Unfortunately the detailed errors are disabled on the ARR ISS for security reasons.

The solution is simple. By opening the [C:WindowsSystem32inetsrvconfigapplicationHost.config](http://learn.iis.net/page.aspx/124/introduction-to-applicationhostconfig/) (the main IIS configuration file) and setting the [maxAllowedContentLength](http://msdn.microsoft.com/en-us/library/ms689462.aspx)
in system.webServer/security/requestFiltering/requestLimits to a higher value, we automatically allow larger bodies for incoming requests and thus avoiding the 404.13 error caused by the request filtering module. In the below example I've set the limit to 256 MB - the value is expressed in bytes.

```xml
<system.webServer>
	<security>
		<requestFiltering>
			<requestLimits maxAllowedContentLength="268435456" />
		</requestFiltering>
	</security>
</system.webServer>
```

Tip: Instead of editing the applicationHost.config file manually you can also install the [IIS Admin Pack Tech Preview 2](http://blogs.msdn.com/carlosag/archive/2008/05/13/IISAdminPackTechnicalPreview2Released.aspx) which will give you the option to edit request filtering settings directly from the IIS Manager, as well as a number of other management GUI improvements.
