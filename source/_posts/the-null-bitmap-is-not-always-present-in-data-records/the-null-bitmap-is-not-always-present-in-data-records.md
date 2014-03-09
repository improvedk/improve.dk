permalink: the-null-bitmap-is-not-always-present-in-data-records
title: The Null Bitmap is Not Always Present in Data Records
date: 2011-07-15
tags: [SQL Server - Internals]
---
While [implementing sparse column support](http://improve.dk/archive/2011/07/15/sparse-column-storage-ndash-the-sparse-vector.aspx" target="_blank) for [OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank), I ran into a special condition that caught me by surprise – a data record with no null bitmap. Even [Paul Randal](http://www.sqlskills.com/BLOGS/PAUL/" target="_blank) mentioned that the null bitmap would *always* be present in data records in his [A SQL Server DBA myth a day: (6/30) three null bitmap myths](http://www.sqlskills.com/BLOGS/PAUL/post/A-SQL-Server-DBA-myth-a-day-(630)-three-null-bitmap-myths.aspx" target="_blank) post.

<!-- more -->

## Optimized storage of sparse-only tables

During my testing I discovered that tables containing only sparse columns neither stored a null bitmap, nor the usual number of columns. Let’s create a test table and find a reference to the data page:

```sql
CREATE TABLE OnlyFixedSparse (A int SPARSE)
INSERT INTO OnlyFixedSparse VALUES (5)
DBCC IND (X, 'OnlyFixedSparse', -1)
```

image_211.png

And then let’s check the record contents for page (1:4359):

```sql
DBCC PAGE (X, 1, 4359, 3)
```

image_65.png

The first two bytes contain the record status bits. Next two bytes contain the offset for the end of the fixed-length portion of the record – which is 4 as expected, since we have no non-sparse fixed-length columns. As shown in the *Record Attributes* output, the status bytes indicates that there’s no null bitmap, and sure enough, the next two bytes indicates the number of variable length columns. The remaining bytes contains the variable length offset array as well as the [sparse vector](http://improve.dk/archive/2011/07/15/sparse-column-storage-ndash-the-sparse-vector.aspx" target="_blank).

### Under what conditions does the data record not contain a null bitmap?

I did a quick empirical test to verify my theory that this only happens on tables containing only sparse columns:

<table width="500">
	<tbody>
		<tr>
			<td valign="top" width="299">**Schema**</td>
			<td valign="top" width="193">**Contains null bitmap**</td>
		</tr>
		<tr>
			<td valign="top" width="299">Only variable length columns</td>
			<td valign="top" width="193">Yes</td>
		</tr>
		<tr>
			<td valign="top" width="299">Only fixed length columns</td>
			<td valign="top" width="193">Yes</td>
		</tr>
		<tr>
			<td valign="top" width="299">Only sparse fixed length columns</td>
			<td valign="top" width="193">No</td>
		</tr>
		<tr>
			<td valign="top" width="299">Only sparse variable length columns</td>
			<td valign="top" width="193">No</td>
		</tr>
		<tr>
			<td valign="top" width="299">Fixed length + sparse fixed length columns</td>
			<td valign="top" width="193">Yes</td>
		</tr>
		<tr>
			<td valign="top" width="299">Variable length + sparse fixed length columns</td>
			<td valign="top" width="193">Yes</td>
		</tr>
	</tbody>
</table>

Thus it would seem that this is an optimization made possible for tables containing nothing but sparse columns.

### There’s always an exception to the exception

It *is* actually possible to have a data record without a null bitmap for a table with non-sparse columns too. Continuing on from the OnlyFixedSparse table from before, let’s add two extra nullable columns:

```sql
ALTER TABLE OnlyFixedSparse ADD B int NULL
Alter Table OnlyFixedSparse ADD C varchar(10) NULL
```

Checking the stored record reveals the exact same output as before:

image_86.png

Thus it would seem that even without a null bitmap the usual alter semantics are followed – the addition of new nullable columns does not need to alter existing records. If we’d added a non-nullable column to the table, we would have to modify the record, causing the addition of a null bitmap and column count. The same goes if we insert a value into any of those new columns:

```sql
UPDATE OnlyFixedSparse SET B = 2
```

image_105.png

By setting the value of the B column we just added 7 extra bytes to our data record. 4 for the integer, 2 for the column count and 1 for the null bitmap. Had we not performed the update for all records in the table, only the affected records would be updated. This means we may have data records for a table where some have a null bitmap while others don’t. Just take a look at this:

```sql
CREATE TABLE OnlyFixedSparse (A int SPARSE)
INSERT INTO OnlyFixedSparse VALUES (5), (6)
ALTER TABLE OnlyFixedSparse ADD B int NULL
UPDATE OnlyFixedSparse SET B = 2 WHERE A = 5
```

image_125.png

## Conclusion

As I unfortunately do not work on the SQL Server team and I haven’t seen this condition documented, I can only theorize on this. For all normal data records, the null bitmap is always present, even if the table does not contain any null columns. While we can achieve [read optimizations](http://www.sqlskills.com/blogs/paul/post/Inside-the-Storage-Engine-Anatomy-of-a-record.aspx" target="_blank) when columns may be null, for completely non-null tables, we still get the benefit that we can add a new nullable column to an existing schema, without having to modify the already existing records.

While I think it’s bit of a special use case, my theory is that this is a specific optimization made for the case where you have a table with lots of sparse columns and no non-sparse columns present. For those cases, we save at least three bytes – two for the number of columns and at least one for the null bitmap. If there are only sparse columns, we have no need for the null bitmap as the null value is defined by the value not being stored in the sparse vector.
