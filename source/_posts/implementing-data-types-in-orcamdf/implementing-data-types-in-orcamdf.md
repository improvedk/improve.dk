permalink: implementing-data-types-in-orcamdf
title: Implementing Data Types in OrcaMDF
date: 2011-05-05
tags: [SQL Server - Internals, SQL Server - OrcaMDF]
---
Implementing parsing support for SQL Server data types in [OrcaMDF](/introducing-orcamdf) is a simple matter of implementing the ISqlType interface:

<!-- more -->

```csharp
public interface ISqlType
{
	bool IsVariableLength { get; }
	short? FixedLength { get; }
	object GetValue(byte[] value);
}
```

*IsVariableLength* returns whether this data type has a fixed length size or is variable. *FixedLength* returns the fixed length of the data type, provided that it is fixed length, otherwise it returns null. The data type parser itself does not care about the length of variable length fields, the size of the input bytes will determine that. Finally *GetValue* parses the input bytes into and converts them into a .NET object of relevant type.

## SqlInt implementation

int is very simple as it’s fixed length and is very straight forward to convert using BitConverter:

```csharp
public class SqlInt : ISqlType
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

		return BitConverter.ToInt32(value, 0);
	}
}
```

And the related tests:

```csharp
[TestFixture]
public class SqlIntTests
{
	[Test]
	public void GetValue()
	{
		var type = new SqlInt();
		byte[] input;

		input = new byte[] { 0x5e, 0x3b, 0x27, 0x2a };
		Assert.AreEqual(707214174, Convert.ToInt32(type.GetValue(input)));

		input = new byte[] { 0x8d, 0xf9, 0xaa, 0x30 };
		Assert.AreEqual(816511373, Convert.ToInt32(type.GetValue(input)));

		input = new byte[] { 0x7a, 0x4a, 0x72, 0xe2 };
		Assert.AreEqual(-495826310, Convert.ToInt32(type.GetValue(input)));
	}

	[Test]
	public void Length()
	{
		var type = new SqlInt();

		Assert.Throws<ArgumentException>(() => type.GetValue(new byte[3]));
		Assert.Throws<ArgumentException>(() => type.GetValue(new byte[5]));
	}
}
```

## SqlNVarchar implementation

nvarchar is very simple as well – note that we return null for the length as the length varies and the ISqlType implementation must be stateless. GetValue simply converts whatever amount of input bytes it gets into the relevant .NET data type, string in this case.

```csharp
public class SqlNVarchar : ISqlType
{
	public bool IsVariableLength
	{
		get { return true; }
	}

	public short? FixedLength
	{
		get { return null; }
	}

	public object GetValue(byte[] value)
	{
		return Encoding.Unicode.GetString(value);
	}
}
```

And the relevant test in this case:

```csharp
[TestFixture]
public class SqlNvarcharTests
{
	[Test]
	public void GetValue()
	{
		var type = new SqlNVarchar();
		byte[] input = new byte[] { 0x47, 0x04, 0x2f, 0x04, 0xe6, 0x00 };

		Assert.AreEqual("u0447u042fu00e6", (string)type.GetValue(input));
	}
}
```

## Other implementations

OrcaMDF currently [supports 12 data types](https://github.com/improvedk/OrcaMDF/tree/2b2403c4422cc47b309857d42fb182970bbe11d8/src/OrcaMDF.Core/Engine/SqlTypes) out of the box. I’ll be covering datetime and bit later as those are a tad special compared to the rest of the current types. As the remaining types are implemented, I will be covering those too.
