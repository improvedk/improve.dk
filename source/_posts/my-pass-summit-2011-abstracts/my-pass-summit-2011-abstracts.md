permalink: my-pass-summit-2011-abstracts
title: My PASS Summit 2011 Abstracts
date: 2011-05-03
tags: [Conferences & Presenting]
---
Last year I went on a marathon conference trip, starting out in LA at [Adobe Max](http://2010.max.adobe.com/), continuing on to [QCon](http://qconsf.com/sf2010/) in San Francisco before finally ending up in lovely Seattle for the [PASS Summit](http://www.sqlpass.org/summit/na2010/).

<!-- more -->

71624_444201957306_724027306_5893683_5375463_n_2.jpg

Of the three, the one I'll definitely be returning to this year is the PASS Summit. Last year I had the honor of getting a nice neon green “FIRST-TIMER” snippet on my badge, a snippet I wore with honor. This year however, I'm aiming for the Alumni snippet, and perhaps even the speaker snippet by submitting a couple of abstracts.

As I've been somewhat busy with [OrcaMDF](/introducing-orcamdf) in my spare time recently, both of my abstracts use OrcaMDF as the origin for going into the internals. I really hope I get a chance to present about this as I'm super psyched about it :)

## Knowing the Internals – Who Needs SQL Server Anyway?

You're stuck on a remote island with just a laptop and your main database .MDF file. The boss calls and asks you to retrieve some data, but alas, you forgot to install SQL Server on your laptop. Luckily you have a HEX editor by your side!

In this level 500 deep dive session we will go into the intimate details of the MDF file format. Think using DBCC Page is pushing it? Think again! As a learning experiment, I've created an open source parser for MDF files, called OrcaMDF. Using the OrcaMDF parser I'll go through the primary storage mechanisms, how to parse page headers, boot pages, internal system tables, data & index records, b-tree structures as well as the supporting IAM, GAM, SGAM & PFS pages.

Has your database suffered an unrecoverable disk corruption? This session might just be your way out! Using a corrupt & unattachable MDF file, I'll demo how to recover as much data as possible. This session is not for the faint of heart, there will be bytes!

### Why & What?

I originally gave this presentation at [Miracle Open World 2011](http://mow2011.dk/mow2011.aspx) and got some great feedback. Fueled by positive feedback I continued the development and am now at the point where I have so much content I considered doing a 3½ hour session instead. By attending this session you will not only see diagrams of internal structures on slides, you'll actually see C# code demonstrated that parses & utilizes them!

## Demystifying Database Metadata

You know how to query sys.tables, sys.columns and perhaps even sys.system_internals_allocation_units to discover the metadata contents of a database. But where are these views getting their data from, and how do the core system tables relate?

Based on my work with OrcaMDF, an open source C# parser for MDF files, I will demonstrate how to parse the internal system tables. Using just the boot page as origin, we'll discover how to traverse the chain of references that ultimately end up in references to the actual system table data, from where we can parse the data records.

Once we've got the system table data, I'll demonstrate how to correlate the different tables to end up with the data we see in common system views.

### Why & What?

As I continued the development of OrcaMDF I used the various system views extensively – sys.objects, tables, columns, indexes, internals_allocation_units etc. However, as development moved forward and I needed to parse that metadata myself, I had to look into the underlying tables – sysallocunits, sysschobjs, sysrowsets, sysrowsetcolumns. Join this session and let's enter the realm of the hidden metadata and let's explore it together!
