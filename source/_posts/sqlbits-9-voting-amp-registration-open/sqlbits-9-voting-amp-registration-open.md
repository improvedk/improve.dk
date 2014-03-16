permalink: sqlbits-9-voting-amp-registration-open
title: SQLBits 9 Voting & Registration Open
date: 2011-07-12
tags: [Conferences & Presenting]
---
The [registration for SQLBits 9 is now open](http://sqlbits.com/information/Pricing.aspx).Equally important, the voting for sessions is has also opened. Simply login and take a look at the [list of sessions](http://sqlbits.com/information/PublicSessions.aspx) to vote for your preferred sessions.

<!-- more -->

## My sessions

<img style="margin: 15px; display: inline; float: right;" alt="Training Day" src="http://sqlbits.com/images/SQLBits/SQLBitsTrainingDay.png" align="right" />

Besides doing a full day training on the [SQL Server Storage Engine and MDF File Internals](http://sqlbits.com/information/Event9/SQL_Server_Storage_Engine_and_MDF_File_Internals/TrainingDetails.aspx), I’ve also submitted two related sessions that present a subset of the training day material, in a much more dense format. Both sessions are very technical deep dives on the storage internals. If you think they sound interesting, I’d appreciate a vote :)

### Knowing The Internals, Who Needs SQL Server Anyway?

You're stuck on a remote island with just a laptop and your main database .MDF file. The boss calls and asks you to retrieve some data, but alas, you forgot to install SQL Server on your laptop. Luckily you have a HEX editor by your side!

In this level 500 deep dive session we will go into the intimate details of the MDF file format. Think using DBCC Page is pushing it? Think again! As a learning experiment, I've created an open source parser for MDF files, called OrcaMDF. Using the OrcaMDF parser I'll go through the primary storage structures, how to parse page headers, boot pages, internal system tables, data & index records, b-tree structures as well as the supporting IAM, GAM, SGAM & PFS pages.

Has your database suffered an unrecoverable disk corruption? This session might just be your way out! Using a corrupt & unattachable MDF file, I'll demo how to recover as much data as possible. This session is not for the faint of heart, there will be bits & bytes.

### Demystifying Database Metadata

You know how to query sys.tables, sys.columns and perhaps even sys.system_internals_allocation_units to discover the metadata contents of a database. But where are these views getting their data from, and how do the core system tables relate?

Based on my work with OrcaMDF, an open source C# parser for MDF files, I will demonstrate how to parse the internal system tables. Using just the boot page as origin, we'll discover how to traverse the chain of references that ultimately end up in references to the actual system table data, from where we can parse the data records.

Once we’ve got the system table data, I’ll demonstrate how to correlate the different tables to end up with the data we see in common system views.

## My picks

<img style="margin: 15px; display: inline; float: right;" alt="Sessions Title" src="http://sqlbits.com/images/headings/Sessions.png" width="286" height="98" align="right" />Having only 10 votes, it’s tough to pick out the top 10 sessions from a list of almost ~150 sessions. Though it was a struggle, I ended up with the following votes:

[Performance tuning from the field](http://sqlbits.com/Sessions/Event9/Performance_tuning_from_the_field) by Simon Sabin  
[Transaction Log Performance and Troubleshooting – Deep Dive](http://sqlbits.com/Sessions/Event9/Transaction_Log_Performance_and_Troubleshooting-Deep_Dive) by Chirag Roy  
[SQL Server Denali – Always On Deep Dive](http://sqlbits.com/Sessions/Event9/SQL_Server_Denali-Always_On_Deep_Dive) by Bob Duffy  
[HA/DR – Focus on Options, Comparisons and Interoperability](http://sqlbits.com/Sessions/Event9/HA_DR-Focus_on_Options_Comparisons_and_Interoperability) by Chirag Roy  
[READPAST & Furious: Transactions, Locking and Isolation](http://sqlbits.com/Sessions/Event9/READPAST__Furious_Transactions_Locking_and_Isolation) by Mark Broadbent  
[Preparation for Disaster](http://sqlbits.com/Sessions/Event9/Preparation_for_Disaster) by Steve Jones  
[Busted! A journey into the most common TSQL & Tuning myths](http://sqlbits.com/Sessions/Event9/Busted_A_journey_into_the_most_common_TSQL__Tuning_myths) by David Morrison  
[Turbocharge Database Recoverability Performance](http://sqlbits.com/Sessions/Event9/Turbocharge_Database_Recoverability_Performance) by Chirag Roy  
[Implementing Real-Time Data Warehouse](http://sqlbits.com/Sessions/Event9/Implementing_Real-Time_Data_Warehouse) by Sutha Thiru  
[Replication – Best Practices, Troubleshooting & Performance](http://sqlbits.com/Sessions/Event9/Replication-Best_Practices_Troubleshooting__Performance) by Neil Hambly  

Not until now did I notice that three of my sessions are by [Chirag Roy](http://sqlking.wordpress.com/), I’ll use this as an opportunity to congratulate him on becoming the latest [Microsoft Certified Master](http://www.microsoft.com/learning/en/us/certification/master-sql.aspx)!
