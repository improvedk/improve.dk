permalink: setting-up-multiple-iis-application-request-routing-farms-on-the-same-server
title: Setting up Multiple IIS Application Request Routing Farms on the Same Server
date: 2010-06-16
tags: [IIS]
---
Inspired by a [post on the ARR forums](http://forums.iis.net/t/1168684.aspx), let me walk you through how to setup multiple [IIS Application Request Routing](http://www.iis.net/download/applicationrequestrouting) server farms on a single machine.

## The scenario

I own a bunch of search engines, [bing.com](http://bing.com/), [google.com](http://google.com/) and [yahoo.com](http://yahoo.com/). Likewise, I own a bunch of news sites [cnn.com](http://cnn.com/), [english.aljazeera.net](http://english.aljazeera.net/) and [foxnews.com](http://foxnews.com/)(alright, bear with me on that last one, just using it as an example). Luckily, my users don't really care which search engine or news site they go to, as long as they end up at "a" search engine or "a" news site. To help me distribute the load we'll setup two IIS ARR web farms that'll load balance between the news sites and search engines.

## Setting up the farms

Start by [installing the Application Request Routing module](http://learn.iis.net/page.aspx/482/install-application-request-routing/) if you haven't done so already. Now create two sever farms called "News" and "Search". Add the search &amp; news websites to each of their server farms like so:

sumiarrfarms_1_2.jpg

You may also setup the farms directly in applicationHost.config:

<pre lang="xml"><webfarms>
    <webfarm enabled="true" name="News">
        <server enabled="true" address="cnn.com">
        <server enabled="true" address="foxnews.com">
        <server enabled="true" address="english.aljazeera.net">
    </server></server></server></webfarm>
    <webfarm enabled="true" name="Search">
        <server enabled="true" address="google.com">
        <server enabled="true" address="bing.com">
        <server enabled="true" address="yahoo.com">
    </server></server></server></webfarm>
    <applicationrequestrouting>
        <hostaffinityproviderlist>
            <add name="Microsoft.Web.Arr.HostNameRoundRobin">
            <add name="Microsoft.Web.Arr.HostNameMemory">
        </add></add></hostaffinityproviderlist>
    </applicationrequestrouting>
</webfarms></pre>

## Setting up the rules

Go to the IIS level "URL Rewrite" settings page. If any default rules were created in the installation of IIS ARR, delete them now. Create a new rule from the blank template and call it "search.local". Set the Match URL to use Wildcards and enter a pattern of "*". This ensures that the rule matches and URL that we send to the server - no matter the hostname.

sumiarrfarms_2_2.jpg

Expand the Conditions pane and add a new condition. Set the input to "{HTTP_HOST}" and type the pattern "search.local". The {HTTP_HOST} condition ensures that this rule only matches requests to the search.local hostname.

sumiarrfarms_3_2.jpg

Finally set the action type to "Route to Server Farm" and choose the Search server farm. Tick the "Stop processing of subsequent rules" as the request should be routed onto the server farm as soon as this rule matches.

sumiarrfarms_4_2.jpg

As the last step, create a second identical rule except it should be named news.local and have an {HTTP_POST} condition matching the news.local hostname. Setting up the rewrite rules can also be done in applicaitonHOst.config directly:

<pre lang="xml"><rewrite>
    <globalrules>
        <rule name="search.local" stopprocessing="true" patternsyntax="Wildcard">
            <match url="*">
            <action url="http://Search/{R:0}" type="Rewrite">
            <conditions>
                <add pattern="search.local" input="{HTTP_HOST}">
            </add></conditions>
        </action></match></rule>
        <rule name="news.local" stopprocessing="true" patternsyntax="Wildcard">
            <match url="*">
            <action url="http://News/{R:0}" type="Rewrite">
            <conditions>
                <add pattern="news.local" input="{HTTP_HOST}">
            </add></conditions>
        </action></match></rule>
    </globalrules>
</rewrite></pre>

Once done, your rules should look like this:

sumiarrfarms_5_2.jpg

## Testing the farms

Create two host entries in your %windir%System32Driversetchosts file that makes news.local and search.local point to 127.0.0.1. Now when you open a browser and enter news.local or search.local, you should hit the search engines and news sites we setup earlier. By default IIS ARR will distribute the requests to the server with the least current requests - and since we're the only ones hitting them we'll basically always just hit the first server. To alleviate this you can change the load distribution mode to a weighted round robin with even distribution, that'll ensure you hit the sites one by one in turn.

sumiarrfarms_6_2.jpg

Note that this'll only load CNN and Bing correctly as several of the other domains only listen to their configured domains (IIS ARR will proxy the reuqest onto the origin servers with a hostname request of search &amp; news.local respectfully). I'll follow up on how to fix that later on.
