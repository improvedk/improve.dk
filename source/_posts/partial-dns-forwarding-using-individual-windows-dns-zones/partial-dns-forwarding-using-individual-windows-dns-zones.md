permalink: partial-dns-forwarding-using-individual-windows-dns-zones
title: Partial DNS Forwarding Using Individual Windows DNS Zones
date: 2013-05-21
tags: [Windows]
---
At our office, all machines are using a local Windows DNS server for their outgoing DNS queries. This allows us to make internal zones like .ipaperlan that points to all of our internal systems, while setting up the DNS server to forward all unknown queries to Google DNS. One feature I'm missing in the standard Windows DNS server is the option to partially forward individual zones. However, there is a workaround that will allow you to setup partial DNS forwarding using individual Windows DNS zones.

<!-- more -->

## The Scenario

Imagine you have a domain *improve.dk* that already has a number of public DNS records like the following.

Capture.png

In this case all I want to do is to add a record on our internal network, *jira.improve.dk*. As this record should only be made available internally, we can't just add it to the public DNS records for the domain.

I could make a new DNS zone for the improve.dk domain in our local DNS server, but that would result in all DNS queries for improve.dk being answered by our local DNS server, rather than being forwarded. As long as I recreate all public DNS records in our local DNS server, this would work fine, but it's not a viable solution as I'd now have to keep the two DNS setups in sync manually.

## The Solution

Instead of creating a zone for the whole improve.dk domain, you can make a zone specifically for just the record you need to add. First right click "Forward Lookup Zones" and select "New Zone..." and then follow these steps (pretty much all defaults):

1.png

2.png

3.png

4.png

5.png

6.png

Now that the zone has been created, simply right click it and choose "New Host (A or AAAA)...". In the dialog, leave the Name blank as that'll affect the record itself, while entering the desired IP like so:

6_b.png

7.png

And just like that, DNS lookups for jira.improve.dk will now be answered locally while all other requests will be forwarded to whatever DNS server is set up as the forwarding server.

**One word of warning** - You might not want to do this on Active Directory domain servers as they're somewhat more finicky about their DNS setup. I'm honestly not aware of what complications might arise, so I'd advice you to be careful or perhaps find another solution.
