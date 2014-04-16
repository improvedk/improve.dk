permalink: what-is-the-size-of-the-lob-pointer-for-max-types-like-varchar-varbinary-etc
title: What is the Size of the LOB Pointer for (MAX) Types Like Varchar, Varbinary, Etc?
date: 2011-07-18
tags: [SQL Server - Internals]
---
LOB types like varchar(MAX), nvarchar(MAX), varbinary(MAX) and xml suffer from split personality disorder. SQL Server may store values in-row or off-row depending on the size of the value, the available space in the record and the table settings. Because of this, it's no easy task to predict the size of the pointer left in the record itself. You might even say… It depends.

<!-- more -->

Based on [this post](http://www.sqlservercentral.com/Forums/Topic1143500-391-1.aspx), and the fact that I'm working on LOB type support for [OrcaMDF](/introducing-orcamdf) at the moment, I decided to look into the LOB pointer storage structures.

## Setup

For testing, we'll use a very simple schema:

```sql
CREATE TABLE Lob
(
	A char(5000) NULL,
	B varchar(MAX) NOT NULL
)
```

## The [BLOB Inline Data] for in-row data

If data is small enough (“small enough” being hard to define as it depends on the free space in the page, the mood of SQL Server and probably a bunch of other undocumented factors), it will be stored in the record itself. Let's insert a single small row:

```sql
INSERT INTO Lob (B) VALUES ('Test')
```

Since A takes up 5000 bytes and we only try to insert 4 bytes into B, there's plenty of space for it to be stored in-row, taking up only the expected 4 bytes that we inserted. This behavior is just as a normal varchar(X) SLOB column would react.

image_4.png

## The [BLOB Inline Root] for row-overflow data

Now let's truncate the table and insert a new row, forcing SQL Server to push the data off-row as there isn't enough space on the original record:

```sql
TRUNCATE TABLE LOB
INSERT INTO Lob (B) VALUES (REPLICATE(CAST('a' AS varchar(max)), 4000))
```

Since A once again takes up a fixed amount of 5000 bytes and we're now trying to insert 4000 more bytes, we exceed the maximum capacity of 8096 bytes for the page body, causing SQL Server to push the data off-row. Running a DBCC IND confirms that SQL Server has allocated a new IAM page to track the LOB data pages:

```sql
DBCC IND (X, 'Lob', -1)
```

image_6.png

Extracting the record and looking at column 2 reveals that we're now storing a 24 byte row-overflow pointer:

image_8.png

This pointer, once again, is exactly like a SLOB column would be stored. The (MAX) LOB variants do have one trick that SLOBs don't have though – they can be longer than 8000 bytes. In that case, we need more than one page to store the value – and thus the off-row pointer needs to point at more than one page. The off-row pointer, at an abstract level, points to a root of pointers that then point onwards to the actual data pages (or onto another root in case we need more than two levels of page references). If the root is small enough, it'll be stored in-row. The smallest root possible is 12 bytes – a single page reference (the extra 12 bytes is due to overhead). Each following page reference takes up an extra 12 bytes. Thus, an inline root pointing to two pages will take up 36 bytes of space, and so forth, just look at this:

image_10.png

image_12.png

image_14.png

image_16.png

image_18.png

Note how we go from 24 to 36, 48, 60 and finally 72 bytes for a total of 40.000 bytes of data, stored on five data pages. Once we exceed 72 bytes, SQL Server doesn't store the whole root inline any longer, instead if points to a single new slot on another page. The (1:379) page is a text_tree page, containing references to the pages where the data is stored:

image_20.png

I haven't been able to make SQL Server store inline blob roots any larger than 72 bytes so I'm guessing that's a hard cutoff value before it'll start referencing text_tree pages. Ignoring text_tree pages, the pointer format so far has been exactly the same as for SLOBs. So when exactly does SQL Server store a classic 16 byte LOB pointer for (MAX) LOB types?

## The [Textpointer] for LOB data

SQL Server will *never* store a LOB pointer for the (MAX) LOB types, *unless* the [large value types out of row](http://msdn.microsoft.com/en-us/library/ms173530.aspx) setting has been turned on. Let's clear the table, set the setting, and then insert a new row like before:

```sql
TRUNCATE TABLE LOB
EXEC sp_tableoption N'Lob', 'large value types out of row', 'ON'
INSERT INTO Lob (B) VALUES (REPLICATE(CAST('a' AS varchar(max)), 4000))
```

Now just as with the inline root, SQL Server allocates an IAM page to track the LOB data pages:

image_22.png

But when we look at the record stored on page (1:4380), we see that it stores a Textpointer instead of an inline blob root:

image_24.png

## Mixed pointer types

As long as the [large value types out of row](http://msdn.microsoft.com/en-us/library/ms173530.aspx) setting is off (which it is by default), the (MAX) LOB types will act exactly like a SLOB column, except for the fact that the data can be larger than 8000 bytes. Once we turn the setting on, the (MAX) LOB types start acting like classic LOB types. So does this mean that the tables will always either use inline blob roots or textpointers? No, if only it were that simple. Take a look at this sample:

```sql
CREATE TABLE TrickyLob
(
	A varchar(MAX) NOT NULL
)
INSERT INTO TrickyLob VALUES ('Mark')
INSERT INTO TrickyLob VALUES (REPLICATE(CAST('a' AS varchar(MAX)), 9000))
EXEC sp_tableoption N'TrickyLob', 'large value types out of row', 'ON'
INSERT INTO TrickyLob VALUES (REPLICATE(CAST('a' AS varchar(MAX)), 4000))
```

Running DBCC PAGE on the single allocated data page reveals that we now have three records using three different pointer types:

image_28.png

Lesson: When sp_tableoption is run to set the [large value types out of row](http://msdn.microsoft.com/en-us/library/ms173530.aspx) setting, it only takes effect for newly added records. A table rebuild won't affect existing inline blob roots either, only updates to existing records will rebuild the record and convert the inline blob root to a textpointers.

## Conclusion

Predicting the LOB pointer type & size can be tricky as it depends on multiple factors. Using the above, you should be able to get a notion of what will be stored, as well as to interpret the DBCC PAGE results you might run into.
