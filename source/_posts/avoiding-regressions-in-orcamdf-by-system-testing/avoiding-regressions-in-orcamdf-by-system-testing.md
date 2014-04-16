permalink: avoiding-regressions-in-orcamdf-by-system-testing
title: Avoiding Regressions in OrcaMDF by System Testing
date: 2011-06-14
tags: [.NET, SQL Server - OrcaMDF, Testing]
---
As I continue to add new features & support for new data structures in [OrcaMDF](/introducing-orcamdf), the risk of [regressions](http://en.wikipedia.org/wiki/Software_regression) increase. Especially so as I'm developing in a largely unknown field, given that I can't plan for structures and relations that I do not yet know about. To reduce the risk of regressions, testing is an obvious need.

<!-- more -->

## Unit testing

[Unit testing](http://en.wikipedia.org/wiki/Unit_testing) is the process of testing the smallest parts of the code, which would be functions in object oriented programming. A sample test for the [SqlBigInt](https://github.com/improvedk/OrcaMDF/blob/694dd0cff213dc48b5153b040a41fdc707914680/src/OrcaMDF.Core/Engine/SqlTypes/SqlBigInt.cs) data type parsing class could look like this:

```csharp
using System;
using NUnit.Framework;
using OrcaMDF.Core.Engine.SqlTypes;

namespace OrcaMDF.Core.Tests.Engine.SqlTypes
{
	[TestFixture]
	public class SqlBigIntTests
	{
		[Test]
		public void GetValue()
		{
			var type = new SqlBigInt();
			byte[] input;

			input = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F };
			Assert.AreEqual(9223372036854775807, Convert.ToInt64(type.GetValue(input)));

			input = new byte[] { 0x82, 0x5A, 0x03, 0x1B, 0xD5, 0x3E, 0xCD, 0x71 };
			Assert.AreEqual(8200279581513702018, Convert.ToInt64(type.GetValue(input)));

			input = new byte[] { 0x7F, 0xA5, 0xFC, 0xE4, 0x2A, 0xC1, 0x32, 0x8E };
			Assert.AreEqual(-8200279581513702017, Convert.ToInt64(type.GetValue(input)));
		}

		[Test]
		public void Length()
		{
			var type = new SqlBigInt();

			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[9]));
			Assert.Throws<ArgumentException>(() => type.GetValue(new byte[7]));
		}
	}
}
```

This tests the main entrypoints for the SqlBigInt class, testing for over/underflow of the long bigint data type, as well as checking the length. This works great for simple classes like SqlBigInt. Unit testing more complex interrelated classes requires infrastructure support for mocking out called methods, related classes, etc. While this is a working strategy, it arguably requires some effort, especially at the early stage of a project where the architecture is dynamic.

## System testing

On the other end of the spectrum, we have [system testing](http://en.wikipedia.org/wiki/System_testing). System testing seeks to test the system as a whole, largely ignoring the inner workings of either system, which merits a categorization as [black-box testing](http://en.wikipedia.org/wiki/Black_box_testing). In the case of OrcaMDF I've estimated that I can catch 90% of all regressions using just 10% of the time, compared to unit testing which would have the reverse properties. As such, it's a great way to test during development, while allowing for the introduction of key unit & integration tests as necessary.

Say I wanted to test the parsing of user table names in the [DatabaseMetaData](https://github.com/improvedk/OrcaMDF/blob/694dd0cff213dc48b5153b040a41fdc707914680/src/OrcaMDF.Core/MetaData/DatabaseMetaData.cs) class, I could mock the values of the SysObjects list, while also mocking [MdfFile](https://github.com/improvedk/OrcaMDF/blob/694dd0cff213dc48b5153b040a41fdc707914680/src/OrcaMDF.Core/Engine/MdfFile.cs) as that's a require parameter for the constructor. To do that, I'd have to extract MdfFile into an interface and use a mocking framework on top of that.

Taking the system testing approach, I'm instead performing the following workflow:

* Connect to a running SQL Server.
* Create test schema in the fixture setup.
* Detach the database.
* Run OrcaMDF on the detached .mdf file and validate the results.

A sample test, creating two user tables and validating the output from the DatabaseMetaData, looks like this:

```csharp
using System.Data.SqlClient;
using NUnit.Framework;
using OrcaMDF.Core.Engine;

namespace OrcaMDF.Core.Tests.Integration
{
	public class ParseUserTableNames : SqlServerSystemTest
	{
		[Test]
		public void ParseTableNames()
		{
			using(var mdf = new MdfFile(MdfPath))
			{
				var metaData = mdf.GetMetaData();

				Assert.AreEqual(2, metaData.UserTableNames.Length);
				Assert.AreEqual("MyTable", metaData.UserTableNames[0]);
				Assert.AreEqual("XYZ", metaData.UserTableNames[1]);
			}
		}

		protected override void RunSetupQueries(SqlConnection conn)
		{
			var cmd = new SqlCommand(@"
				CREATE TABLE MyTable (ID int);
				CREATE TABLE XYZ (ID int);", conn);
			cmd.ExecuteNonQuery();
		}
	}
}
```

This allows for extremely quick testing of actual real life scenarios. Want to test the parsing of forwarded records? Simply create a new test, write the T-SQL code to generate the desired database state and then validate the scanned table data.

## The downside to system testing

Unfortunately system testing is no panacea; it has its downsides. The most obvious one is performance. A unit test is usually required to run extremely fast, allowing you to basically run them in the background on each file save. Each of these system tests takes about half a second to run, being CPU bound. Fortunately, they can be run in parallel without problems. On a quad core machine that'll allow me to run 480 tests per minute. This'll allow a manageable test time for a complete test set, while still keeping a subset test very quick. Usually a code change won't impact more than handful of tests.
