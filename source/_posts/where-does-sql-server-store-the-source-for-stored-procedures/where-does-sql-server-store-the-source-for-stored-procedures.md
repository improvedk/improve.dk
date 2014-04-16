permalink: where-does-sql-server-store-the-source-for-stored-procedures
title: Where Does SQL Server Store the Source for Stored Procedures?
date: 2012-08-27
tags: [SQL Server - Internals, SQL Server - OrcaMDF]
---
At the moment I'm working on extending [OrcaMDF Studio](https://github.com/improvedk/OrcaMDF) to not only list base tables, DMVs and tables, but also stored procedures. That's easy enough, we just need to query sys.procedures – or that is, the sys.sysschobjs base table, since the sys.procedures DMV isn't available when SQL Server isn't running.

<!-- more -->

However, I don't want to just list the stored procedures, I also want to present the source code in them. That brings up a new task – retrieving said source code. Where is it stored? I wasn't able to find anything on Google, so let's take a look for ourselves!

I've created a new empty database with a data file of three megabytes. In this database, I've created a single stored procedure like so:

```sql
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE XYZ
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 'AABBCC' AS Output
END
```

Now when I select from sys.procedures, we can see that the procedure has object ID 2105058535:

```sql
select * from sys.procedures
```

image_2.png

So far so good. We can then retrieve the definition itself as an nvarchar(MAX) by querying sys.sql_modules like so:

```sql
select * from sys.sql_modules where object_id = 2105058535
```

image_4.png

And there you have it, the source code for the XYZ procedure! But hold on a moment, while I've gotten the object ID for the procedure by querying the sys.sysschobjs base table, I don't have access to sys.sql_modules yet, as that's a view and not a base table. Let's take a look at where sys.sql_modules gets the definition from:

```sql
select object_definition(object_id('sys.sql_modules'))
```

```sql
SELECT
	object_id = o.id,
	definition = Object_definition(o.id),
	uses_ansi_nulls = Sysconv(bit, o.status & 0x40000), -- OBJMOD_ANSINULLS
	uses_quoted_identifier = sysconv(bit, o.status & 0x80000),   -- OBJMOD_QUOTEDIDENT
	is_schema_bound = sysconv(bit, o.status & 0x20000),    -- OBJMOD_SCHEMABOUND
	uses_database_collation = sysconv(bit, o.status & 0x100000),  -- OBJMOD_USESDBCOLL
	is_recompiled = sysconv(bit, o.status & 0x400000),     -- OBJMOD_NOCACHE
	null_on_null_input = sysconv(bit, o.status & 0x200000),   -- OBJMOD_NULLONNULL
	execute_as_principal_id = x.indepid
FROM
	sys.sysschobjs o
LEFT JOIN
	sys.syssingleobjrefs x ON x.depid = o.id AND x.class = 22 AND x.depsubid = 0 -- SRC_OBJEXECASOWNER
WHERE
	o.pclass <> 100 AND
	(
		(o.type = 'TR' AND has_access('TR', o.id, o.pid, o.nsclass) = 1) OR
		(type IN ('P','V','FN','IF','TF','RF','IS') AND has_access('CO', o.id) = 1) OR
		(type IN ('R','D') AND o.pid = 0)
	)
```

Hmmm, so sys.sql_modules gets the source by using the object_definition system function. Unfortunately, the following doesn't work:

```sql
select object_definition(object_id('object_definition'))
```

image_6.png

I happen to remember that sys.sql_modules is a replacement for the, now deprecated, sys.syscomments legacy view. Let's take a look at where that one gets the source from:

```sql
select object_definition(object_id('sys.syscomments'))
```

