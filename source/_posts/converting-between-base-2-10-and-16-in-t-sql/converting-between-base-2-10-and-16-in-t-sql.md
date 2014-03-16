permalink: converting-between-base-2-10-and-16-in-t-sql
title: Converting Between Base 2, 10 and 16 in T-SQL
date: 2011-07-11
tags: [SQL Server - Tricks]
---
There are many [numeral systems](http://en.wikipedia.org/wiki/List_of_numeral_systems), the most common ones in computer science being [binary](http://en.wikipedia.org/wiki/Binary_numeral_system) (base 2), [decimal](http://en.wikipedia.org/wiki/Decimal) (base 10) and [hexadecimal](http://en.wikipedia.org/wiki/Hexadecimal) (base 16). All numbers can be expressed in either system and you may now and then need to convert between them.

<!-- more -->

Take the number 493.202.384 as an example, it can be be expressed as either 0n493202384 in decimal, 0x1D65ABD0 in hexadecimal or 0b11101011001011010101111010000 in binary. Note how the 0n prefix declares a decimal value, 0x a hexadecimal and 0b a binary value.

## Converting using Google

If you’ve got an internet connection, the quickest and simplest way is often to just use Google. We can convert the above number using “in X” queries:

[493202384 in hex](http://www.google.dk/search?sourceid=chrome&ie=UTF-8&q=493202384+in+hex)  
[493202384 in binary](http://www.google.dk/search?sourceid=chrome&ie=UTF-8&q=493202384+in+binary)

## Converting using Windows Calculator

You can also open Windows Calculator, switch to the programmer mode and type in the decimal value (or the hex/binary value):

image_25.png

And from then on we can just switch the numerical system selector to the left:

image_43.png

image_63.png

## Converting between decimal & hex in T-SQL

Sometimes however, it’s just a tad easier if we could do it directly from a T-SQL query. Converting between decimal and hexadecimal is straightforward and can be done using just built in functions:

```sql
-- Decimal to hex
SELECT CAST(493202384 AS varbinary)

-- Hex to decimal
SELECT CAST(0x1D65ABD0 AS int)

-- Decimal to hex to decimal
SELECT CAST(CAST(493202384 AS varbinary) AS int)
```

image_84.png

## Converting binary to decimal using T-SQL

Converting to/from binary is a bit more tricky though, as there are no built in functions for formatting a decimal number as a binary string, nor converting the latter to the first.

The following function takes in a binary string and returns a bigint with the decimal value:

```sql
CREATE FUNCTION [dbo].[BinaryToDecimal]
(
	@Input varchar(255)
)
RETURNS bigint
AS
BEGIN

	DECLARE @Cnt tinyint = 1
	DECLARE @Len tinyint = LEN(@Input)
	DECLARE @Output bigint = CAST(SUBSTRING(@Input, @Len, 1) AS bigint)

	WHILE(@Cnt < @Len) BEGIN
		SET @Output = @Output + POWER(CAST(SUBSTRING(@Input, @Len - @Cnt, 1) * 2 AS bigint), @Cnt)

		SET @Cnt = @Cnt + 1
	END

	RETURN @Output	

END
```

The function looks at each char in the input string (starting from behind), adding POWER(2, @Cnt) to the result if the bit is set – with special handling of the first (that is, from behind) character since POWER(2, 0) is 1 while we need it to be 0.

Usage is straight forward:

```sql
SELECT dbo.BinaryToDecimal('11101011001011010101111010000')
```

image_182.png

## Converting decimal to binary using T-SQL

The following function takes a bigint as input and returns a varchar with the binary representation, using the [short division by two with remainder](http://www.wikihow.com/Convert-from-Decimal-to-Binary) algorithm:

```sql
CREATE FUNCTION [dbo].[DecimalToBinary]
(
	@Input bigint
)
RETURNS varchar(255)
AS
BEGIN

	DECLARE @Output varchar(255) = ''

	WHILE @Input > 0 BEGIN

		SET @Output = @Output + CAST((@Input % 2) AS varchar)
		SET @Input = @Input / 2

	END

	RETURN REVERSE(@Output)

END
```

Again usage is straight forward:

```sql
SELECT dbo.DecimalToBinary(493202384)
```

image_123.png

## Ensuring correctness

A simple test to ensure correct conversions would be to convert from A to B and back to A again, using both of the above functions. Thus whatever we give as input should be the output as well:

```sql
SELECT dbo.DecimalToBinary(dbo.BinaryToDecimal('11101011001011010101111010000'))
SELECT dbo.BinaryToDecimal(dbo.DecimalToBinary(493202384))
```

image_202.png

Et voilá! Once we have the functions, they can easily be used in a normal query:

```sql
SELECT
	object_id,
	CAST(object_id AS varbinary) AS object_id_hex,
	dbo.DecimalToBinary(object_id) AS object_id_binary
FROM
	sys.objects
```

image_162.png
