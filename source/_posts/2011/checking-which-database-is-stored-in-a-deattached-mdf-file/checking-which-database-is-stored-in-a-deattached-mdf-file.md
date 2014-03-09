permalink: checking-which-database-is-stored-in-a-deattached-mdf-file
title: Checking Which Database is Stored in a Detached MDF File
date: 2011-05-19
tags: [.NET, SQL Server - OrcaMDF]
---
Inspired by [this](http://stackoverflow.com/questions/6061510/any-way-to-quickly-tell-which-database-if-any-is-attached-to-a-mdf-file" target="_blank) question on StackOverflow, I’ve made a quick script to demonstrate how this might be done using [OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank).

In this example I’m looping all .mdf files in my local SQL Server data directory. Each one is loaded using OrcaMDF, the boot page is fetched and finally the database name is printed:

```csharp
using System;
using System.IO;
using OrcaMDF.Core.Engine;

namespace OrcaMDF.Adhoc
{
    class Program
    {
        static void Main()
        {
			foreach (string mdfPath in Directory.GetFiles(@"C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA"))
			{
				if (!mdfPath.ToLower().EndsWith(".mdf"))
					continue;

				using (var file = new MdfFile(mdfPath))
				{
					var bootPage = file.GetBootPage();
					Console.WriteLine(bootPage.DatabaseName);
				}
			}
        }
    }
}
```

And the following is the output we get:

image_21.png

Which, coincidentally, matches up to the databases I’ve got attached to my local SQL Server. At this point we could match this list up to the one we’d get from sys.databases and see which files didn’t have a matching database, and thus weed out the non-attached mdf files from our data directory.
