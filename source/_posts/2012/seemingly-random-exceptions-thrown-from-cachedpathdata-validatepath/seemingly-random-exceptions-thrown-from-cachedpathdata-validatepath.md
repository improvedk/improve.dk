permalink: seemingly-random-exceptions-thrown-from-cachedpathdata-validatepath
title: Seemingly Random Exceptions Thrown From CachedPathData.ValidatePath
date: 2012-01-31
tags: [.NET]
---
Several times a day I’d get an error report email noting that the following exception had occurred in our ASP.NET 4.0 application:

<blockquote>
System.Web.HttpException (0x80004005)  
at System.Web.CachedPathData.ValidatePath(String physicalPath)  
at System.Web.HttpApplication.PipelineStepManager.ValidateHelper(HttpContext context)
</blockquote>

The 80004005 code is a red herring – it’s used for lots of different errors and doesn’t really indicate what’s wrong. Besides that, there’s no message on the exception, so I was at a loss for what might’ve caused it.

We have several 100k’s of visitors each day and I only got 5-10 of these exceptions a day, so it wasn’t critical. Even so, I don’t like exceptions being thrown without reason. After much digging (and cursing at the combination of our error logging and gmail trimming the affected URL’s), I discovered the cause.

All of the URL’s had an extra %20 at the end – caused by others linking incorrectly to our site.

After a short bit of Googling, I found the new [RelaxedUrlToFileSystemMapping httpRuntime attribute](http://msdn.microsoft.com/en-us/library/system.web.configuration.httpruntimesection.relaxedurltofilesystemmapping.aspx" target="_blank) in .NET 4.0. And sure enough, setting it to false (or letting it have it’s default false value), an exception is thrown when I add %20 to the URL. Once set to true, everything works as expected.

Though I got the problem solved, I would’ve appreciated a more descriptive exception being thrown.
