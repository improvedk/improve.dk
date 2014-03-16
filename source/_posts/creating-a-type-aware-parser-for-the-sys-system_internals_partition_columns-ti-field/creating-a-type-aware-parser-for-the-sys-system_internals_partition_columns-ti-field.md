permalink: creating-a-type-aware-parser-for-the-sys-system_internals_partition_columns-ti-field
title: Creating a Type Aware Parser for the sys.system_internals_partition_columns.ti Field
date: 2011-07-13
tags: [SQL Server - Internals]
---
Based on my findings [exploring the sys.system_internals_partition_columns.ti field](/exploring-the-sys-system_internals_partition_columns-ti-field), I needed parser that could extract the scale, precision, max_length as well as the max_inrow_length fields from it. The tricky part is that those values are stored differently for each individual type, added onto the fact that some types have hardcoded defaults that are not stored in the ti field, even though there’s space for it.

<!-- more -->

As a result of some reverse engineering and empirical testing, I’ve made a SysrscolTIParser class that takes in the ti value (I have no idea what the acronym stands for – type information perhaps?), determines the type and parses it corresponding to the type. I won’t go into details as that’s all described in my [previous post](/exploring-the-sys-system_internals_partition_columns-ti-field).

```csharp
using System;
using OrcaMDF.Core.MetaData.Enumerations;

namespace OrcaMDF.Core.MetaData
{
	public class SysrscolTIParser
	{
		public byte Scale;
		public byte Precision;
		public short MaxLength;
		public short MaxInrowLength;
		public int TypeID;

		public SysrscolTIParser(int ti)
		{
			TypeID = ti & 0xFF;

			if (!Enum.IsDefined(typeof(SystemType), TypeID))
				throw new ArgumentException("Unknown TypeID '" + TypeID + "'");

			switch((SystemType)TypeID)
			{
				case SystemType.Bigint:
					MaxLength = MaxInrowLength = 8;
					Precision = 19;
					break;

				// All CLR types internally stored as varbinaries
				//case SystemType.Geography:
				//case SystemType.Geometry:
				//case SystemType.Hierarchyid:
				case SystemType.Varbinary:
				// Also covers SystemType.Sysname
				case SystemType.Nvarchar:
				case SystemType.Binary:
				case SystemType.Char:
				case SystemType.Nchar:
				case SystemType.Image:
				case SystemType.Ntext:
				case SystemType.Text:
				case SystemType.Varchar:
				case SystemType.Xml:
					MaxLength = (short)((ti & 0xFFFF00) >> 8);
					if (MaxLength == 0)
					{
						MaxLength = -1;
						MaxInrowLength = 8000;
					}
					else
						MaxInrowLength = MaxLength;
					break;

				case SystemType.Bit:
					MaxLength = MaxInrowLength = Precision = 1;
					break;

				case SystemType.Date:
					Precision = 10;
					MaxLength = MaxInrowLength = 3;
					break;

				case SystemType.Datetime:
					Scale = 3;
					Precision = 23;
					MaxLength = MaxInrowLength = 8;
					break;

				case SystemType.Datetime2:
					Scale = (byte)((ti & 0xFF00) >> 8);
					Precision = (byte)(20 + Scale);
					if (Scale < 3)
						MaxLength = MaxInrowLength = 6;
					else if (Scale < 5)
						MaxLength = MaxInrowLength = 7;
					else
						MaxLength = MaxInrowLength = 8;
					break;

				case SystemType.DatetimeOffset:
					Scale = (byte)((ti & 0xFF00) >> 8);
					Precision = (byte)(26 + (Scale > 0 ? Scale + 1 : Scale));
					if (Scale < 3)
						MaxLength = MaxInrowLength = 8;
					else if (Scale < 5)
						MaxLength = MaxInrowLength = 9;
					else
						MaxLength = MaxInrowLength = 10;
					break;

				case SystemType.Decimal:
				case SystemType.Numeric:
					Precision = (byte)((ti & 0xFF00) >> 8);
					Scale = (byte)((ti & 0xFF0000) >> 16);
					if (Precision < 10)
						MaxLength = MaxInrowLength = 5;
					else if (Precision < 20)
						MaxLength = MaxInrowLength = 9;
					else if (Precision < 29)
						MaxLength = MaxInrowLength = 13;
					else
						MaxLength = MaxInrowLength = 17;
					break;

				case SystemType.Float:
					Precision = 53;
					MaxLength = MaxInrowLength = 8;
					break;
					
				case SystemType.Int:
					Precision = 10;
					MaxLength = MaxInrowLength = 4;
					break;

				case SystemType.Money:
					Scale = 4;
					Precision = 19;
					MaxLength = MaxInrowLength = 8;
					break;

				case SystemType.Real:
					Precision = 24;
					MaxLength = MaxInrowLength = 4;
					break;

				case SystemType.Smalldatetime:
					Precision = 16;
					MaxLength = MaxInrowLength = 4;
					break;

				case SystemType.Smallint:
					Precision = 5;
					MaxLength = MaxInrowLength = 2;
					break;

				case SystemType.Smallmoney:
					Scale = 4;
					Precision = 10;
					MaxLength = MaxInrowLength = 4;
					break;

				case SystemType.Sql_Variant:
					MaxLength = MaxInrowLength = 8016;
					break;

				case SystemType.Time:
					Scale = (byte)((ti & 0xFF00) >> 8);
					Precision = (byte)(8 + (Scale > 0 ? Scale + 1 : Scale));
					if (Scale < 3)
						MaxLength = MaxInrowLength = 3;
					else if (Scale < 5)
						MaxLength = MaxInrowLength = 4;
					else
						MaxLength = MaxInrowLength = 5;
					break;

				case SystemType.Timestamp:
					MaxLength = MaxInrowLength = 8;
					break;

				case SystemType.Tinyint:
					Precision = 3;
					MaxLength = MaxInrowLength = 1;
					break;

				case SystemType.Uniqueidentifier:
					MaxLength = MaxInrowLength = 16;
					break;

				default:
					throw new ArgumentException("TypeID '" + TypeID + "' not supported.");
			}
		}
	}
}
```

