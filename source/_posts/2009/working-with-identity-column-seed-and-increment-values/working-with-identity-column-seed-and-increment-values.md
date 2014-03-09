permalink: working-with-identity-column-seed-and-increment-values
title: Working With Identity Column Seed & Increment Values
date: 2009-10-28
tags: [SQL Server - Tricks]
---
All of the following samples are based on the following table:

<pre lang="tsql" escaped="true">CREATE TABLE [dbo].[tblCars]
(
	[CarID] [int] IDENTITY(2,5) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
)</pre>

## Find identity column seed and increment values

We can use the [IDENT_SEED](http://msdn.microsoft.com/en-us/library/ms189834.aspx), [IDENT_INCR](http://msdn.microsoft.com/en-us/library/ms189795.aspx) and [IDENT_CURRENT](http://msdn.microsoft.com/en-us/library/ms175098.aspx) functions to retrieve the identity seed and increment values, as well as the current value. Note that the next row will have IDENT_CURRENT() + IDENT_INCR() as its identity value.

<pre lang="tsql" escaped="true">SELECT
	IDENT_SEED('tblCars') AS Seed,
	IDENT_INCR('tblCars') AS Increment,
	IDENT_CURRENT('tblCars') AS CurrentIdentity</pre>

Result:

<pre lang="tsql" escaped="true">Seed	Increment	CurrentIdentity
2		5			17</pre>

An alternative way is to query the [sys.identity_columns](http://technet.microsoft.com/en-us/library/ms187334.aspx) system view for the same values. Note that the sys.columns view (of which sys.identity_columns inherit) has an object_id column specifying the object ID of the table to which the column belongs. Thus we'll have to apply a predicate filtering away any columns not belonging to the desired table, tblCars in this example.

<pre lang="tsql" escaped="true">SELECT
	seed_value AS Seed,
	increment_value AS Increment,
	last_value AS CurrentIdentity
FROM
	sys.identity_columns
WHERE
	object_id = OBJECT_ID('tblCars')</pre>

Result:

<pre lang="tsql" escaped="true">Seed	Increment	CurrentIdentity
2		5			17</pre>

A third way of finding the current identity value is to use the [DBCC CHECKIDENT](http://technet.microsoft.com/en-us/library/ms176057.aspx) function:

<pre lang="tsql" escaped="true">DBCC CHECKIDENT(tblCars, NORESEED)</pre>

Result:

<pre lang="tsql" escaped="true">Checking identity information: current identity value '22', current column value '22'.
DBCC execution completed. If DBCC printed error messages, contact your system administrator.</pre>

## Changing the seed value

Using the DBCC CHECKIDENT command we can manually apply a new seed value to our table. Note that this will enable you to set an identity value that'll cause the identity column to have duplicates unless you have a unique index on the column, in which case you'll get an error instead. Thus, if you manually reseed the table, make sure you won't run into duplicate values.

<pre lang="tsql" escaped="true">DBCC CHECKIDENT(tblCars, RESEED, 500)</pre>

Result:

<pre lang="tsql" escaped="true">Checking identity information: current identity value '22', current column value '500'.
DBCC execution completed. If DBCC printed error messages, contact your system administrator.</pre>

If for some reason the identity value has become out of synch with the values in the table, we can automatically reseed the table to a valid identity value. In the following case I've manually set the seed to 10 while the highest identity value in the table is 27. After running RESEED with no explicit value, the seed is automatically set to 27, thus the next inserted row will have an identity value of 32, provided the increment is 5.

<pre lang="tsql" escaped="true">DBCC CHECKIDENT(tblCars, RESEED)</pre>

Result:

<pre lang="tsql" escaped="true">Checking identity information: current identity value '10', current column value '27'.
DBCC execution completed. If DBCC printed error messages, contact your system administrator.</pre>

## Getting the maximum and minimum identity values

Using the IDENTITYCOL alias for any identity column in a table (of which there can be at most one), we can easily select the maximum and minimum identity values:

<pre lang="tsql" escaped="true">SELECT
	MAX(IDENTITYCOL) AS MaximumIdentity,
	MIN(IDENTITYCOL) AS MinimumIdentity
FROM
	tblCars</pre>

Result:

<pre lang="tsql" escaped="true">MaximumIdentity	MinimumIdentity
27				22</pre>

## Changing the identity increment value

Unfortunately there's no easy way to change the increment value of an identity column. The only way to do so is to drop the identity column and add a new column with the new increment value. The following code will create a new temporary table, copy the data into it, recreate the original table with the correct increment value and then finally copy the data back using [SET IDENTITY_INSERT ON](http://msdn.microsoft.com/en-us/library/aa259221(SQL.80).aspx) to insert explicit values into the identity column.

<pre lang="tsql" escaped="true">BEGIN TRAN

-- Create new temporary table to hold data while restructuring tblCars
CREATE TABLE tblCars_TMP
(
	CarID int NOT NULL,
	Name nvarchar(50) NOT NULL
)

-- Insert tblCars data into tblCars_TMP
INSERT INTO tblCars_TMP SELECT * FROM tblCars

-- Drop original table
DROP TABLE tblCars

-- Create new tblCars table with correct identity values (1,1) in this case
CREATE TABLE [dbo].[tblCars]
(
	[CarID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
)

-- Reinsert data into tblCars table
SET IDENTITY_INSERT tblCars ON
INSERT INTO tblCars (CarID, Name) SELECT CarID, Name FROM tblCars_TMP
SET IDENTITY_INSERT tblCars OFF

COMMIT</pre>
