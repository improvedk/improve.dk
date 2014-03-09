permalink: weighted-random-selections-in-sql-server
title: Weighted Random Selections in SQL Server
date: 2007-11-25
tags: [SQL Server]
---
## UPDATE

After testing my code through based on JP’s comments, I’ve realized my implementation was way too naïve and cannot be used for most datasets. For a correct weighted random implementation, see [Dems’ answer on StackOverflow](http://stackoverflow.com/questions/58457/random-weighted-choice-in-t-sql/454454#454454" target="_blank).

## Original (flawed) implementation

There are no built-in functions for selecting weighted averages in SQL Server. Fortunately it's a simple task to do so oneself.

We'll use this table as an example:

```tsql
CREATE TABLE #tmp
(
	Name varchar(64),
	Points int
)

INSERT INTO #tmp VALUES ('Mark', 25);
INSERT INTO #tmp VALUES ('Jakob', 12);
INSERT INTO #tmp VALUES ('Peter', 17);
INSERT INTO #tmp VALUES ('Anders', 0);
INSERT INTO #tmp VALUES ('Kirsten', 33);
INSERT INTO #tmp VALUES ('Mads', 4);
```

This table represents a list of players in an arbitrary game. The more points you have, the bigger the chance of winning. It has to be weighted, meaning that the person with just 4 points may win, but is unlikely to do so.

The RAND() function in SQL Server returns a floating point number between 0 and 1. Multiplying that with our points gives a random weight based on the amount of points. Unfortunately the RAND() function is seeded once for each query, not for each row - meaning that for each row RAND() will yield the same result, effectively multiplying the points with a constant all the way through. We need to provide a new seed for the RAND() function for each row. NEWID() returns a new unique identifier that may be used as a seed if cast to VARBINARY:

```tsql
SELECT Name, Points, RAND(CAST(NEWID() AS VARBINARY)) * Points AS Weight FROM #tmp ORDER BY Weight DESC

Name     Points  Weight
Peter    17      15,9795741766356
Mark     25      14,9122204505153
Kirsten  33      9,67888480542761
Jakob    12      9,38697608441358
Mads     4       0,833340539027792
Anders   0       0
```

And here we have the result ordered by weight. As you can see, although Kirsten has the most points, Peter ended up winning the competition.

## FIG1: Showing the statistical distribution of using RAND(NEWID())

```tsql
DECLARE @SampleSize int = 100000;

WITH RND AS
(
	select top (@SampleSize) ROUND(RAND(CAST(NEWID() AS VARBINARY)), 1, 1) AS RandValue from sys.objects cross join sys.columns
)
SELECT
	COUNT(*),
	RandValue,
	COUNT(*) / CAST(@SampleSize AS float) * 100 AS [%]
FROM
	RND
GROUP BY
	RandValue
ORDER BY
	COUNT(*) DESC
```

Results:

```tsql
COUNT	VAL	%
10117	0,8	10,117
10091	0,4	10,091
10073	0	10,073
10034	0,9	10,034
9996	0,5	9,996
9993	0,2	9,993
9956	0,7	9,956
9927	0,6	9,927
9923	0,3	9,923
9890	0,1	9,89
```
