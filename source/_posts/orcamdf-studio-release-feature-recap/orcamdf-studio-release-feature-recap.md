permalink: orcamdf-studio-release-feature-recap
title: OrcaMDF Studio Release + Feature Recap
date: 2011-11-25
tags: [SQL Server - OrcaMDF]
---
Just about two and a half months have passed since I last posted an [OrcaMDF feature recap](/orcamdf-feature-recap). Since then I’ve been busy attending three of the top SQL Server conferences – SQLBits, PASS and SQL Rally. It’s been excellent chatting about [OrcaMDF](/introducing-orcamdf) and getting some feedback on where to take it, thanks to all of you!

<!-- more -->

Though I’ve been busy, I’ve also managed to put some work into OrcaMDF in the meantime.

## New features

A non-exhaustive list of additions since my last post:

* [Support for multi-file databases](/orcamdf-now-supports-databases-with-multiple-data-files).
* [Exposing metadata through standard SQL Server DMVs](/orcamdf-now-exposes-metadata-through-system-dmvs).
* Optimized performance by more light weight usage of byte arrays, as well as sharing a single instance of the table schema between all rows.

I’ve also added support for the following extra data types:

* money
* smallmoney
* uniqueidentifier
* User defined types

And just one more minor thing...

## OrcaMDF Studio

Common among much of the feedback I got was something along the lines of: Awesome project! I definitely want to try it out, but I haven’t had the time yet.

To be honest, it’s been somewhat cumbersome to try out OrcaMDF. You had to download the source, compile it yourself and then write your own application to actually invoke OrcaMDF. It’s never been my intention for it to be directly end user usable, as that’s just not my focus. However, to improve upon OrcaMDF, I need more usage feedback. As a result, I decided to create OrcaMDF Studio – an UI on top of OrcaMDF, allowing you to query user tables, dmvs and base tables without looking at any of the code at all. What you’re seeing here is OrcaMDF showing the contents of the Product table in a standard AdventureWorks 2008 database:

image_43.png

### Base tables

OrcaMDF Studio exposes all of the base tables (that it currently supports & parses), just like normal tables:

image_12.png

### Dynamic Management Views

OrcaMDF also exposes all of the currently supported DMVs, just like any other table:

image_14.png

### User tables, indexes & schemas

Finally, OrcaMDF also exposes all of the user tables, including their schemas, indexes and index schemas:

image_161.png

### Error reporting

As OrcaMDF, and the Studio, is still far from production ready, you’ll probably run into unsupported scenarios or common bugs. If you do, OrcaMDF Studio will report it to you, as well as save a stack trace in the application directory. Here’s an example of a typical error – trying to open a table with an unsupported column type (xml for example):

image_18.png

If you look in the ErrorLog.txt file that’s written to the application directory, you’ll see an exception like this:

```
25-11-2011 00:41:21
----------
System.ArgumentException: Unsupported type: xml(-1)
   at OrcaMDF.Core.MetaData.DataColumn..ctor(String name, String type, Boolean nullable) in D:ProjectsOrcaMDFsrcOrcaMDF.CoreMetaDataDataColumn.cs:line 135
   at OrcaMDF.Core.MetaData.DataColumn..ctor(String name, String type) in D:ProjectsOrcaMDFsrcOrcaMDF.CoreMetaDataDataColumn.cs:line 20
   at OrcaMDF.Core.MetaData.DatabaseMetaData.GetEmptyDataRow(String tableName) in D:ProjectsOrcaMDFsrcOrcaMDF.CoreMetaDataDatabaseMetaData.cs:line 155
   at OrcaMDF.Core.Engine.DataScanner.ScanTable(String tableName) in D:ProjectsOrcaMDFsrcOrcaMDF.CoreEngineDataScanner.cs:line 31
   at OrcaMDF.OMS.Main.loadTable(String table) in D:ProjectsOrcaMDFsrcOrcaMDF.OMSMain.cs:line 158
```

Completely anonymous data that just gives an indication of where the error occurred. If you run into issues like these, I would appreciate if you would send me the ErrorLog.txt file so I can debug potential issues. All issues, requests, error reports, etc. should be sent to my email.

### Database version support

OrcaMDF has been developed for, and tested with, SQL Server 2008 R2. Some operations will work on SQL Server 2005, but many will fail since some of the base tables have different schemas. OrcaMDF does not differ and will thus throw up once you hit those differences. I’ll add an abstraction layer + support for both at a later time.

### Opening live databases

OrcaMDF Studio will have to take a read lock on the database file(s) for now. As SQL Server takes exclusive locks on the files, this means you can’t open a live database. You can either detach the database, take it offline, backup-restore-detach or simply use a non-attached database to begin with. For a later release, I’ll add automatic VSS snapshot functionality.

### System requirements

OrcaMDF is built on top of the [.NET Framework 4.0](http://www.microsoft.com/download/en/details.aspx?id=17851) – as such, you will need to have it installed for OrcaMDF Studio to run. OrcaMDF Studio will run on both x64 and x86 machines.

### Disclaimer

Once again, OrcaMDF + Studio is experimental software. I give no guarantees whatsoever, and you’re using it at your own risk. OrcaMDF will not write anything to disk and will thus not, in any way, modify your database files. However, I strongly suggest that you **do not** use this on a production database, either way.

OrcaMDF neither knows nor cares about security. No matter who owns that schema or object, OrcaMDF will parse it just fine – no need for pesky usernames and passwords. However, OrcaMDF does not support [Transparent Data Encryption](http://msdn.microsoft.com/en-us/library/bb934049.aspx), so databases using TDE will not be supported.

### Download

You can download the OrcaMDF Studio binary release at the [OrcaMDF Github project page](https://github.com/improvedk/OrcaMDF/downloads). Please don’t ever download OrcaMDF anywhere else, you risk that someone modifies the source and distributes their own version. **Always** get it from the Github project page.

Once you download it, simply unpack it and run the OrcaMDF.OMS.exe application:

image_221.png

Note that this is a debug build – hence the .pdf files. Debug builds will be slightly slower than release builds, but it does enable me to get full stack traces if/when an exception occurs. Once OrcaMDF stabilizes, I’ll provide both debug and release versions.
