permalink: live-blogging-from-the-2011-pass-summit-day-2-keynote
title: Live Blogging From the 2011 PASS Summit Day 2 Keynote
date: 2011-10-13
tags: [Conferences & Presenting]
---
It's 8:15 AM and I'm back at the bloggers table, ready for the day 2 keynote. The format will be the same as [yesterday](/live-blogging-from-the-2011-pass-summit-day-1-keynote).

<!-- more -->

On a side note – I'm surprised at how comfortable wearing a kilt is.

**[8:18]  **Bill Graziano is on stage, cheering at all those of us wearing #sqlkilts. Next up applauding all the volunteers, speakers, chapter leaders, etc. Thank you!

**[8:23]  **Lori Edwards is awarded the 2011 PASSion award, well deserved!

**[8:27]  **Video on stage praising new SQL Server 2012 through personal testimonials. Looking forward to hear about the new features, hopefully non-BI today.

**[8:30]  **Quentin Clark (Corporate VP, SQL Server, Microsoft) on stage. Using the same lovely Metro based slidedeck that we saw yesterday.

**[8:31]  **Urges us to stop using the word Denali and instead use SQL Server 2012. Lots of hardware & appliances on stage with Quentin – appliances will probably be a big topic, just as it was at SQLBits.

**[8:32]  **SQL Azure powered by SQL Server 2012 at this point. 180+ new features, can't showcase all, we'll see top 12.

**[8:35]  **Required 9s & Protection: SSIS as a server, HA for StreamInsight, SQL Server AlwaysON – HA/DR being the main keyword.

**[8:36]  **Bob Erickson (Executive VP of Interlink Transport Technologies) on stage telling about their experiences with SQL Server 2012 Mission Critical deployment. Lots of fancy buzz words, no technical details or practical details.

**[8:41]**  AlwaysON being demoed live on stage. Unfortunately fonts and UI is so small, even with scarse zoomit usage, that noone can see what actually goes on.

**[8:44]  **Zoomiit used on stage, bloggers table goes wild in applause.

**[8:45]  **AlwaysON demo is cool and all, but this is the same stuff we've seen at multiple presentations already.

**[8:47]  **Quentin has moved onto the next topic – Blazing Fast Performance.

**[8:49]  **Performance enhancements in RDBMs engine, SSAS, SSIS, ColumnStore indexes (previously known as Apollo). ColumnStore indexes are seriously interesting. Any kind of demo would've been cool.

**[8:50]  **Quentin moving onto next two topics – Rapid Data Exploration & Managed Self-Service BI. In other words, a rehash of what we saw yesterday. It looks cool, most DBA's I've talked with are somewhat reluctant to letting their users get access to self-service BI.

**[8:51]  **Credible Consistent Data. Lots of buzz words, hard to extract any practical details.

**[8:53]  **Quentin welcoming Lara Rubbelke on stage – taking a jab at the Excel demos done yesterday. Demo starts out with Sharepoint interface with miniscule fonts – as usual, noone can see what's happening. Sounds like this is going into ColumnStore demo.

**[8:56]  **Lara creating a ColumnStore index live on stage, using Zoomit too. Stil needs to run at way lower resolution for everybody to be able to see what's happening. Lara ditching the GUI and writing the index in T-SQL instead, lots of cheers from bloggers table.

**[9:00]  **Lara demoing cloud based DQS, Azure marketplace, ColumnStore indexes.

**[9:02]  **Quentin onto next topic – Organization Compliance. Added audit options as well as user - defined server roles.

**[9:04]  **Next topic – Peace of Mind. Production simulated application testing – distributed replay hint. SCOM advisor & management packs. Expanded support – Premier mission critical, no direct details yet.

**[9:05]  **Next topic – Scalable Data Warehousing + Fast Time to Solution. Hardware vendors delivering hardware applications. HW+SW+Support – turn on the faucet, buy appliances, preoptimized. This is the SQLBits keynote. Choice of hardware – semi-appliance option as well?

**[9:10]  **Going through hardware on stage, introducing the Dell & HP appliances.for PDW, DW in general. Plug'n'play appliances. Appliances sound great, but aren't they just overpriced versions of what you can do yourself? Add virtualization and you've got a complete appliance in just a VMDK. From boot to data loading / production usage – 20 minutes. If you have the money, this is a great & quick way to get started.

**[9:14]  **HP appliance can be ordered a month from now. Can start small and scale up – "Don't know how big you can built it, haven't reached limit yet".

**[9:19]  **Next topic – Extent Any Data, Anywhere – ODBC drivers for Linux – unreadable white on light orange background slide text.

**[9:21]  **Beyond relational – adding new support for FileTable – FileStream evolved. 2D spatial, semantic search.

**[9:25]  **Semantic search being demoed on stage by Michael Rys. Semantic search is basically an addition on top of FTS adding more language specific semantic improvements.

**[9:28]  **Next topic – Optimized Productivity – Juneau / SQL Server Data Tools reference. Shipped with SQL Server 2012. Unified across DB & BI, meaning no more SSMS/BI, just one "studio". Deployment & targeting freedom – just like .NET dev, we can target a certain platform and be limited to just hte options available. Talking contained databases.

**[9:29]  **Last topic – Scale on Demand. AlwaysON, deployment across public & private – hybrid cloud/private reference? Elastic scale = cloud reference, spin up many instances. It's not vertical scaling, this is sharding through SQL Azure.

**[9:31]  **Nicholas Dritsas on stage demoing deployment through SSMS 2012 / SQL Server Data Tools. "And I know how to get a big applause – Zoomit" – keynote speakers seem to be catching up on the zoomit hype. Thank god!

**[9:33]  **Speakers referencing zoomit means they're checking the tweets realtime – excellent.

**[9:34]  **This is cool – SSMS being used to manage both local and Azure databases. You can now manage Azure without defaulting to the online web based management portal. This will make it much easier to achieve the "hybrid" approach of having both on-premise and cloud based databases.

**[9:35]  **Now demoing how to do SQL Azure backups to Windows Azure. Lovely, and about time. Why wasn't this here from the start?

**[9:39]  **Cihan Biyikoglu entering the stage to talk about Elastic Scale – also known as SQL Azure Federations.

**[9:43]  **New Azure management portal uses Metro UI, Looks slick, but does it scale?

**[9:45]  **SQL Azure will now scale up to 150GB. But if only way to do "elastic scaling" is federation, whether it's 50GB or 150GB doesn't really change much other than your shard size. Since we're still limited by unknown hardware, we may not be able to even utilize a 150GB database before having to federate.

**[9:47]  **Execution plans in the Azure web interface based on Metro, looks slick.
