---
permalink: avoiding-page-splits-byhellip-splitting-pages
title: Avoiding Page Splits By Splitting Pages
date: 2011-05-24
tags: [SQL Server - Optimization]
---
Continuing my review of my old database designs, I stumbled upon yet another mind numbing design decision. Back then, I'd just recently learned about the whole page split problem and how you should always use sequentially valued clustered keys.  

<!-- more -->

This specific table needed to track a number of views for a given entity, storing a reference to the entity and the time of the view. As I knew page splits where bad, I added a clustered index key like so:

```sql
CREATE TABLE EntityViews
(
	ViewID int identity PRIMARY KEY CLUSTERED,
	EntityID int NOT NULL,
	Created datetime NOT NULL,
	OtherData char(20)
)
```

With a schema like this, insertions won't cause fragmentation as they'll follow the nice & sequential ViewID identity value. However, I did realize that all of my queries would be using EntityID and Created as a predicate, reading most, if not all, of the columns. By clustering on ViewID, I'd have to scan the entire table for all queries. As that obviously wouldn't be efficient, I added a nonclustered index:

```sql
CREATE NONCLUSTERED INDEX IDX_EntityID_Created ON EntityViews (EntityID, Created) INCLUDE (OtherData)
```

If you're shaking your head by now, good. This index solved my querying issue as I could now properly seek my data using (WHERE EntityID = x AND Created BETWEEN y AND z) predicates. However, the nonclustered index contains all of my columns, including ViewID as that's the referenced clustered key. And thus I'm storing all my data twice! My clustered index is neatly avoiding fragmentation, but my nonclustered index (that contains all the same data!) is experiencing the exact fragmentation issues that I originally wanted to avoid!

Realizing this fact, the correct schema would've been:

```sql
CREATE TABLE EntityViews
(
	EntityID int NOT NULL,
	Created datetime NOT NULL,
	OtherData char(20)
)
CREATE CLUSTERED INDEX CX_EntityID_Created ON EntityViews (EntityID, Created)
```

We save the surrogate key value bytes for each row and all the data is stored only once. There's no need for secondary indexes as all the data is stored in the natural querying order. However, page splitting will occur as EntityID won't be sequential. This is easily avoided by scheduling REINDEX & REUBILD as appropriate.

Furthermore, as the clustered key is sorted on Created secondarily, older non-fragmented data won't be affected – it'll only affect the most recently added pages, which are probably in memory anyways and thus won't cause problems for querying.
