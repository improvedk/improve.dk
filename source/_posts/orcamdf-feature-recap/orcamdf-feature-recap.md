permalink: orcamdf-feature-recap
title: OrcaMDF Feature Recap
date: 2011-09-10
tags: [SQL Server - OrcaMDF]
---
Time flies – it's been about four months since I originally [introduced my pet project, OrcaMDF](/introducing-orcamdf). Since then, quite a lot has happened and OrcaMDF is somewhat more capable than when it started out. As a result I thought I'd provide a recap of what OrcaMDF is currently capable of, as well as what my plans are for the future.

<!-- more -->

## Page types

OrcaMDF currently supports full parsing of Data, Index, TextMix, TextTree, GAM, SGAM, IAM, and PFS pages. It also supports minimal parsing of the Boot page, used for finding the starting point of the base table metadata.

Remaining are Sort, FileHeader, DiffMap and MLMap pages. As MLMap and DiffMap use the same bitmap format as IAM, GAM and SGAM pages, parsing those should be straightforward. FileHeader is a bit more tricky and will require some DBCC PAGE love. Sort pages are less relevant as those are only used temporarily while SQL Server is running and should thus not be stored in your MDF file.

## Data types

I've been adding more and more data types to OrcaMDF, lately I've added support for parsing all of the LOB types except for XML. Currently supported data are:

* bigint
* binary
* bit
* char
* datetime
* decimal
* image
* int
* nchar
* ntext
* nvarchar(x)
* nvarchar(MAX)
* smallint
* sysname
* text
* tinyint
* varbinary(x)
* varbinary(MAX)
* varchar(x)
* varchar(MAX)

Adding support for further data types is relatively easy, it's just a matter of analyzing the storage format and [implementing the ISqlType interface](/implementing-data-types-in-orcamdf).

## Table & index structures

Using the DataScanner class, OrcaMDF is able to scan both clustered tables as well as heaps. Using the IndexScanner class enables you to scan nonclustered indexes, whether they're applied to a clustered table or a heap.

## Metadata

The only publicly exposed metadata that OrcaMDF currently exposes is a list of table names. Internally, OrcaMDF is able to parse both indexes, tables, partitions, allocation units and columns. This enables you to scan a clustered table/heap/index, providing just its name. OrcaMDF will automatically parse the schema of the object and discover where the IAM chain starts (for heaps) or find the root page for indexes.

## Notable leftovers

OrcaMDF currently only supports single-file databases, that is, no secondary data files. Adding support would be trivial, but my efforts are concentrated on supporting the core data structures in data files, so secondary files wouldn't change much. As for corrupted files & corruption detection, OrcaMDF assumes the MDF files to be in perfect condition. There are many places where corruption could be detected, but I'll postpone that until the day I feel OrcaMDF is able to parse most functioning databases correctly. The projected start out (and still is) as a way of getting a deeper understanding of SQL Server internals, and as such, corruption detection is less important at this stage – though it's definitely something I'd like to add eventually.

## What's coming next

I want to extend the current metadata parsing capabilities, especially with focus on exposing the metadata publicly. Through OrcaMDF, it should be possible to reproduce the database/object/column tree as seen in SQL Server Management Studio. I want to provide table names, indexes, schemas, keys, etc. I also want to take a look at compression, starting out with row compression. As the format is rather well documented (compared to LOB structures, for example), it shouldn't pose too many problems.

If you have any suggestions or features you'd love to see supported, do let me know!

I've also had a number of requests for examples of how to use OrcaMDF. I'll probably end up creating a series of blog posts showcasing examples of how to use various features of OrcaMDF through code. I'm also planning to create a post on how to fetch the source code and compile it and finally how to run it from there – just to make it a tad easier to take it for a spin :)
