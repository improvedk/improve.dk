permalink: reading-bits-in-orcamdf
title: Reading Bits in OrcaMDF
date: 2011-05-12
tags: [SQL Server - Internals, SQL Server - OrcaMDF]
---
Bits are stored very differently from other fixed length data types in SQL Server. Usually all fixed length columns will be present, one after the other, in the fixed data part of a record. As the smallest unit of data we can write to disk is a byte, the naïve approach to storing bits would be to use a whole bit for each bit. It would be very simple to parse as it would follow the usual scheme, but it would also waste quite some space.

## How are bits stored internally on records?

Instead, several bit columns values are stored in a single byte, up to a total of 8 max, naturally. Say we had a table definition like this:

<pre lang="tsql" escaped="true">CREATE TABLE BitTest
(
	A bit
	B bit
	C bit
	D int
)</pre>

The fixed length data part of our record would be 5 bytes, 4 for the integer column and a single byte, of which only three bits are used, for the bit columns.

[<img class="alignnone size-full wp-image-2206" alt="image_2" src="http://improve.dk/wp-content/uploads/2011/05/image_27.png" width="363" height="179" />](http://improve.dk/wp-content/uploads/2011/05/image_27.png)

Let’s add some more columns:

<pre lang="tsql" escaped="true">CREATE TABLE BitTest
(
	A bit
	B bit
	C bit
	D int
	E bit
	F bit
	G bit
	H smallint
	I bit
	J bit
	K bit
)</pre>

The bit columns E-G’s ordinal position is after D, but they’ll continue to use the first “bit byte” until it’s full. The following diagram shows that the H smallint column is stored directly after the int column, and not until we add the 9th bit is a new bit byte added:

[<img class="alignnone size-full wp-image-2207" alt="image_4" src="http://improve.dk/wp-content/uploads/2011/05/image_43.png" width="486" height="234" />](http://improve.dk/wp-content/uploads/2011/05/image_43.png)

## The need for state while reading bits from records

Obviously we can’t just read one field at a time, incrementing the fixed length data offset pointer for each read, as we usually do for normal fixed length data types. We need some kind of state that will tell us which byte we’re currently reading bits from, and when to read a new bit byte. Allow me to introduce RecordReadState:

```csharp
public class RecordReadState
{
	// We start out having consumed all bits as none have been read
	private int currentBitIndex = 8;
	private byte bits;

	public void LoadBitByte(byte bits)
	{
		this.bits = bits;
		currentBitIndex = 0;
	}

	public bool AllBitsConsumed
	{
		get { return currentBitIndex == 8; }
	}

	public bool GetNextBit()
	{
		return (bits & (1 << currentBitIndex++)) != 0;
	}
}
```

RecordReadState is currently only used for handling bits, but I’ve decided on not creating a BitReadState as we may need to save further read state further along. RecordReadState holds a single byte as well as a pointer that points to the next available bit in that byte. If the byte is exhausted (currentBixIndex = 8 (0-7 being the available bits)), AllBitsConsumed will return true, indicating we need to read in a new bit byte. GetNextBit simply reads the current bit from the bit byte, after which it increases the current bit index. [See the tests](https://github.com/improvedk/OrcaMDF/blob/58250bef24265900b6d94ec90be41b0647508b35/src/OrcaMDF.Core.Tests/Engine/Records/RecordReadStateTests.cs" target="_blank) for demonstration.

## Implementing SqlBit

Once we have the read state implemented, we can implement the SqlBit type:

```csharp
public class SqlBit : ISqlType
{
	private readonly RecordReadState readState;

	public SqlBit(RecordReadState readState)
	{
		this.readState = readState;
	}

	public bool IsVariableLength
	{
		get { return false; }
	}

	public short? FixedLength
	{
		get
		{
			if (readState.AllBitsConsumed)
				return 1;

			return 0;
		}
	}

	public object GetValue(byte[] value)
	{
		if(readState.AllBitsConsumed && value.Length != 1)
			throw new ArgumentException("All bits consumed, invalid value length: " + value.Length);

		if (value.Length == 1)
			readState.LoadBitByte(value[0]);

		return readState.GetNextBit();
	}
}
```

SqlBit requires a read state in the constructor, which is scoped for the current record read operation. It’s important to note that the FixedLength depends on the current AllBitsConsumed value of the read state. Once all bits have been consumed, the current bit field will technically have a length of one – causing a byte to be read. If it’s zero, no bytes will be read, but GetValue will still be invoked. GetValue asserts that there are bits available in case a byte wasn’t read (value.Length == 0). If a value was read, we ask the read state to load a new bit byte, after which we can call GetNextBit to return the current bit from the read state. [See tests of SqlBit](https://github.com/improvedk/OrcaMDF/blob/58250bef24265900b6d94ec90be41b0647508b35/src/OrcaMDF.Core.Tests/Engine/SqlTypes/SqlBitTests.cs" target="_blank).
