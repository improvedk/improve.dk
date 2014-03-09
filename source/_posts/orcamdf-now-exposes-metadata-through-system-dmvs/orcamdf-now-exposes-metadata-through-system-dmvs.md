permalink: orcamdf-now-exposes-metadata-through-system-dmvs
title: OrcaMDF Now Exposes Metadata Through System DMVs
date: 2011-11-10
tags: [SQL Server - OrcaMDF]
---
I’m sitting here on the train in Denmark, on the final leg home from [SQLRally Nordic](http://www.sqlpass.org/sqlrally/2011/" target="_blank). During my presentation based on my OrcaMDF work, I implicitly announced that OrcaMDF now exposes metadata – thougt I might as well share here as well. Other than expanding the core engine support in OrcaMDF, one of the main features I’ve wanted to implement was a way for OrcaMDF to expose metadata about your database. How do you list the tables, indexes, columns, etc. from your database?

<!-- more -->

## Avoiding false abstractions

My initial thought was to create my own abstraction layer on top of the objects. You could get the list of user tables by accessing the database.GetMetadata().UserTables enumeration, you’d get a list of tables, including columns, etc. This has a very clean interface from the development side, everything being normal .NET objects. However, it would also require me to come up with said abstraction – and where do I draw the line on what to expose and what to keep internal? What if my abstraction didn’t feel natural to DBAs, being used to the sys.* DMVs from SQL Server?

## Exposing the built-in DMVs from SQL Server

I spent some time considering who might end up using OrcaMDF – and concluded there might be just about four persons in the world, and those four would be split evenly between DBA and SQL Server dev. Common for those is that they’re already used to navigating the metadata of SQL Server databses through system DMVs like sys.tables, sys.columns, sys.indexes etc. What then struck me was that I’m already able to parse all of the base tables in SQL Server, and using OBJECT_DEFINITION, I can get the source code of the built-in system DMVs. As such, it was a simple matter of creating my own replicas of the built-in DMVs.

## How to use the DMVs in OrcaMDF

Say we wanted to retrieve all the columns for a given data in SQL Server, we create a query like this:

```sql
SELECT
	c.*
FROM
	sys.columns c
INNER JOIN
	sys.tables t ON c.object_id = t.object_id
WHERE
	t.name = 'Persons'
```

Doing the same in OrcaMDF could look like this:

```csharp
using (var db = new Database(new[] { @"C:Test.mdf" }))
{
	var sys = db.Dmvs;

	var table = sys.Tables.Where(t => t.Name == "Persons").Single();
	var columns = sys.Columns.Where(c => c.ObjectID == table.ObjectID);

	foreach (var col in columns)
		Console.WriteLine(col.Name);
}
```

And if you prefer the more SQL-esque syntax of LINQ, you can of course do it like this as well:

```csharp
using (var db = new Database(new[] { @"C:Test.mdf" }))
{
	var sys = db.Dmvs;

	var columns =	from c in sys.Columns
			join t in sys.Tables on c.ObjectID equals t.ObjectID
			where t.Name == "Persons"
			select c;

	foreach (var col in columns)
		Console.WriteLine(col.Name);
}
```

No matter how you choose to do it, this is the result:

image_22.png

## What’s available at this point

If you grab the [latest commit of OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank), you’ll have access to the following DMVs, just as they’re exposed through SQL Server:


* sys.columns
* sys.indexes
* sys.index_columns
* sys.objects
* sys.objects$
* sys.system_internals_allocation_units
* sys.system_internals_partitions
* sys.system_internals_partition_columns
* sys.tables
* sys.types


More is definitely on their way. Let me know if you have a special wish for a DMV – I might just be able to make your wish come true!
