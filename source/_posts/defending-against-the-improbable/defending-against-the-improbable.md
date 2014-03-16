permalink: defending-against-the-improbable
title: Defending Against the Improbable
date: 2009-11-05
tags: [.NET]
---
As little children we've all been taught that it's better to program defensively than relying on exceptions being thrown. However, sometimes it's preferably to just hope for the best and catch exceptions if they happen.

<!-- more -->

## Defending against the improbable

Say we have a web application that receives and ID through the query string and serves a file accordingly, usually we'd write that like:

```csharp
if(File.Exists(path))
    serveFile(path);
else
    serve404();
```

This is what I've been doing in a large image serving website I run. However, it recently struck me that on average about 99.9% of all requests map to existing files, only those few .01% of all requests actually resulted in 404 errors being thrown.

Given the above ratio, if we dropped the File.Exists() check, out of 10,000 requests only 10 of those would result in a FileNotFoundException being thrown while the rest would just be served as usual. That means in 99.9% of all requests we waste resources on performing a trivial File.Exists() request that we know will most likely come back true anyways. What's worse is that this will hit the harddrive and actually cost us an IO operation!

## A local test

Observe the following two methods. serveFileUnsafely will serve a file under the assumption that it probably exists and will rely on a FileNotFoundException being throw if it doesn't exist. serveFileSafely will ensure the file exists before actually serving it (trusting nothing happens between File.Exists() and File.ReadAllText()).

```csharp
private static void serveFileUnsafely(string path)
{
    try
    {
        File.ReadAllText(path);
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine("File does not exist!");
    }
}

private static void serveFileSafely(string path)
{
    if (File.Exists(path))
        File.ReadAllText(path);
    else

        Console.WriteLine("File does not exist!");
}
```

The following two methods will be used to measure the time taken to serve 100 requests. I have created 100 identical files named [1-100].txt, each containing just the text "Hello world!". I have then deleted a random file so there's only 99 left. Thus in this example we assume that only 99% of all requests map to existing files even though the actual app has a success rate in excess of 99.9%. Note that the two methods each hit a separate folder - Test and Test2. This is to avoid any advantage of prewarming the cache before running the second test.

```csharp
private static void testLocalUnsafely()
{
    for (int file = 1; file <= 100; file++)
        serveFileUnsafely(@"E:\Test" + file + ".txt");
}

private static void testLocalSafely()
{
    for (int file = 1; file <= 100; file++)
        serveFileSafely(@"E:\Test2" + file + ".txt");
}
```

The actual profiling goes like this. I'll be using my [CodeProfiler](http://www.improve.dk/blog/2008/04/16/profiling-code-the-easy-way) class to make the measurements, running a total of 500 iterations using a single thread - as well as running an automatic warmup iteration.

```csharp
TimeSpan safeLocalTime = CodeProfiler.ProfileAction(() => testLocalSafely(), 500, 1);
TimeSpan unsafeLocalTime = CodeProfiler.ProfileAction(() => testLocalUnsafely(), 500, 1);

Console.WriteLine("Safe: " + safeLocalTime.TotalSeconds);
Console.WriteLine("Unsafe: " + unsafeLocalTime.TotalSeconds);
```

And the results?

> Safe: 4,211061  
> Unsafe: 4,9368481  
  
> Conclusion: Unsafe is 17% slower than safe.

Interestingly the defensive method actual performs the best! It's easy to conclude that throwing the exception is somewhat more expensive than hitting the IO layer to check for the files existance.

It's no coincidence that I mention the IO layer and not the disk in the above statement. As this is run locally on a machine with 8GBs of memory all 200 files are easily cached in memory - making it a pure in-memory operation to both check for the files existance as well as reading the file contents. This can be verified as well by the CPU taking up 100% resources while the test is running.

## A remote test

Back to the real scenario. The web servers are not serving files off of local disks, they're serving the files from a backend SAN exposed as CIFS shares.

I've copied the two test file directories onto two directories on a remote share.

Two new methods have been added. They're identical to the last ones except that they access a remote share mapped to the local drive Z.

```csharp
private static void testShareUnsafely()
{
    for (int file = 1; file <= 100; file++)
        serveFileUnsafely(@"Z:\Test" + file + ".txt");
}

private static void testShareSafely()
{
    for (int file = 1; file <= 100; file++)
        serveFileSafely(@"Z:\Test2" + file + ".txt");
}
```

The testing is performed like this. Note that I'm only running 10 iterations + warmup here as it'd otherwise take far too long time.

```csharp
TimeSpan safeShareTime = CodeProfiler.ProfileAction(() => testShareSafely(), 10, 1);
TimeSpan unsafeShareTime = CodeProfiler.ProfileAction(() => testShareUnsafely(), 10, 1);

Console.WriteLine("Safe: " + safeShareTime.TotalSeconds);
Console.WriteLine("Unsafe: " + unsafeShareTime.TotalSeconds);
```

The results?

> Safe: 4,1287161  
> Unsafe: 3,1327967  
  
> Conclusion: Unsafe is 25% faster than safe.

## As usual - it depends!

As can be seen, sometimes it's best to defend against exceptions and sometimes it's better to just hope for the best and catch the exception if it occurs. In my scenario throwing an exception was... Well. The exception. Make sure you always consider the cost of avoiding exceptions before you do so blindly.
