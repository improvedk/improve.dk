permalink: automated-testing-of-orcamdf-against-multiple-sql-server-versions
title: Automated Testing of OrcaMDF Against Multiple SQL Server Versions
date: 2011-11-28
tags: [.NET, SQL Server - OrcaMDF]
---
Since I released [OrcaMDF Studio](http://improve.dk/archive/2011/11/25/orcamdf-studio-release-feature-recap.aspx" target="_blank), I’ve gotten aware of some base table differences between SQL Server 2008 and 2005. These differences causes OrcaMDF to fail since it’s coded against 2008 R2 and expect that format.

While working on support for SQL Server 2005, it dawned on me that I need to expand my [testing](http://improve.dk/archive/2011/06/14/avoiding-regressions-in-orcamdf-by-system-testing.aspx" target="_blank) to cover multiple SQL Server versions, instead of just hitting a single one. Somehow I also need to support the fact that some tests are only applicable for certain versions (e.g. sparse column tests should only be run for SQL Server 2008+, etc.).

## NUnit TestCaseSourceAttribute to the rescue!

NUnit supports [inline parameterized tests](http://www.nunit.org/index.php?p=testCase&amp;r=2.5" target="_blank) through the TestCase attribute. Even better, we can also provide the parameter data for test cases dynamically, using the [TestCaseSource](http://www.nunit.org/index.php?p=testCaseSource&amp;r=2.5" target="_blank) attribute.

First up, I implemented a simple enumeration covering the versions that I’m currently working on supporting:

```csharp
public enum DatabaseVersion
{
	SqlServer2005,
	SqlServer2008,
	SqlServer2008R2,
}
```

I then created the SqlServerTestAttribute class, inheriting directly from TestCaseSourceAttribute like so:

```csharp
public class SqlServerTestAttribute : TestCaseSourceAttribute
{
	private static IEnumerable<TestCaseData> versions
	{
		get
		{
			foreach (var value in Enum.GetValues(typeof(DatabaseVersion)))
				yield return new TestCaseData(value).SetCategory(value.ToString());
		}
	}

	public SqlServerTestAttribute() : base(typeof(SqlServerTestAttribute), "versions")
	{ }
}
```

The SqlServerTestAttribute class tells TestCaseSourceAttribute to find the test case source data in the private static *versions* property. The versions property enumerates all the DatabaseVersion values and returns them one by one – ensuring to set the test category to the name of the DatabaseVersion value.

Next up, I converted my current tests to use the new SqlServerTest attribute, instead of the previous vanilla NUnit Test attribute:

```csharp
[SqlServerTest]
public void HeapForwardedRecord(DatabaseVersion version)
{
	...
}
```

This causes all of my tests to be run once per enumeration value in the DatabaseVersion enumeration, automatically getting each of the values as input values in the version parameter.

## Supporting different development environments

Now, I don’t want to force everyone to install all versions of SQL Server – they might just want to support SQL Server 2005 &amp; 2008R2 for example. In the OrcaMDF.Core.Tests project, I’ve defined a connection string for each supported test database like so:

<pre lang="xml" escaped="true">&lt;connectionStrings&gt;
	&lt;clear/&gt;
	&lt;add name="SqlServer2005" connectionString="Data Source=.SQL2005;Integrated Security=SSPI"/&gt;
	&lt;add name="SqlServer2008R2" connectionString="Data Source=.;Integrated Security=SSPI"/&gt;
&lt;/connectionStrings&gt;</pre>

If a database doesn’t have a connection (the name corresponding to the DatabaseVersion enumeration value), the test won’t be run for that version, simple as that. In this case I’m currently ignoring SQL Server 2008 as I only have 2005 and 2008R2 installed on my machine.

To perform the filtering on available databases, I’ve modified my test cases to let the base class actually run the test, using a lambda:

```csharp
[SqlServerTest]
public void HeapForwardedRecord(DatabaseVersion version)
{
	RunDatabaseTest(version, db =>
	{
		var scanner = new DataScanner(db);
		var rows = scanner.ScanTable("HeapForwardedRecord").ToList();

		Assert.AreEqual(25, rows[0].Field<int>("A"));
		Assert.AreEqual("".PadLeft(5000, 'A'), rows[0].Field<string>("B"));

		Assert.AreEqual(28, rows[1].Field<int>("A"));
		Assert.AreEqual("".PadLeft(4000, 'B'), rows[1].Field<string>("B"));
	});
}
```

The RunDatabase method is exposed in the SqlServerSystemTestBase class:

```csharp
protected void RunDatabaseTest(DatabaseVersion version, Action<Database> test)
{
	string versionConnectionName = version.ToString();

	// Only run test for this version if a connection string has been provided
	if (ConfigurationManager.ConnectionStrings[versionConnectionName] == null)
		Assert.Inconclusive();

	// Setup database and store file paths, if we haven't done so already
	ensureDatabaseIsSetup(version);

	// Run actual test
	using (var db = new Database(databaseFiles[version]))
		test(db);
}
```

If a corresponding connection string hasn’t been declared in the configuration file, we abort the test and mark it as inconclusive – we simply weren’t able to run it given the current setup. Next up, ensureDatabaseIsSetup() runs the usual setup code (as detailed in the [earlier blog post](http://improve.dk/archive/2011/06/14/avoiding-regressions-in-orcamdf-by-system-testing.aspx" target="_blank)), though this time once per database versions, per fixture. Finally an OrcaMDF instance is created and passed onto the actual test as a parameter.

## Supporting different SQL Server feature versions

As mentioned, I need a way of executing some tests only on certain versions of SQL Server. The standard SqlServerTestAttribute automatically enumerations *all* values of the DatabaseVersion enumeration, but there’s no reason we can’t create a SqlServer2005TestAttribute like this:

```csharp
public class SqlServer2005TestAttribute : TestCaseSourceAttribute
{
	private static IEnumerable<TestCaseData> versions
	{
		get
		{
			yield return new TestCaseData(DatabaseVersion.SqlServer2005).SetCategory(DatabaseVersion.SqlServer2005.ToString());
		}
	}

	public SqlServer2005TestAttribute() : base(typeof(SqlServer2005TestAttribute), "versions")
	{ }
}
```

Or what about tests that need to be run on SQL Server 2008+?

```csharp
public class SqlServer2008PlusTestAttribute : TestCaseSourceAttribute
{
	private static IEnumerable<TestCaseData> versions
	{
		get
		{
			foreach (var value in Enum.GetValues(typeof(DatabaseVersion)))
				if((DatabaseVersion)value >= DatabaseVersion.SqlServer2008)
					yield return new TestCaseData(value).SetCategory(value.ToString());
		}
	}

	public SqlServer2008PlusTestAttribute()
		: base(typeof(SqlServer2008PlusTestAttribute), "versions")
	{ }
}
```

Once we have the attributes, it’s as easy as marking the individual tests with the versions they’re supposed to be run on:

```csharp
[SqlServer2008PlusTest]
public void ScanAllNullSparse(DatabaseVersion version)
{
	RunDatabaseTest(version, db =>
	{
		var scanner = new DataScanner(db);
		var rows = scanner.ScanTable("ScanAllNullSparse").ToList();

		Assert.AreEqual(null, rows[0].Field<int?>("A"));
		Assert.AreEqual(null, rows[0].Field<int?>("B"));
	});
}
```

## ReSharper test runner support

For the tests to run, you’ll need ReSharper 6.0 as ReSharper 5.1 doesn’t support the TestCaseSource attribute. Once you do, you’ll see a result like this once run (having enabled SQL Server 2005 &amp; 2008 R2 testing in this case):

image_24.png

Each test case is automatically multiplied by each DatabaseVersion (the Parse test isn’t, since it doesn’t implement SqlServerSystemTestBase and thus isn’t run on multiple versions). Most of the tests are failing on SQL Server 2005 since I don’t support it yet. All 2008 tests are inconclusive as I’m not running the tests. And finally, all of the 2008R2 tests are green, yay!

## Filtering the tests

Obviously, we don’t want to run the tests for all versions of SQL Server all the time, that’d simply be too time consuming. One way to disable the testing of a specific version would be to remove the connection string. However, that still yields an inconclusive output, and it’s somewhat cumbersome to edit the configuration file all the time.

Unfortunately, the ReSharper test runner doesn’t support category filtering of parameterized tests created using the TestCaseSourceAttribute. I’ve created a [feature request case on YouTRACK](http://youtrack.jetbrains.net/issue/RSRP-283463" target="_blank) as I really hope they’ll consider adding it for 6.1. If you also think it’d be awesome, please consider voting for the request case!

Fortunately, the NUnit test runner *does* support this kind of filtering. Opening the OrcaMDF.Core.Tests assembly in the NUnit test runner gives the following result:

image_44.png

Notice how it already knows about the parameterized test parameters, even before we’ve run the test! Also note how it recognizes that the DifferingRecordFormats test is only to be run on SQL Server 2008+ while the FGSpecificClusteredAllocation test is to be run on 2005+.

What’s even better – if we go to the Categories tab, we get a list of all the test categories:

image_64.png

By explicitly selecting certain categories, we can choose to run just those versions. Once run, the other versions will be clearly greyed out:

image_81.png

Note the horrible runtime of 89 secs – just over 1 second per test. 98% of that time is spent in the LobTypes feature testing. Thanks to the category format, I can also apply categories to the main tests themselves, and thus easily filter out the long running tests and just concentrate on the quick ones. Lob types are especially demanding to test since they involve a lot of disk activity, creating all of the setup tables &amp; rows before hitting the database.

## Going forward

Adding new versions is as simple as installing that version of SQL Server, adding a connection string in the configuration settings, and finally, adding the SQL Server name to the DatabaseVersion enumeration. That’s all there is to it.

On the more advanced side, at some point, I will need to test the many permutations of upgrade paths. Based on some testing I did, a SQL Server 2005 database upgraded to 2008R2 isn’t necessarily identical to a native 2008R2 one, or to a 2008-2008R2 upgraded one. As such, I’d need to test the many distinct upgrade paths to ensure full compatibility. However, that’s not high on my priority list, and it’d take even more time to test.
