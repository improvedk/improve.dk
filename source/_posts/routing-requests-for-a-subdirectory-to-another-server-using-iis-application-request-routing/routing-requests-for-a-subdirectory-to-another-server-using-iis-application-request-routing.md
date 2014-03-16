permalink: routing-requests-for-a-subdirectory-to-another-server-using-iis-application-request-routing
title: Routing Requests for a Subdirectory to Another Server Using IIS Application Request Routing
date: 2010-06-21
tags: [IIS]
---
In this post I'll walk you through how to setup [IIS Application Request Routing](http://www.iis.net/download/applicationrequestrouting) so that any requests for a /wiki/ subdirectory are routed onto a separate server, while all other requests are handled by the server itself.

<!-- more -->

Let's imagine a fictional scenario where I want to add a wiki to my website. Thus, all requests to improve.dk/wiki/* are mapped to a dedicated LAMP based server that runs some kind of wiki software. All other requests should be served by the normal improve.dk webserver.

The first task is to setup a new server farm, called Wiki in my case. Add the server to the list of servers, using its hostname, MyWikiServer in this case. If you setup (temporarily, for testing) improve.dk so it maps to the wiki server, requesting http://improve.dk/Wiki/ should return back the expected result from the wiki server, if requested from the normal webserver.

rrsa1_2.jpg

```xml
<webfarms>
    <webfarm enabled="true" name="Wiki">
        <server enabled="true" address="MyWikiServer">
    </server></webfarm>
    <applicationrequestrouting>
        <hostaffinityproviderlist>
            <add name="Microsoft.Web.Arr.HostNameRoundRobin">
            <add name="Microsoft.Web.Arr.HostNameMemory">
        </add></add></hostaffinityproviderlist>
    </applicationrequestrouting>
</webfarms>
```

Next, setup a URL Rewrite rule at the global level like the following:

rrsa2_2.jpg

The rule matches all requests for the /wiki/ directory, whether there's a trailing slash or further subdirectories. It ensures not to match a request like /wikipedia/. The request is routed onto the Wiki webfarm which sends the request onto the MyWikiServer server. Note that there's a condition ensuring this rule will only match requests for the improve.dk domain so other websites aren't affected. There are no changes to the actual improve.dk website setup.

**Correction:** As noted by Rob Hudson, the regular expression should actually be:

> ^wiki$|^wiki/(.\*)
