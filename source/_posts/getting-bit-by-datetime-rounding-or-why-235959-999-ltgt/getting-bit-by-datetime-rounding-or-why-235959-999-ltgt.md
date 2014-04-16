permalink: getting-bit-by-datetime-rounding-or-why-235959-999-ltgt
title: Getting Bit by Datetime Rounding or Why 23:59:59.999 < '23:59:59.999'
date: 2011-06-16
tags: [SQL Server - Internals]
---
Earlier today I was doing some ad-hoc querying to retrieve some numbers for the month of May. Not giving it deeper thought, I made a simple query like this:

<!-- more -->

```sql
SELECT
	SUM(SomeColumn)
FROM
	SomeTable
WHERE
	SomeDatetime BETWEEN '2011-05-01' AND '2011-05-31 23:59:59.999'
```

Much to my surprise, the last rows looked like this:

image_2.png

Why in the world are results from June included when I had an explicit predicate limiting the results to May? The answer can be found in one of my [earlier posts on parsing dates](/parsing-dates-in-orcamdf). As SQL Server stores the millisecond part of a datetime with a precision of 1/300th of a second, with .997 being the highest possible stored value. .998 will be rounded down to .997 while .999 will be rounded up – causing a rollover of the day part.

Let's setup a simple sample data set:

```sql
CREATE TABLE DateTest
(
	ID int,
	Created datetime
)

INSERT INTO
	DateTest (ID, Created)
VALUES 
	(1, '2011-05-31 23:59:59.996'),
	(2, '2011-05-31 23:59:59.997'),
	(3, '2011-05-31 23:59:59.998'),
	(4, '2011-05-31 23:59:59.999')
```

Performing my simple query reveals the same problem as earlier today:

```sql
SELECT
	*
FROM
	DateTest
WHERE
	Created BETWEEN '2011-05-01' AND '2011-05-31 23:59:59.999'
```

image_4.png

Row number 4 is inserted with a date of 2011-06-01 00:00:00.000 since the .999 millisecond part causes a day rollover. Equally, the .999 value causes the last predicate part to be interpreted as 2011-06-01 00:00:00.000 during the CONVERT_IMPLICIT conversion.

A simple rewrite of the query guarantees to return just the results we want:

```sql
SELECT
	*
FROM
	DateTest
WHERE
	Created >= '2011-05-01' AND Created < '2011-06-01'
```

image_10.png
