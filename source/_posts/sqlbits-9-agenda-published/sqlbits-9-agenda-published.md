permalink: sqlbits-9-agenda-published
title: SQLBits 9 Agenda Published
date: 2011-08-03
tags: [Conferences & Presenting]
---
The [agenda for SQLBits 9](http://sqlbits.com/information/Agenda.aspx) has been published (though it's still provisional). It's looking really, really good. Especially so when you consider the price of the event – you've got until the 26th of August to get the [early bird price](http://sqlbits.com/information/Pricing.aspx) of £375 for two complete days of conference – PLUS a whole day of [full day training sessions](http://sqlbits.com/information/TrainingDay.aspx).

<!-- more -->

## My presentation

I'll be presenting my [Knowing the Internals, Who Needs SQL Server Anyway?](http://sqlbits.com/Sessions/Event9/Knowing_the_Internals_Who_Needs_SQL_Server_Anyway_) session. This is by far my favorite session! I originally gave it back in April at Miracle Open World, though under the name "Anatomy of an MDF File". Since then, I've added a good three months of development onto [OrcaMDF](https://github.com/improvedk/OrcaMDF), the project on which the backend of the session is based. As such, this session will be even more awesome than last! Here's the abstract:

> You're stuck on a remote island with just a laptop and your main database .MDF file. The boss calls and asks you to retrieve some data, but alas, you forgot to install SQL Server on your laptop. Luckily you have a HEX editor by your side! 

> In this level 500 deep dive session we will go into the intimate details of the MDF file format. Think using DBCC Page is pushing it? Think again! As a learning experiment, I've created an open source parser for MDF files, called OrcaMDF. Using the OrcaMDF parser I'll go through the primary storage structures, how to parse page headers, boot pages, internal system tables, data & index records, b-tree structures as well as the supporting IAM, GAM, SGAM & PFS pages. 

> Has your database suffered an unrecoverable disk corruption? This session might just be your way out! Using a corrupt & unattachable MDF file, I'll demo how to recover as much data as possible. This session is not for the faint of heart, there will be bits & bytes.

## My training day

If you think the session sounds awesome, fret not, you can opt in for a full day of delicious SQL Server internals at a level few people venture to! As I just blogged about recently, [knowing the internals is the key to creating efficient databases](/how-not-to-reinvent-indexes), without knowing how to do so. I'm honored to have [Simon Sabin feature my training day on his blog](http://sqlblogcasts.com/blogs/simons/archive/2011/07/26/must-attend-training-day-for-anyone-serious-about-sql.aspx), and I really cannot promote it any better than he does. This is the official abstract:

> Join me for a journey into the depths of the SQL Server storage engine. Through a series of lectures and demonstrations we'll look at the internals of pages, data types, indexes, heaps, extent & page allocation, allocation units, system views, base tables and nightmarish ways of data recovery. We'll look at several storage structures that are either completely undocumented or very scarcely so.  By the end of the day, not only will you know about the most important data structures in SQL Server, you'll also be able to parse every bit of a SQL Server data file yourself, using just a hex editor! There will be lots of hands on demos, a few of them performed in C# to demonstrate the parsing of data files outside of SQL Server.

For a more thorough agenda, you should [go check it out at the SQLBits website](http://sqlbits.com/information/Event9/SQL_Server_Storage_Engine_and_MDF_File_Internals/TrainingDetails.aspx).
