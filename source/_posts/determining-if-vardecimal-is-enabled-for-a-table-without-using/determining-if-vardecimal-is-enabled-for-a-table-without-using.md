permalink: determining-if-vardecimal-is-enabled-for-a-table-without-using
title: Determining If Vardecimal Is Enabled For a Table Without Using OBJECTPROPERTY
date: 2011-12-12
tags: [SQL Server - Internals]
---
Determining whether [vardecimal](http://msdn.microsoft.com/en-us/library/bb508963(v=sql.90).aspx) is enabled for a given table is usually done by using the OBJECTPROPERTY function like so:

<!-- more -->

```sql
SELECT OBJECTPROPERTY(OBJECT_ID('MyTable'), 'TableHasVarDecimalStorageFormat')
```

If it’s disabled, you’ll see this:

image_2.png

Whereas if it’s enabled, you’ll see a 1:

image_41.png

This works excellent as long as SQL Server is running, and you have access to the OBJECTPROPERTY function. However, as for all I know, there’s no DMV that exposes the vardecimal status for a table. I’ve also not been able to find this property in any of the base tables (if you know where/how it’s actually stored, please let me know!).

However, I’ve come up with the following query as a workaround for determining if vardecimal is enabled for a given table, without using OBJECTPROPERTY:

```sql
SELECT
	COUNT(*)
FROM
	sys.system_internals_partition_columns PC
INNER JOIN
	sys.partitions P ON P.partition_id = pc.partition_id
INNER JOIN
	sys.tables T ON T.object_id = P.object_id
WHERE
	T.name = 'MyTable' AND
	P.index_id <= 1 AND
	PC.system_type_id = 106 AND
	PC.leaf_offset < 0
```

What this does is to look for all of the decimal columns for the table, stored on any partition (as vardecimal is set at the table level, we don’t really care about the specific partitions) belonging to either the clustered index or heap.

Note that while this is usually true, you can actually have partitions within the same object, with both decimal and vardecimal columns. If you enable and disable vardecimal quickly, you'll often see an extra partition with the old schema definition, though no pages are allocated. Thus, if you're using this method for parsing purposes - make sure to check at the partition level. For most use cases, this won't be an issue.

If *any* of those decimal columns have a negative leaf_offset value (result > 0), we can be sure that vardecimal is enabled for the table. The leaf_offset value determines the physical order of the fixed length columns in the actual records stored on disk. All variable length columns will have a negative value, and as such, normal decimal columns should always have a positive value. If any decimal column has a negative leaf_offset value, we know it’s stored in the variable length section of the records – and only vardecimals are stored that way!

## Example

Here’s a table without vardecimal enabled:

```sql
CREATE TABLE MyDecimalTable (A decimal(5, 0))

SELECT
	COUNT(*)
FROM
	sys.system_internals_partition_columns PC
INNER JOIN
	sys.partitions P ON P.partition_id = pc.partition_id
INNER JOIN
	sys.tables T ON T.object_id = P.object_id
WHERE
	T.name = 'MyDecimalTable' AND
	P.index_id <= 1 AND
	PC.system_type_id = 106 AND
	PC.leaf_offset < 0
```

image_61.png

And here’s one *with* vardecimal enabled:

```sql
CREATE TABLE MyVardecimalTable (A decimal(5, 0))

EXEC sp_tableoption 'MyVardecimalTable', 'vardecimal storage format', '1'

SELECT
	COUNT(*)
FROM
	sys.system_internals_partition_columns PC
INNER JOIN
	sys.partitions P ON P.partition_id = pc.partition_id
INNER JOIN
	sys.tables T ON T.object_id = P.object_id
WHERE
	T.name = 'MyVardecimalTable' AND
	P.index_id <= 1 AND
	PC.system_type_id = 106 AND
	PC.leaf_offset < 0
```

image_8.png
