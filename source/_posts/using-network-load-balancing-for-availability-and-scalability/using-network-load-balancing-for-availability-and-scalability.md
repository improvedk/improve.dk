---
permalink: using-network-load-balancing-for-availability-and-scalability
title: Using Network Load Balancing for Availability & Scalability
date: 2008-03-08
tags: [.NET]
---
There are two primary reasons for venturing into the realms of clustering/load balancing - availability & scalability. In this post I'll give a quick demo of how to setup Windows Network Load Balancing (NLB) on Server 2003 and how it affects the availability of a web application.

<!-- more -->

When we have several nodes doing the same thing, if one of them fails, the cluster as a whole continues - provided that the nodes are not so overburdened that a single node failing will kill the others due to the extra load. Most applications will have an upper limit on how much it can scale on a single box. You can get very far by vertically scaling your solution (buying faster & usally exponentially more expensive hardware), but true scalability always comes in the form of horizontal scaling. Add another box and receive more or less linear return of investment in regards of added computing power.

NLB enables us to easily add new nodes to a cluster and thus letting them share the workload. There are several issues to consider before setting up a cluster for a real application. Does the application share user file data? - in such a case you'll have to make sure all nodes in the cluster have access to those files, and remember - the cluster is no stronger than the weakest link. Usually you'd probably go for a [SAN](http://en.wikipedia.org/wiki/Storage_area_network) to store all common files. State is also an important factor as most web applications rely on storing user specific data in statebags like the Session object. When the load is balanced out on several servers, the user could potentially visit several servers during his visit to the website, and unless the state storage is centralized, the user will have different session data on each server. Again, remember that you'll have to provide a redundant storage location for the session data, or else you'll compromise the availability of the cluster.

In this demonstration I'll be using two virtual PCs running on my own host computer. All three computers are running on the subnet 192.168.0.X. Here are the virtual machines involved:

```
192.168.0.34 - VENETIAN
192.168.0.32 – MIRAGE
```

When you setup an NLB cluster, you create a new virtual IP address that gets mapped to each individual server, besides their own static IP address. In this demo I'll setup the cluster on the virtual IP address 192.168.0.50. Before we get too far, I should mention that if you are going to setup an NLB cluster in a production environment, you should use machines with dual NICs so one NIC can connect to the public lan while the other NIC connects to a private switched lan where only the cluster nodes are connected. This ensures that the internal cluster communication is not polluting the general network traffic, aswell as making it a lot more efficient since we'll then be able to utilize [unicast communciation](http://en.wikipedia.org/wiki/Unicast) between the nodes instead of relying on [multicast communication](http://en.wikipedia.org/wiki/Multicast).

For demonstrating the effects of a node crash, I've created a very simple load testing tool, the main functionality is an infinite loop trying to make a request at a time while registering success/failures. It'll also show the text result that is returned in a one-line textbox:

```cs
while (running)
{
	HttpWebRequest req = (HttpWebRequest)WebRequest.Create(txtUrl.Text);
	req.Timeout = 1000;
	req.KeepAlive = false;

	try
	{
		WebResponse res = req.GetResponse();
		using (StreamReader sr = new StreamReader(res.GetResponseStream()))
			lastResult = sr.ReadToEnd();

		successfulRequests++;
	}
	catch (WebException)
	{
		failedRequests++;
	}
}
```

As I'm running this test on two VPCs on my local machine, I won't be getting any extra performance out of my cluster since the VPCs will just use 50% CPU each. This demo will concentrate on the availability of the cluster - but feel assured that performance will be better if you distribute the cluster over several separate computers.

Here's the demo of how to setup & use NLB:


{% youtube 32uWPp8PpUA %}


## Other good resources

Rick Strahl on [Web Farming with the Network Load Balancing Service in Windows Server 2003](http://www.west-wind.com/presentations/loadbalancing/NetworkLoadBalancingWindows2003.asp)

Peter A. Bromberg on [Network Load Balancing, Session State and IP Affinity](http://www.eggheadcafe.com/articles/20020302.asp)

## Downloads

[SimpleLoadTester.zip - Sample code](SimpleLoadTester.zip)