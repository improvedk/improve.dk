permalink: saving-space-by-storing-real-values-in-smallints
title: Saving Space by Storing Decimal Values in Integer Data Types
date: 2011-05-31
tags: [SQL Server - Internals]
---
I recently stumbled upon a [question on Stack Overflow](http://stackoverflow.com/questions/6015605/design-question-on-storing-meteorological-data-on-sql-server-2008/6016237#6016237" target="_blank) on how best to reduce their data size as it’s growing out of hand. As the original author hasn’t replied back yet (as of writing this post, I’m making some assumptions on the scenario – so take it as an abstract scenario). The basic scenario is that they have a number of measuring stations, each one of those containing a lot of equipment reporting back to a SQL Server in a schema like the following abstract:

<!-- more -->

```sql
CREATE TABLE Measurements
(
	DataID bigint IDENTITY,
	StationID int,
	MeasurementA real,
	MeasurementB real,
	MeasurementC real
	... 100 more columns
)
```

They’re willing to loose some precision of the data, for the purpose of saving space. As some of the data is measuring wind speed in meters/sec and air pressure, I’m making the assumptions that most of the data will be in the 0-200 and 500-2000 ranges, depending on the scale used.

If the wind speed does not need accuracy further than two decimals, storing it in a 4 byte real column is a lot of waste. Instead we might store it in a smallint column, saving 2 bytes per column. The data would be converted like so:

```sql
35.7   => 35.7   * 100 = 3,570
1.38   => 1.38   * 100 = 138
155.29 => 155.29 * 100 = 15,529
84.439 => 84.439 * 100 = 8,443 (with the .9 being rounded off due to integer math)
```

So by multiplying all the values by 100, we achieve a precision of two decimal points, with all further decimal points being cropped. As the smallint max value is 32,767, the maximum value we could store in this format would be:

```sql
327.67 => 327.67 * 100 = 32,767
```

Which is probably enough for most wind measurements. Hopefully.

For the larger values in the 500-2000 ranges, we can employ the same technique by multiplying by 10. This only gives us a single digit of precision, but allows for values in the –3,276.8 to 3,276.7 range, stored using just 2 bytes per column. Employing the same technique we could also store values between 0 and 2.55 in a single byte tinyint column, with a precision of two digits.

Unless you really need to save those bytes, I wouldn’t recommend you do this as it’s usually better to store the full precision. However, this does show that we can store decimals in integer data types with a bit of math involved.
