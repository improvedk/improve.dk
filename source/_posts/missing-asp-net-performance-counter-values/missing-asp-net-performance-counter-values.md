---
permalink: missing-asp-net-performance-counter-values
title: Missing ASP.NET Performance Counters
date: 2008-04-01
tags: [Windows]
---
Before attempting to optimize code or fix any kind of load issue, you should first gather data and become aware of what bottlenecks you're experiencing. A great way to do this is through the Performance Monitor application. Recently I tried monitoring my ASP.NET applications, but all my counters had a value of 0. As I thought initially, it's a simple problem, but the solution was not easily found.

<!-- more -->

In [some](http://www.velocityreviews.com/forums/t101885-aspnet-performance-counters-not-updating.html) [cases](http://www.velocityreviews.com/forums/t70137-re-aspnet-performance-counters-are-all-zero-.html) it might be due to lack of permissions on the performance counter registry keys.

In my case it's because I was running Server 2003 x64, but my IIS was running in 32 bit mode (due to a couple of reasons, mainly lack of x64 support in some 3rd party components). When you run the IIS worker processes in 32 bit mode, the performance counters that are used are part of the SysWow64 scheme. The problem with this is that the usual Performance Monitor application will not read these 32 bit performance counters, and as a result you will see them all with a value of 0.

The fix is simple... Simply open up C:\Windows\SysWOW64\perfmon.exe instead of the usual Performance Monitor link in the Administrative Tools directory. This version of perfmon is the good old 32 bit version that will read the 32 bit ASP.NET performance counters. This trick applies for all x64 versions of Windows.
