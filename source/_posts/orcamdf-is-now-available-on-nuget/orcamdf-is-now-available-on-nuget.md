permalink: orcamdf-is-now-available-on-nuget
title: OrcaMDF Is Now Available on NuGet
date: 2013-05-13
tags: [.NET, SQL Server - OrcaMDF]
---
Thanks to Justin Dearing ([b](http://www.justaprogrammer.net/)|[t](https://twitter.com/zippy1981)), OrcaMDF is now available on [NuGet](https://www.nuget.org/packages/OrcaMDF.Core)!

<!-- more -->

OrcaMDF being on NuGet means the bar just got lowered even more if you want to try it out. Let me show you how easy it is to read the Adventureworks 2008 R2 Database using OrcaMDF:

To begin, let's create a vanilla .NET *Console Application*:

1.png

Once the solution has been made, right click *References* and go to *Manage NuGet Packages*:

2.png

Once the dialog opens, simply search for *OrcaMDF* and click the *Install* button for the OrcaMDF.Core package:

3.png

When done, you should now see a small green checkmark next to the OrcaMDF.Core package:

4.png

At this point the OrcaMDF.Core assembly will be available and all you have to do is start using it. For example you could print out all of the products along with their prices by modifying the Program.cs file like so (you'll have to alter the path to AdventureWorks2008R2_Data.mdf file so it points to a local copy (which must not be in use by SQL Server) on your machine):

```csharp
using System;
using OrcaMDF.Core.Engine;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main()
		{
			using (var db = new Database(@"C:\AdventureWorks2008R2_Data.mdf"))
			{
				var scanner = new DataScanner(db);

				foreach (var row in scanner.ScanTable("Product"))
				{
					Console.WriteLine(row.Field<string>("Name"));
					Console.WriteLine("Price: " + row.Field<double>("ListPrice"));
					Console.WriteLine();
				}
			}
		}
	}
}
```

And then just running the solution:

5.png

And there you have it, in just a few quick short steps you've now fetched OrcaMDF and read the Products table, from the standard AdventureWorks 2008 R2 database, without even touching SQL Server.

With OrcaMDF now being available on NuGet as well as with [a simple GUI](/orcamdf-studio-release-feature-recap/), it really doesn't get any simpler to take it for a spin :)
