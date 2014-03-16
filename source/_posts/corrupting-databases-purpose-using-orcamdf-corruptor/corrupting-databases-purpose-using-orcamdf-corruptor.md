permalink: corrupting-databases-purpose-using-orcamdf-corruptor
title: Corrupting Databases on Purpose Using the OrcaMDF Corruptor
date: 2013-11-05
tags: [.NET, SQL Server - Internals, SQL Server - OrcaMDF, SQL Server]
---
Sometimes you must first do evil, to do good. Such is the case when you want to hone your skills in corruption recovery of SQL Server databases.

<!-- more -->

To give me more material to test the new [RawDatabase](/orcamdf-rawdatabase-a-swiss-army-knife-for-mdf-files/) functionality, I've now added a [Corruptor class](https://github.com/improvedk/OrcaMDF/blob/master/src/OrcaMDF.Framework/Corruptor.cs) to OrcaMDF. Corruptor does more or less what the name says - it corrupts database files on purpose.

The corruption itself is quite simple. Corruptor will choose a number of random pages and simply overwrite the page completely with all zeros. Depending on what pages are hit, this can be quite fatal.

I shouldn't have to say this, but just in case... Please do not use this on anything valuable. **It will fatally corrupt your data.**

## Examples

There are two overloads for the Corruptor.CorruptFile method, both of them return an IEnumerable of integers - a list of the page IDs that have been overwritten by zeros.

The following code will corrupt 5% of the pages in the AdventureWorks2008R2LT.mdf file, after which it will output each page ID that has been corrupted. You can specify the percentage of pages to corrupt by changing the second parameter.

```csharp
var corruptedPageIDs = Corruptor.CorruptFile(@"C:\AdventureWorks2008R2LT.mdf", 0.05);
Console.WriteLine(string.Join(", ", corruptedPageIDs));
```

```
606, 516, 603, 521, 613, 621, 118, 47, 173, 579,
323, 217, 358, 515, 615, 271, 176, 596, 417, 379,
269, 409, 558, 103, 8, 636, 200, 361, 60, 486,
366, 99, 87
```

To make the corruption hit even harder, you can also use the second overload of the CorruptFile method, allowing you to specify the exact number of pages to corrupt, within a certain range of page IDs. The following code will corrupt exactly 10 pages within the first 50 pages (zero-based), thus hitting mostly metadata.

```csharp
var corruptedPageIDs = Corruptor.CorruptFile(@"C:\AdventureWorks2008R2LT.mdf", 10, 0, 49);
Console.WriteLine(string.Join(", ", corruptedPageIDs));
```

```
16, 4, 0, 32, 15, 14, 30, 2, 49, 9
```

In the above case I was extraordinarily unlucky seeing as page 0 is the file header page, page 2 is the first GAM page, page 9 is the boot page and finally page 16 is the page that contains the allocation unit metadata. With corruption like this, you can be certain that DBCC CHECKDB will be giving up, leaving you with no other alternative than to restore from a backup.

Or... You could try to recover as much data as possible using [OrcaMDF RawDatabase](/orcamdf-rawdatabase-a-swiss-army-knife-for-mdf-files/), but I'll get back to that later :)