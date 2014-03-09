permalink: how-not-to-reinvent-indexes
title: How Not to Reinvent Indexes
date: 2011-08-01
tags: [SQL Server]
---
In a moment of weakness I [pleged to make an absolute fool of myself](https://twitter.com/#!/improvedk/status/96646125402062849) for this months [Meme Monday](http://thomaslarock.com/2011/07/meme-monday-for-august/). I wish I could say that this happened 20 years ago, when I was but a young grasshopper. To my disgrace, this happened fewer years ago than I’d like to admit.

<!-- more -->

## Wow, that’s a lot of data!

As part of beginning a new project that would publish catalogs on the web, I tried to do some capacity calculations on storing user statistics. We needed to be able to query the number of views a single page in a given catalog had had, by the hour. Assuming a worst case scenario of 24/7 visitors for a catalog with 100 pages, that would equal 100 pages * 24 hours * 365 days, roughly equal to a million rows per year, per catalog.

At this point I’d been working with SQL Server for some years, though exclusively as a developer. I had no knowledge of the inner workings, storage, index internals, etc. I knew enough to get by as a normal web dev, never really reaching any limits in SQL Server no matter how brain dead my solutions were. As a result, when I figured we might have hundreds of these catalogs, we might have hundreds of millions of rows. Wow, there’s absolutely no way SQL Server will be able to search that must data in a table!

## Reinventing the clustered index

Being convinced there was no way SQL Server would be able to search that many rows in a single table, I chose to shard my data. Not into separate tables, that’d be too easy. Instead I opted to create a database per catalog, with the sole purpose of storing statistics.

This was brilliant. It really was. Or at least I thought so.

Now instead of SQL Server having to search through a hundred million row large table, I would just query my catalog statistics like so:

```sql
SELECT * FROM CatalogStatistics_[CatalogID].dbo.StatisticsTable WHERE Period BETWEEN @X AND @Y
```

I knew indexes were crucial to querying so I made sure to create a nonclustered index on the Period column. Usually It’d go unused as it would require massive amounts of bookmark lookups and there’d be sufficiently small amounts of data that a clustered index scan was more effective.

## Knowing of indexes does not mean you understand them

Obviously I’d heard of indexes, I’d even used them actively. I thought I understood them – you just create them on the columns you query and everything works faster, right?

I’ll give myself the credit that I knew SQL Server would need some kind of way to quickly narrow down the data it had to search. I thought I’d help out SQL Server by storing the data in separate databases, making sure it would be easy for it to scan just the data for a specific catalog. Had I known how indexes really worked, being stored in an ordered binary tree, I’d realize SQL Server wouldn’t benefit from my scheme at all.

I just made stuff worse by causing log trashing, disk trashing, memory trashing, management trashing, backup trashing, you name it, I trashed it.

## Dude, this isn’t gonna work

Fast forward a couple of months. Performance wasn’t the bottleneck as there just wasn’t nearly enough data or querying to really cause concern. What was becoming a bottleneck on the other hand; management. We were on a managed server solution with an external hosting company acting as DBAs, though only ensuring SQL Server was running and was backed up. I got an email saying that they were having trouble handling our backups. At that point we had just short of 3.000 databases on the instance.

At the same time I was having trouble satisfying our querying requirements. In the beginning we just needed to query the statistics of a single catalog at a time. Later on we needed to dynamically aggregate statistics across several catalogs at a time. Suffice to say, this didn’t work out well in the long run:

```sql
SELECT X, Y, Z FROM CatalogStatistics_123.dbo.StatisticsTable WHERE Period BETWEEN @X AND @Y

UNION ALL

SELECT X, Y, Z FROM CatalogStatistics_392.dbo.StatisticsTable WHERE Period BETWEEN @X AND @Y

UNION ALL

SELECT X, Y, Z FROM CatalogStatistics_940.dbo.StatisticsTable WHERE Period BETWEEN @X AND @Y

UNION ALL

SELECT X, Y, Z FROM CatalogStatistics_1722.dbo.StatisticsTable WHERE Period BETWEEN @X AND @Y

...
```

## My revelation

I remember going to my first SQL Server conference, attending an entry level internals session. Suddenly I knew what a page was, I knew, on a high level, how data was stored in SQL Server. Suddenly I understood the importance of the data being stored in a b-tree and how much it meant to my scalability concerns.

What I really like about this whole ordeal, when looking back at it, is how I didn’t attend a session on proper indexing. I didn’t attend a session about SQL Server limitations, how to store hundreds of millions of rows. Nope, I intended a session on the internals. Having just that basic knowledge of the internals suddenly provided me the necessary knowledge to figure it out myself. Maybe this is the real reason I’ve become slightly obsessed with internals ever since.

I don’t want a fish, I want to know how to fish.
