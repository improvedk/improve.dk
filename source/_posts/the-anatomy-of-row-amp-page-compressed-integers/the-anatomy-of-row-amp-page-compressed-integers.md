permalink: the-anatomy-of-row-amp-page-compressed-integers
title: The Anatomy of Row & Page Compressed Integers
date: 2012-01-30
tags: [SQL Server - Internals]
---
While working on row compression support for [OrcaMDF](https://github.com/improvedk/OrcaMDF), I ran into some challenges when trying to parse integers. Contrary to normal non-compressed integer storage, these are all variable width – meaning an integer with a value of *50* will only take up one byte, instead of the usual four. That wasn't new though, seeing as [vardecimals are also stored as variable width](/how-are-vardecimals-stored). What is different however is the way the numbers are stored on disk. Note that while I was only implementing row compression, the integer compression used in page compression is exactly the same, so this goes for both types of compression.

<!-- more -->

## Tinyint

Tinyint is pretty much identical to the usual tinyint storage. The only exception being that a value of 0 will take up no bytes when row compressed, where as in non-compressed storage it'll store the value 0x0, taking up a single byte. All of the integer types are the same regarding 0-values – the value is indicated by the compression row metadata and thus requires no actual value stored.

## Smallint

Let's start out by looking at a normal non-compressed smallint, for the values –2, –1, 1 and 2. As mentioned before, 0 isn't interesting as nothing is stored. Note that all of these values are represented exactly as they're stored on disk – in this case they're stored in [little endian](http://en.wikipedia.org/wiki/Endianness).

```csharp
-2	=	0xFEFF
-1	=	0xFFFF
1	=	0x0100
2	=	0x0200
```

Starting with the values 1 and 2, they're very straightforward. Simply convert it into decimal and you've got the actual number. –1 however, is somewhat different. The value 0xFFFF converted to decimal is 65.535 – the maximum value we can store in an unsigned two-byte integer. The SQL Server range for a smallint is –32.768 to 32.767.

Calculating the actual values relies on what's called [integer overflows](http://en.wikipedia.org/wiki/Integer_overflow). Take a look at the following C# snippet:

```csharp
unchecked
{
	Console.WriteLine(0 + (short)32767);
	Console.WriteLine(0 + (short)32768);
	Console.WriteLine(0 + (short)32769);
	// ...
	Console.WriteLine(0 + (short)65534);
	Console.WriteLine(0 + (short)65535);
}
```

The output is as follows:

```csharp
32767
-32768
-32767
-2
-1
```

If we start with the value 0 and add the maximum value for a signed short, 32.767, we obviously end up with just that – 32.767. However, if we add 32.768, which is outside the range of the short, we rollover and end up with the smallest possible short value. Since these are constant numbers, the compiler won't allow the overflow – unless we encapsulate our code in an unchecked {} section.

You may have heard of the fabled [sign bit](http://en.wikipedia.org/wiki/Sign_bit). Basically it's the highest order bit that's being used to designate whether a number is positive or negative. As special sounding as it is, it should be obvious from the above that the sign bit isn't special in any way – though it can be queried to determine the sign of a given number. Take a look at what happens to the sign bit when we overflow:

```csharp
32.767	=	0b0111111111111111
-32.768	=	0b1000000000000000
-32.767	=	0b1000000000000001
```

For the number to become large enough for it to cause an overflow, the high order "sign bit" needs to be set. It isn't magical in any way, it's simply used to cause the overflow.

OK, so that's some background information on how normal non-compressed integers are stored. Now let's have a look at how those same smallint values are stored in a row compressed table:

```csharp
-2	=	0x7E
-1	=	0x7F
1	=	0x81
2	=	0x82
```

Let's try and convert those directly to decimal, as we did before:

```csharp
-2	=	0x7E	=	126
-1	=	0x7F	=	127
1	=	0x81	=	129
2	=	0x82	=	130
```

Obviously, these are not stored the same way. The immediate difference is that we're now only using a single byte – due to the variable-width storage nature. When parsing these values, we should simply look at the number of byte stored. If it's using a single byte, we know it's in the 0 to 255 range (for tinyints) or –128 to 127 range for smallints. Smallints in that range will be stored using a single signed byte.

If we use the same methodology as  before, we obviously get the wrong results.1 <> 0 + 129. The trick in this case is to treat the stored values as unsigned integers, and then minimum value as the offset. That is, instead of using 0 as the offset, we'll use the signed 1-byte minimum value of –128 as the offset:

```csharp
-2	=	0x7E	=	-128 + 126
-1	=	0x7F	=	-128 + 127
1	=	0x81	=	-128 + 129
2	=	0x82	=	-128 + 130
```

Aha, so that must mean we'll need to store two bytes as soon as we exceed the signed 1-byte range, right? Right!

image_2.png

One extremely important difference is that the non-compressed values will always be stored in little endian on disk, whereas the row compressed integers are stored using big endian! So not only do they use different offset values, they also use different endianness. The end result is the same, but the calculations involved are dramatically different.

## Int & bigint

Once I figured out the endianness and number scheme of the row-compressed integer values, int and bigint were straightforward to implement. As with the other types, they're still variable width so you may have a 5-byte bigint as well as a 1-byte int. Here's the main parsing code for my SqlBigInt type implementation:

```csharp
switch (value.Length)
{
	case 0:
		return 0;

	case 1:
		return (long)(-128 + value[0]);

	case 2:
		return (long)(-32768 + BitConverter.ToUInt16(new[] { value[1], value[0] }, 0));

	case 3:
		return (long)(-8388608 + BitConverter.ToUInt32(new byte[] { value[2], value[1], value[0], 0 }, 0));

	case 4:
		return (long)(-2147483648 + BitConverter.ToUInt32(new[] { value[3], value[2], value[1], value[0] }, 0));

	case 5:
		return (long)(-549755813888 + BitConverter.ToInt64(new byte[] { value[4], value[3], value[2], value[1], value[0], 0, 0, 0 }, 0));

	case 6:
		return (long)(-140737488355328 + BitConverter.ToInt64(new byte[] { value[5], value[4], value[3], value[2], value[1], value[0], 0, 0 }, 0));

	case 7:
		return (long)(-36028797018963968 + BitConverter.ToInt64(new byte[] { value[6], value[5], value[4], value[3], value[2], value[1], value[0], 0 }, 0));

	case 8:
		return (long)(-9223372036854775808 + BitConverter.ToInt64(new[] { value[7], value[6], value[5], value[4], value[3], value[2], value[1], value[0] }, 0));

	default:
		throw new ArgumentException("Invalid value length: " + value.Length);
}
```

The *value* variable is a byte array containing the bytes as stored on disk. If the length is 0, nothing is stored and hence we know it has a value of 0. For each of the remaining valid lengths, it's simply a matter of using the smallest representable number as the offset and then adding the stored value onto it.

For non-compressed values we can use the BitConverter class directly as it expects the input value to be in system endianness – and for most Intel and AMD systems, that'll be little endian (which means OrcaMDF won't run on a big endian system!). However, as the compressed values are stored in big endian, I have to remap the input array into little endian format, as well as pad the 0-bytes so it matches up with the short, int and long sizes.

For the shorts and ints I'm reading unsigned values in, as that's really what I'm interested in. This works since int + uint is coerced into a long value. I can't do the same for the long's since there's no data type larger than a long. For the maximum long value of 9.223.372.036.854.775.807, what's actually stored on disk is 0xFFFFFFFFFFFFFFFF. Parsing that as a signed long value using BitConverter results in the value –1 due to the overflow. Wrong as that may be, it all works out in the end due to an extra negative overflow:

```csharp
-9.223.372.036.854.775.808 + 0xFFFFFFFFFFFFFF =>
-9.223.372.036.854.775.808 + -1 =
9.223.372.036.854.775.807
```

## Conclusion

As usual, I've had a lot of fun trying to figure out how the bytes on disk ended up as the values I saw when performing a SELECT query. It doesn't take long to realize that while the excellent Internals book really takes you far, there's so much more to dive into.
