permalink: presenting-a-precon-at-sqlbits
title: Presenting a Precon at SQLBits
date: 2011-06-30
tags: [Conferences and Presenting]
---
I'm honored to announce that I'll be presenting my *[SQL Server Storage Engine and MDF File Internals](http://sqlbits.com/information/Event9/SQL_Server_Storage_Engine_and_MDF_File_Internals/TrainingDetails.aspx)* precon at [SQLBits](http://sqlbits.com/) on September 29th. It's an amazing bunch of [precon sessions and presenters](http://sqlbits.com/information/TrainingDay.aspx), I didn't think I'd be speaking at SQLBits, much less presenting a precon – given the lineup of speakers.

<!-- more -->

ImGoingToSqlBits200.png

## So what will I be presenting?

Let me start out with the official abstract:

> Join me for a journey into the depths of the SQL Server storage engine. Through a series of lectures and demonstrations we'll look at the internals of pages, data types, indexes, heaps, extent & page allocation, allocation units, system views, base tables and nightmarish ways of data recovery. We'll look at several storage structures that are either completely undocumented or very scarcely so.  By the end of the day, not only will you know about the most important data structures in SQL Server, you'll also be able to parse every bit of a SQL Server data file yourself, using just a hex editor! There will be lots of hands on demos, a few of them performed in C# to demonstrate the parsing of data files outside of SQL Server.

It all stems from some experimental coding I did back in March, trying to parse SQL Server data pages using C#. What started out as a learning experiment resulted in me presenting on the *[Anatomy of an MDF File](http://mow2011.dk/speakers/mark-s-rasmussen.aspx)* at the [Miracle Open World 2011](http://mow2011.dk/mow2011.aspx) conference in April. Since then I've continued my experiment and officially christened it [OrcaMDF](https://github.com/improvedk/OrcaMDF) while opening up the source.

During my research for OrcaMDF I've reverse engineered data structures, matched up various undocumented base tables and generally achieved a really great understanding of the storage engine and MDF file format. It's my goal that attendees of my session will have a complete (well, almost) understanding of the on disk structures we'll see in typical databases. Not just by looking at them through DBCC PAGE, but by taking it a couple of steps further by attempting to parse the contents by hand. You'll get an understanding of the importance of the sys.sysallocunits base table and how that fuels all metadata in SQL Server.

For the full agenda, please check the official [precon description](http://sqlbits.com/information/Event9/SQL_Server_Storage_Engine_and_MDF_File_Internals/TrainingDetails.aspx). If you have any questions about the content that you'd like to clear up before signing up for either mine or one of the other precons available, I encourage you to leave a comment here or grab a hold of [me on Twitter](http://twitter.com/#!/improvedk).

## Prerequisites

While I will give brief introductions to the main concepts, I will assume a solid knowledge of what SQL Server does. I won't spend a long time explaining why you should use clustered tables over heaps, instead I'll give you the toolset to hopefully make that decision yourself. I'm not exaggerating when I classified the precon as level 500 – there will be 300 content too but we'll be peaking at 500 several times during the day. We will be looking at hex codes, converting between decimal, hex and binary as needed – so make sure you don't throw up when you see binary numbers :)

## Registration

Registration hasn't opened yet, but [sign up and you'll be notified](http://www.sqlbits.com/information/Pricing.aspx) as soon as it opens. A three day conference, including a precon day for just £375 really is a steal so make sure to get the early bind discount! I still can't fathom that Saturday is free, as in £0, no charge – that's too good an offer to pass up!
