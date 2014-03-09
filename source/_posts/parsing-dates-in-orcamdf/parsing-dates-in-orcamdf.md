permalink: parsing-dates-in-orcamdf
title: Parsing Dates in OrcaMDF
date: 2011-05-10
tags: [SQL Server - Internals, SQL Server - OrcaMDF]
---
There are several different date related data types in SQL Server. Currently [OrcaMDF](http://improve.dk/archive/2011/05/03/introducing-orcamdf.aspx" target="_blank) supports the three most common types: [date](http://msdn.microsoft.com/en-us/library/bb630352.aspx" target="_blank), [datetime](http://msdn.microsoft.com/en-us/library/ms187819.aspx" target="_blank) & [smalldatetime](http://msdn.microsoft.com/en-us/library/ms182418.aspx" target="_blank).

<!-- more -->

## Implementing SqlDate

The simplest of the three is date – it’s a 3 byte fixed length type that stores the number of days passed since the default value of 1900-01-01. The only tricky part is that .NET does not have any standard representation of three byte integer values, only shorts & ints which are either too large or too small. Thus, to read the number of days correctly, we’ll have to perform some shift magic to get the correct number into a .NET four byte integer. Once we’ve got the date, we can just create a new default DateTime and add the number of days.

```csharp
public class SqlDate : ISqlType
{
	public bool IsVariableLength
	{
		get { return false; }
	}

	public short? FixedLength
	{
		get { return 3; }
	}

	public object GetValue(byte[] value)
	{
		if (value.Length != 3)
			throw new ArgumentException("Invalid value length: " + value.Length);

		// Magic needed to read a 3 byte integer into .NET's 4 byte representation.
		// Reading backwards due to assumed little endianness.
		int date = (value[2] << 16) + (value[1] << 8) + value[0];

		return new DateTime(1, 1, 1).AddDays(date);
	}
}
```

You can see the [relevant tests here](https://github.com/improvedk/OrcaMDF/blob/58250bef24265900b6d94ec90be41b0647508b35/src/OrcaMDF.Core.Tests/Engine/SqlTypes/SqlDateTests.cs" target="_blank).

## Adding time – implementing SqlDateTime

Whereas date only stores the date, datetime also stores a time factor. Datetime is stored as a fixed length 8 byte value, the first being the time part while the second is the date part. Calculating the date is done more or less the same way as in the date example, except this time it’s stored as a normal four byte integer, so it’s much easier to handle. The time part is stored as the number of clock ticks since midnight, with one tick being 1/300th of a second. To represent the tick value, we first define a constant with the value 10d/3d.

All time values are actually stored in the same integer time value, so to access the individual values, we’ll need to perform some division & modulus.

<table border="0" cellspacing="0" cellpadding="2" width="400">
	<tbody>
		<tr>
			<td valign="top" width="200">**Part**</td>
			<td valign="top" width="200">**Calculation**</td>
		</tr>
		<tr>
			<td valign="top" width="200">Hours</td>
			<td valign="top" width="200">X / 300 / 60 / 60</td>
		</tr>
		<tr>
			<td valign="top" width="200">Minutes</td>
			<td valign="top" width="200">X / 300 / 60 % 60</td>
		</tr>
		<tr>
			<td valign="top" width="200">Seconds</td>
			<td valign="top" width="200">X / 300 % 60</td>
		</tr>
		<tr>
			<td valign="top" width="200">Milliseconds</td>
			<td valign="top" width="200">X % 300 * 10d / 3d</td>
		</tr>
	</tbody>
</table>

```csharp
public class SqlDateTime : ISqlType
{
	private const double CLOCK_TICK_MS = 10d/3d;

	public bool IsVariableLength
	{
		get { return false; }
	}

	public short? FixedLength
	{
		get { return 8; }
	}

	public object GetValue(byte[] value)
	{
		if (value.Length != 8)
			throw new ArgumentException("Invalid value length: " + value.Length);

		int time = BitConverter.ToInt32(value, 0);
		int date = BitConverter.ToInt32(value, 4);

		return new DateTime(1900, 1, 1, time/300/60/60, time/300/60%60, time/300%60, (int)Math.Round(time%300*CLOCK_TICK_MS)).AddDays(date);
	}
}
```

You can see the [relevant tests here](https://github.com/improvedk/OrcaMDF/blob/58250bef24265900b6d94ec90be41b0647508b35/src/OrcaMDF.Core.Tests/Engine/SqlTypes/SqlDateTimeTests.cs" target="_blank).

## Last but not least, SqlSmallDateTime

Smalldatetime is brilliant when you need to store a date with limited range (~1900 - ~2079) and a precision down to one second. For most purposes, a time precision of one second is plenty, and we save a lot of space by limiting the precision and date range. A smalldatetime value takes up just 4 bytes, the first two being the number of minutes since midnight, and the last two being the number of days since the default values of 1900-1-1. The math processing done is the same as with datetime, though at a smaller scale.

<table border="0" cellspacing="0" cellpadding="2" width="400">
	<tbody>
		<tr>
			<td valign="top" width="200">**Part**</td>
			<td valign="top" width="200">**Calculation**</td>
		</tr>
		<tr>
			<td valign="top" width="200">Hours</td>
			<td valign="top" width="200">X / 60</td>
		</tr>
		<tr>
			<td valign="top" width="200">Minutes</td>
			<td valign="top" width="200">X % 60</td>
		</tr>
	</tbody>
</table>

```csharp
public class SqlSmallDateTime : ISqlType
{
	public bool IsVariableLength
	{
		get { return false; }
	}

	public short? FixedLength
	{
		get { return 4; }
	}

	public object GetValue(byte[] value)
	{
		if (value.Length != 4)
			throw new ArgumentException("Invalid value length: " + value.Length);

		ushort time = BitConverter.ToUInt16(value, 0);
		ushort date = BitConverter.ToUInt16(value, 2);

		return new DateTime(1900, 1, 1, time / 60, time % 60, 0).AddDays(date);
	}
}
```

You can see the [relevant tests here](https://github.com/improvedk/OrcaMDF/blob/58250bef24265900b6d94ec90be41b0647508b35/src/OrcaMDF.Core.Tests/Engine/SqlTypes/SqlSmallDateTimeTests.cs" target="_blank).
