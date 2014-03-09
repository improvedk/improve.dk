permalink: sql-server-datetime-rounding-made-easy
title: SQL Server Datetime Rounding Made Easy
date: 2006-12-13
tags: [SQL Server]
---
**Update:** As noted in the comments, this method does not actually round, it truncates the datetime value.

<!-- more -->

As feature complete as MS SQL Server may be, it really lacks some simple datetime rounding functions. Here is a simple trick to easily round of datetimes at any precision you wish.

We will be using two builtin functions, namely the [DATEADD](http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tsqlref/ts_da-db_3vtw.asp) and the [DATEDIFF](http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tsqlref/ts_da-db_5vxi.asp) function.

We can round off at nearly any precision that SQL Server itself supports, for instance: Minute, Day, Hour, Day, Month, Year and so forth.

In this example I'll select the original "Created" column from the table tblHits, as well as a rounding of the Created column by the day:

```sql

SELECT Created,
	DATEADD(Day, DATEDIFF(Day, 0, Created), 0) AS CreatedDay
FROM tblHits

```

It'll return the following original / rounded values:

```

19-11-2006 22:39:27 -> 19-11-2006 00:00:00
20-11-2006 02:27:31 -> 20-11-2006 00:00:00
...

```

And naturally we can do the same, this time rounded by the hour:

```sql

SELECT Created,
	DATEADD(Hour, DATEDIFF(Hour, 0, Created), 0) AS CreatedHour
FROM tblHits

```

It'll return the following:

```

19-11-2006 22:39:27 -> 19-11-2006 22:39:00
20-11-2006 02:27:31 -> 20-11-2006 02:27:00
...

```

You could of course also do the rounding on the webserver after you've selected the database results. Sometimes though, we have to do it at the database level. For instance, if I want to know how many visitors I have per day on my blog:

```sql

SELECT COUNT(1) AS Visitors,
	DATEADD(Day, DATEDIFF(Day, 0, Created), 0) AS Date
FROM tblHits
GROUP BY DATEADD(Day, DATEDIFF(Day, 0, Created), 0)
ORDER BY Date DESC

```

Now, I know that this is not optimal in terms of performance since we're doing the DATEADD(DATEDIFF()) trick twice to both select it and group by it, but I've kept it this way to follow the [KISS principle](http://en.wikipedia.org/wiki/KISS_principle).
