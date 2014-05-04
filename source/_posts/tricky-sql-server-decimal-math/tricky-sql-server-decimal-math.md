---
permalink: tricky-sql-server-decimal-math
title: Tricky SQL Server Decimal Math
date: 2009-05-06
tags: [SQL Server - Data Types, SQL Server - Optimization]
---
SQL Server datatypes are not always what they seem to be. [Martin Schmidt](http://www.performanceduo.com/) recently had an [interesting blog post (in danish)](http://www.performanceduo.com/post/Gc3a6t-en-Datatype-.aspx) regarding implicit decimal conversion that sparked my interest.

<!-- more -->

Let me sketch up the scenario. We have a simple table with a decimal column like so:

```sql
CREATE TABLE tblDecimalTest
(
	DecimalColumn dec(5,2) NOT NULL
)
```

Note that the DecimalColumn has a precision of five and a scale of two. That basically boils down to 999.99 being the largest number we can store and -999.99 being the smallest. The precision of five defines the maximum number of digits in the number, scale defines the number of digits to the right of the decimal point. If we insert an integer value of 999 it'll have .00 stored implicity, thus we can't insert neither 1000 nor 10000 without any decimal digits. Knowing the configured precision and scale is important, as we'll see in just a moment.

Let us insert a single row into the tblDecimalTest table.

```sql
DECLARE @Decimal1 dec(5,2) = 20.5
DECLARE @Decimal2 dec(5,2) = 27.52
INSERT INTO tblDecimalTest (DecimalColumn) VALUES (@Decimal1 / @Decimal2)
```

This is the result if we perform a select on the table:

```sql
SELECT * FROM tblDecimalTest

DecimalColumn
0.74
```

Both decimal variables were declared as dec(5,2) so it matches the column in the table. Calculating 20.5 / 27.52 on a standard calculator gives a result of 0.7449127, but as we're storing this with a scale of two, the value is rounded off to 0.74.

We just inserted a value of 20.5 / 27.52 into a dec(5,2) column. Let's make a select using those same variables:

```sql
DECLARE @Decimal1 dec(5,2) = 20.5
DECLARE @Decimal2 dec(5,2) = 27.52
SELECT * FROM tblDecimalTest WHERE DecimalColumn = @Decimal1 / @Decimal2

Results:
(0 row(s) affected)
```

What is that? No results! Why does this happen? After all, we just inserted @Decimal1 / @Decimal2, so surely we should be able to select that row again? The key lies in how SQL Server [converts decimal datatypes during math operations](http://msdn.microsoft.com/en-us/library/ms190476.aspx). What we're looking for is the divison operator which defines the following precision and scale calculations:

```
e1 / e2:
Result precision = p1 - s1 + s2 + max(6, s1 + p2 + 1)
Result scale = max(6, s1 + p2 + 1)
```

Let's input our values into that formula.

```
dec(5,2) / dec(5,2):
Precision	= p1-s1+s2 + max(6, s1+p2+1)	= 5-2+2 + max(6, 2+5+1)		= 5 + max(6,8)	= 5+8 = 13
Scale		= max(6, s1+p2+1)		= max(6, 2+5+1)			= max(6,8)	= 8
Type		= dec(13,8)
```

Thus, our division of two dec(5,2) variables is implicitly converted into a dec(13,8) value! Similar conversions are made for addition, subtraction and multiplication.

```
dec(5,2) + dec(5,2), dec(5,2) - dec(5,2):
Precision	= max(s1, s2) + max(p1-s1, p2-s2) + 1	= max(5, 5) + max(3, 3) + 1	= 5+3+1	= 9
Scale		= max(2, 2)				= 2
Type		= dec(9,2)

dec(5,2) * dec(5,2):
Precision	= p1+p2+1	= 5+5+1	= 11
Scale		= s1+2		= 2+2	= 4
Type		= dec(11,4)
```

Let's try and check the division result directly:

```sql
DECLARE @Decimal1 dec(5,2) = 20.5
DECLARE @Decimal2 dec(5,2) = 27.52
SELECT @Decimal1 / @Decimal2

(No column name)
0.74491279
```

When performing the WHERE clause, we're in fact comparing a dec(5,2) column with a dec(13,8) value. Behind the scenes, SQL Server will implicitly convert the values to a common datatype that fits both - which is dec(13,8). With a precision of 13 and a scale of 8, 0.74 and 0.74491279 are not equal, and thus we don't get any results back. If we were to cast the divison as a dec(5,2) explicitly, we would find the row:

```sql
DECLARE @Decimal1 dec(5,2) = 20.5
DECLARE @Decimal2 dec(5,2) = 27.52
SELECT * FROM tblDecimalTest WHERE DecimalColumn = CAST(@Decimal1 / @Decimal2 AS dec(5,2))

DecimalColumn
0.74
```

While testing in SQL Server Management Studio, this might be an obvious problem. When encountering the same problem from code, it's much more difficult to notice - especially if you don't know the precise schema you're working against. Observe the following code working on an empty tblDecimalTest table.

```cs
using(SqlConnection conn = new SqlConnection(@"Data Source=.SQL2008;Initial Catalog=Test;Integrated Security=SSPI"))
{
	using(SqlCommand cmd = conn.CreateCommand())
	{
		decimal dec1 = 20.5M;
		decimal dec2 = 27.52M;

		cmd.CommandText = "INSERT INTO tblDecimalTest (DecimalColumn) VALUES (@DecimalValue)";
		cmd.Parameters.Add("@DecimalValue", SqlDbType.Decimal).Value = dec1 / dec2;

		conn.Open();
		cmd.ExecuteNonQuery();

		cmd.CommandText = "SELECT COUNT(*) FROM tblDecimalTest WHERE DecimalColumn = @DecimalValue";
		Console.WriteLine("Implicit cast: " + cmd.ExecuteScalar());

		cmd.CommandText = "SELECT COUNT(*) FROM tblDecimalTest WHERE DecimalColumn = CAST(@DecimalValue as dec(5,2))";
		Console.WriteLine("Explicit cast: " + cmd.ExecuteScalar());
	}
}
```

The result:

```
Implicit cast: 0
Explicit cast: 1
```

Without knowledge of the schema, and how SQL Server treats decimal math operations, this could've been a tough bug to track down.
