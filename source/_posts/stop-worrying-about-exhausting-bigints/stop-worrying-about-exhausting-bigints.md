---
permalink: stop-worrying-about-exhausting-bigints
title: Stop Worrying About Exhausting Bigints
date: 2012-02-20
tags: [SQL Server]
---
I've done it myself, worried about what to do when I exhausted my bigint identity value. I was worried that part of the LSN being a bigint – what would happen when it ran out? Would it perform an integer overflow? The answer? Worrying about exhausting the range of a bigint is not something you should spend time on. Allow me to elaborate.

<!-- more -->

## The log table

Imagine you have a table to which you only add new rows. You never delete rows and thus you can't reuse previous identity values. Instead of looking at how many rows you can have (9,223,372,036,854,775,807; or 18,446,744,073,709,551,616 if you use the maximum negative value as the seed), let's take a look at the storage requirements, if you were to actually exhaust the range.

Imagine that each row stores nothing but the bigint – no data at all, only the bigint identity value. The fixed row size can easily be calculated as such: two bytes for the status bits, two bytes for the null bitmap pointer, eight bytes for the bigint value, two bytes for the null bitmap column count and finally a single byte for the null bitmap itself. In total – 15 bytes. On top of that we need to add two bytes for an entry into the record offset array.

On an 8KB page, the header takes up 96 bytes, leaving 8096 bytes for the body, including the record offset array. Weighing in at 17 bytes per record, we can store a total of 476 records per page.

If we theoretically were to max out the positive bigint range, storing all records on disk; storing *just the bigint identity column* would take up a whopping 9,223,372,036,854,775,807 / 476 * 8KB = 140,985 PB. And this is with a seed value of 1 – you can double that amount if you were to start at the negative max. Seeing as the SQL Server database size limit is 524TB – you should probably worry about that sometime before worrying about running out of bigint seed values.

## The high transaction/sec table

OK, ok, you don't store all of the rows, you delete them shortly after they've entered the system. Let's say you never have more than 1000 rows in the table at a time, thus avoiding the storage issue.

If you allocate 1,000,000 new rows every second (deleting the other 999,000 of them), how long would you last?

9,223,372,036,854,775,807 / 1,000,000 / 60 / 60 / 24 / 365 = 292,471 years. Even for atheists, that's a long time.

OK, ok, that's not enough. What if you allocated 100,000,000 new rows every second instead?

9,223,372,036,854,775,807 / 100,000,000 / 60 / 60 / 24 / 365 = 2,924 years.

So at 100 million new bigint seed allocations per second, you'd still last almost 3,000 years before running out. And you can double that to 6,000 years if you start from the negative max seed value.

If you do manage to setup a SQL Server system with this kind of tx/sec – I'd love to see it!