```sql
SELECT
	o.id AS id,  
	convert(smallint, case when o.type in ('P', 'RF') then 1 else 0 end) AS number,  
	s.colid,
	s.status,  
	convert(varbinary(8000), s.text) AS ctext,  
	convert(smallint, 2 + 4 * (s.status & 1)) AS texttype,  
	convert(smallint, 0) AS language,  
	sysconv(bit, s.status & 1) AS encrypted,  
	sysconv(bit, 0) AS compressed,  
	s.text  
FROM
	sys.sysschobjs o
CROSS APPLY
	OpenRowset(TABLE SQLSRC, o.id, 0) s  
WHERE
	o.nsclass = 0 AND
	o.pclass = 1 AND
	o.type IN ('C','D','P','R','V','X','FN','IF','TF','RF','IS','TR') AND
	has_access('CO', o.id) = 1  

UNION ALL  

SELECT
	c.object_id AS id,  
	convert(smallint, c.column_id) AS number,  
	s.colid,
	s.status,  
	convert(varbinary(8000), s.text) AS ctext,  
	convert(smallint, 2 + 4 * (s.status & 1)) AS texttype,  
	convert(smallint, 0) AS language,  
	sysconv(bit, s.status & 1) AS encrypted,  
	sysconv(bit, 0) AS compressed,  
	s.text  
FROM
	sys.computed_columns c
CROSS APPLY
	OpenRowset(TABLE SQLSRC, c.object_id, c.column_id) s  

UNION ALL  

SELECT
	p.object_id AS id,  
	convert(smallint, p.procedure_number) AS number,  
	s.colid,
	s.status,  
	convert(varbinary(8000), s.text) AS ctext,  
	convert(smallint, 2 + 4 * (s.status & 1)) AS texttype,  
	convert(smallint, 0) AS language,  
	sysconv(bit, s.status & 1) AS encrypted,  
	sysconv(bit, 0) AS compressed,  
	s.text  
FROM
	sys.numbered_procedures p
CROSS APPLY
	OpenRowset(TABLE SQLSRC, p.object_id, p.procedure_number) s  

UNION ALL  

SELECT
	o.id AS id,  
	convert(smallint, case when o.type in ('P', 'RF') then 1 else 0 end) AS number,  
	s.colid,
	s.status,  
	convert(varbinary(8000), s.text) AS ctext,  
	convert(smallint, 2) AS texttype,  
	convert(smallint, 0) AS language,  
	sysconv(bit, 0) AS encrypted,  
	sysconv(bit, 0) AS compressed,  
	s.text  
FROM
	sys.sysobjrdb o
CROSS APPLY
	OpenRowset(TABLE SQLSRC, o.id, 0) s  
WHERE
	db_id() = 1 AND 
	o.type IN ('P','V','X','FN','IF','TF')
```

Bummer. It doesn't use object_definition, but instead another internal function in the form of OpenRowset(TABLE SQLSRC, o.id, 0). I'm not one to give up easily though – I've previously [reverse engineered the OpenRowset(TABLE RSCPROP)](/exploring-the-sys-system_internals_partition_columns-ti-field) function.

Let's take a different approach to the problem. Everything in SQL Server is stored on 8KB pages in a fixed format. As the procedures aren't encrypted, they must be stored in clear text somewhere in the database – we just don't know where. Let's detach the database and crack open a hex editor (I highly recommend HxD):

image_10.png

Now let's see if we can find the procedure. On purpose I made it return "SELECT ‘AABBCC' AS Output" as that would be easy to search for:

image_12.png

And whadda ya know, there it is:

image_14.png

OK, so now we know that the source is stored in the database, just not where specifically. The data is stored at offset 0x00101AF0 in the data file. In decimal, that's offset 01055472. As each data page is exactly 8KB, we can calculate the ID of the data page that this is stored on (using integer math):

01055472 / 8192 = 128

Aha! At this point we know that the source is stored on page 128 – how about we take a look at that page using DBCC PAGE? After reattaching the database, run:

```sql
dbcc traceon (3604)
dbcc page(Test2, 1, 128, 0)
```

Note that I'm using style 0 for the DBCC PAGE command. At this point, I just want to see the header – there just might be something interesting in there:

image_22.png

As expected, it's a normal data page, as indicated by the m_type field having a value of 1 (which is the internal page type ID for a data page). More interesting though, we can see that the page belongs to object ID 60! Let's have a look at what lies behind that object ID:

```sql
select * from sys.sysobjects where id = 60
```

image_24.png

And all of a sudden, the hunt is on! Let's have a look at the contents of sys.sysobjvalues. Note that before you can select from this table, you'll have to connect using a [dedicated administrator connection](http://msdn.microsoft.com/en-us/library/ms189595.aspx), seeing as it's an internal base table:

```sql
select * from sys.sysobjvalues
```

image_28.png

There's obviously a lot of stuff in here we don't care about, but let's try and filter that objid column down to the object ID of our procedure – 2105058535:

```sql
select * from sys.sysobjvalues where objid = 2105058535
```

image_30.png

I wonder what that imageval column contains, if I remember correctly 0x2D2D would be "--" in ASCII, which reminds me quite a lot of the beginning of the XYZ procedure. Let's try and convert that column into human:

```sql
select convert(varchar(max), imageval) from sys.sysobjvalues where objid = 2105058535
```

image_32.png

And there you have it my dear reader; the source code for the XYZ stored procedure, as stored in the sys.sysobjvalues base table. As a final example, here's how you'd retrieve a list of user stored procedures with their source code, without using neither object_definition nor sys.sql_modules:

```sql
select
	p.name,
	cast(v.imageval as varchar(MAX))
from
	sys.procedures p
inner join
	sys.sysobjvalues v on p.object_id = v.objid
```

image_34.png

Want to see more stuff like this? Don't miss my [full-day precon at SQL Saturday #162 in Cambridge, UK](/presenting-at-sqlsaturday-162-in-cambridge) (Friday, September 7th), or my [Revealing the Magic session at Bleeding Edge 2012](/presenting-at-bleeding-edge-in-slovenia) in Laško, Slovenia (October 23-24th)!