It uses a SystemType enumeration for switching between the types (sorry, formatting isn’t being nice to me here):

```csharp
namespace OrcaMDF.Core.MetaData.Enumerations
{
	public enum SystemType
	{
		Image				= 34,
		Text				= 35,
		Uniqueidentifier		= 36,
		Date				= 40,
		Time				= 41,
		Datetime2			= 42,
		DatetimeOffset			= 43,
		Tinyint				= 48,
		Smallint			= 52,
		Int				= 56,
		Smalldatetime			= 58,
		Real				= 59,
		Money				= 60,
		Datetime			= 61,
		Float				= 62,
		Sql_Variant			= 98,
		Ntext				= 99,
		Bit				= 104,
		Decimal				= 106,
		Numeric				= 108,
		Smallmoney			= 122,
		Bigint				= 127,
		Hierarchyid			= 240,
		Geometry			= 240,
		Geography			= 240,
		Varbinary			= 165,
		Varchar				= 167,
		Binary				= 173,
		Char				= 175,
		Timestamp			= 189,
		Nvarchar			= 231,
		Nchar				= 239,
		Xml				= 241,
		Sysname				= 231
	}
}
```

And last, but not least, a bunch of tests to verify the functionality:

```csharp
using NUnit.Framework;
using OrcaMDF.Core.MetaData;

namespace OrcaMDF.Core.Tests.MetaData
{
	[TestFixture]
	public class SysrscolTIParserTests
	{
		[Test]
		public void Bigint()
		{
			var parser = new SysrscolTIParser(127);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(19, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(127, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);
		}

		[Test]
		public void Binary()
		{
			var parser = new SysrscolTIParser(12973);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(50, parser.MaxLength);
			Assert.AreEqual(173, parser.TypeID);
			Assert.AreEqual(50, parser.MaxInrowLength);
		}

		[Test]
		public void Bit()
		{
			var parser = new SysrscolTIParser(104);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(1, parser.Precision);
			Assert.AreEqual(1, parser.MaxLength);
			Assert.AreEqual(104, parser.TypeID);
			Assert.AreEqual(1, parser.MaxInrowLength);
		}

		[Test]
		public void Char()
		{
			var parser = new SysrscolTIParser(2735);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(10, parser.MaxLength);
			Assert.AreEqual(175, parser.TypeID);
			Assert.AreEqual(10, parser.MaxInrowLength);
		}

		[Test]
		public void Date()
		{
			var parser = new SysrscolTIParser(40);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(10, parser.Precision);
			Assert.AreEqual(3, parser.MaxLength);
			Assert.AreEqual(40, parser.TypeID);
			Assert.AreEqual(3, parser.MaxInrowLength);
		}

		[Test]
		public void Datetime()
		{
			var parser = new SysrscolTIParser(61);
			Assert.AreEqual(3, parser.Scale);
			Assert.AreEqual(23, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(61, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);
		}

		[Test]
		public void Datetime2()
		{
			var parser = new SysrscolTIParser(1834);
			Assert.AreEqual(7, parser.Scale);
			Assert.AreEqual(27, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(42, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);

			parser = new SysrscolTIParser(810);
			Assert.AreEqual(3, parser.Scale);
			Assert.AreEqual(23, parser.Precision);
			Assert.AreEqual(7, parser.MaxLength);
			Assert.AreEqual(42, parser.TypeID);
			Assert.AreEqual(7, parser.MaxInrowLength);
		}

		[Test]
		public void Datetimeoffset()
		{
			var parser = new SysrscolTIParser(1835);
			Assert.AreEqual(7, parser.Scale);
			Assert.AreEqual(34, parser.Precision);
			Assert.AreEqual(10, parser.MaxLength);
			Assert.AreEqual(43, parser.TypeID);
			Assert.AreEqual(10, parser.MaxInrowLength);

			parser = new SysrscolTIParser(1067);
			Assert.AreEqual(4, parser.Scale);
			Assert.AreEqual(31, parser.Precision);
			Assert.AreEqual(9, parser.MaxLength);
			Assert.AreEqual(43, parser.TypeID);
			Assert.AreEqual(9, parser.MaxInrowLength);
		}

		[Test]
		public void Decimal()
		{
			var parser = new SysrscolTIParser(330858);
			Assert.AreEqual(5, parser.Scale);
			Assert.AreEqual(12, parser.Precision);
			Assert.AreEqual(9, parser.MaxLength);
			Assert.AreEqual(106, parser.TypeID);
			Assert.AreEqual(9, parser.MaxInrowLength);

			parser = new SysrscolTIParser(396138);
			Assert.AreEqual(6, parser.Scale);
			Assert.AreEqual(11, parser.Precision);
			Assert.AreEqual(9, parser.MaxLength);
			Assert.AreEqual(106, parser.TypeID);
			Assert.AreEqual(9, parser.MaxInrowLength);
		}

		[Test]
		public void Float()
		{
			var parser = new SysrscolTIParser(62);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(53, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(62, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);
		}

		[Test]
		public void Varbinary()
		{
			var parser = new SysrscolTIParser(165);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(-1, parser.MaxLength);
			Assert.AreEqual(165, parser.TypeID);
			Assert.AreEqual(8000, parser.MaxInrowLength);

			parser = new SysrscolTIParser(228517);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(892, parser.MaxLength);
			Assert.AreEqual(165, parser.TypeID);
			Assert.AreEqual(892, parser.MaxInrowLength);
		}

		[Test]
		public void Image()
		{
			var parser = new SysrscolTIParser(4130);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(16, parser.MaxLength);
			Assert.AreEqual(34, parser.TypeID);
			Assert.AreEqual(16, parser.MaxInrowLength);
		}

		[Test]
		public void Int()
		{
			var parser = new SysrscolTIParser(56);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(10, parser.Precision);
			Assert.AreEqual(4, parser.MaxLength);
			Assert.AreEqual(56, parser.TypeID);
			Assert.AreEqual(4, parser.MaxInrowLength);
		}

		[Test]
		public void Money()
		{
			var parser = new SysrscolTIParser(60);
			Assert.AreEqual(4, parser.Scale);
			Assert.AreEqual(19, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(60, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);
		}

		[Test]
		public void Nchar()
		{
			var parser = new SysrscolTIParser(5359);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(20, parser.MaxLength);
			Assert.AreEqual(239, parser.TypeID);
			Assert.AreEqual(20, parser.MaxInrowLength);
		}

		[Test]
		public void Ntext()
		{
			var parser = new SysrscolTIParser(4195);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(16, parser.MaxLength);
			Assert.AreEqual(99, parser.TypeID);
			Assert.AreEqual(16, parser.MaxInrowLength);
		}

		[Test]
		public void Numeric()
		{
			var parser = new SysrscolTIParser(265580);
			Assert.AreEqual(4, parser.Scale);
			Assert.AreEqual(13, parser.Precision);
			Assert.AreEqual(9, parser.MaxLength);
			Assert.AreEqual(108, parser.TypeID);
			Assert.AreEqual(9, parser.MaxInrowLength);

			parser = new SysrscolTIParser(135020);
			Assert.AreEqual(2, parser.Scale);
			Assert.AreEqual(15, parser.Precision);
			Assert.AreEqual(9, parser.MaxLength);
			Assert.AreEqual(108, parser.TypeID);
			Assert.AreEqual(9, parser.MaxInrowLength);
		}

		[Test]
		public void Nvarchar()
		{
			var parser = new SysrscolTIParser(25831);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(100, parser.MaxLength);
			Assert.AreEqual(231, parser.TypeID);
			Assert.AreEqual(100, parser.MaxInrowLength);

			parser = new SysrscolTIParser(231);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(-1, parser.MaxLength);
			Assert.AreEqual(231, parser.TypeID);
			Assert.AreEqual(8000, parser.MaxInrowLength);
		}

		[Test]
		public void Real()
		{
			var parser = new SysrscolTIParser(59);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(24, parser.Precision);
			Assert.AreEqual(4, parser.MaxLength);
			Assert.AreEqual(59, parser.TypeID);
			Assert.AreEqual(4, parser.MaxInrowLength);
		}

		[Test]
		public void Smalldatetime()
		{
			var parser = new SysrscolTIParser(58);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(16, parser.Precision);
			Assert.AreEqual(4, parser.MaxLength);
			Assert.AreEqual(58, parser.TypeID);
			Assert.AreEqual(4, parser.MaxInrowLength);
		}

		[Test]
		public void Smallint()
		{
			var parser = new SysrscolTIParser(52);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(5, parser.Precision);
			Assert.AreEqual(2, parser.MaxLength);
			Assert.AreEqual(52, parser.TypeID);
			Assert.AreEqual(2, parser.MaxInrowLength);
		}

		[Test]
		public void Smallmoney()
		{
			var parser = new SysrscolTIParser(122);
			Assert.AreEqual(4, parser.Scale);
			Assert.AreEqual(10, parser.Precision);
			Assert.AreEqual(4, parser.MaxLength);
			Assert.AreEqual(122, parser.TypeID);
			Assert.AreEqual(4, parser.MaxInrowLength);
		}

		[Test]
		public void Sql_Variant()
		{
			var parser = new SysrscolTIParser(98);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(8016, parser.MaxLength);
			Assert.AreEqual(98, parser.TypeID);
			Assert.AreEqual(8016, parser.MaxInrowLength);
		}

		[Test]
		public void Text()
		{
			var parser = new SysrscolTIParser(4131);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(16, parser.MaxLength);
			Assert.AreEqual(35, parser.TypeID);
			Assert.AreEqual(16, parser.MaxInrowLength);
		}

		[Test]
		public void Time()
		{
			var parser = new SysrscolTIParser(1833);
			Assert.AreEqual(7, parser.Scale);
			Assert.AreEqual(16, parser.Precision);
			Assert.AreEqual(5, parser.MaxLength);
			Assert.AreEqual(41, parser.TypeID);
			Assert.AreEqual(5, parser.MaxInrowLength);

			parser = new SysrscolTIParser(1065);
			Assert.AreEqual(4, parser.Scale);
			Assert.AreEqual(13, parser.Precision);
			Assert.AreEqual(4, parser.MaxLength);
			Assert.AreEqual(41, parser.TypeID);
			Assert.AreEqual(4, parser.MaxInrowLength);
		}

		[Test]
		public void Timestamp()
		{
			var parser = new SysrscolTIParser(189);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(8, parser.MaxLength);
			Assert.AreEqual(189, parser.TypeID);
			Assert.AreEqual(8, parser.MaxInrowLength);
		}

		[Test]
		public void Tinyint()
		{
			var parser = new SysrscolTIParser(48);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(3, parser.Precision);
			Assert.AreEqual(1, parser.MaxLength);
			Assert.AreEqual(48, parser.TypeID);
			Assert.AreEqual(1, parser.MaxInrowLength);
		}

		[Test]
		public void Uniqueidentifier()
		{
			var parser = new SysrscolTIParser(36);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(16, parser.MaxLength);
			Assert.AreEqual(36, parser.TypeID);
			Assert.AreEqual(16, parser.MaxInrowLength);
		}

		[Test]
		public void Varchar()
		{
			var parser = new SysrscolTIParser(12967);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(50, parser.MaxLength);
			Assert.AreEqual(167, parser.TypeID);
			Assert.AreEqual(50, parser.MaxInrowLength);

			parser = new SysrscolTIParser(167);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(-1, parser.MaxLength);
			Assert.AreEqual(167, parser.TypeID);
			Assert.AreEqual(8000, parser.MaxInrowLength);
		}

		[Test]
		public void Xml()
		{
			var parser = new SysrscolTIParser(241);
			Assert.AreEqual(0, parser.Scale);
			Assert.AreEqual(0, parser.Precision);
			Assert.AreEqual(-1, parser.MaxLength);
			Assert.AreEqual(241, parser.TypeID);
			Assert.AreEqual(8000, parser.MaxInrowLength);
		}
	}
}
```

All of this is just one big giant bit of infrastructure I needed to implement to get my scanning of nonclustered indexes working. As soon as I’ve got that up and running, all of this, plus a lot more, will be committed & pushed to the [OrcaMDF repo](https://github.com/improvedk/OrcaMDF).
