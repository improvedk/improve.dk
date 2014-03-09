permalink: combining-paths-with-multiple-parts
title: Combining Paths With Multiple Parts
date: 2009-09-16
tags: [.NET]
---
Whenever you concatenate multiple strings into a path, you really ought to be using the [System.IO.Path](http://msdn.microsoft.com/en-us/library/system.io.path.aspx) class's Combine method. At times you may be concatenating a number of smaller parts of a path instead of just the two that the [Path.Combine()](http://msdn.microsoft.com/en-us/library/fyy7a5kt.aspx) method takes. Nested Path.Combine calls quickly become difficult to read and error prone:

```csharp
string partOne = @"C:";
string partTwo = "Windows";
string partThree = @"System32\drivers";
string partFour = @"etc\hosts";
string combinedPath;

combinedPath = Path.Combine(Path.Combine(Path.Combine(partOne, partTwo), partThree), partFour);
```

Often we won't have all of our path parts in named variables, and even when we do, they'll rarely be named partOne, partTwo, partX etc. If we mix literal strings with variables and multiple levels of nested Path.Combine calls, mayhem will arise.

As an alternative I'm using a simple wrapper method above the Path.Combine method:

```csharp
public static class PathCombiner
{
	public static string Combine(string path1, string path2, params string[] pathn)
	{
		string path = Path.Combine(path1, path2);

		for (int i = 0; i < pathn.Length; i++)
			path = Path.Combine(path, pathn[i]);

		return path;
	}
}
```

The C# [params](http://msdn.microsoft.com/en-us/library/w5zay9db(VS.71).aspx) keyword allows us to make a method take in any number of parameters of the same type - string in this case. Note that I've split the paths up into three parts - path1, path2 and pathn. If we were to only take the params string[] parameter, the user might send in no parameters at all - which wouldn't make sense. By forcing the user to send in at least two paths, we maintain the interface of Path.Combine and just add extra functionality on top of it - though the user may still just send in two paths as before.

```csharp
static void Main(string[] args)
{
	string partOne = @"C:";
	string partTwo = "Windows";
	string partThree = @"System32\drivers";
	string partFour = @"etc\hosts";
	string combinedPath;

	// Using System.IO.Path
	combinedPath = Path.Combine(Path.Combine(Path.Combine(partOne, partTwo), partThree), partFour);
	Console.WriteLine(combinedPath);
	
	// Using PathCombiner
	combinedPath = PathCombiner.Combine(partOne, partTwo, partThree, partFour);
	Console.WriteLine(combinedPath);

	Console.Read();
}
```

An extension method you say? The logical place to put this function would be in the Path class itself, perhaps named CombineMultiple. Unfortunately the Path class is static so we're unable to extend it. Another option might be directly on string as a CombinePath method like this:

```csharp
public static class PathCombiner
{
	public static string Combine(string path1, string path2, params string[] pathn)
	{
		string path = Path.Combine(path1, path2);

		for (int i = 0; i < pathn.Length; i++)
			path = Path.Combine(path, pathn[i]);

		return path;
	}

	public static string CombinePath(this string path1, string path2, params string[] pathn)
	{
		return Combine(path1, path2, pathn);
	}
}
```

We'd call the extension method like so:

```csharp
combinedPath = partOne.CombinePath(partTwo).CombinePath(partThree).CombinePath(partFour);
```

While this does work, I really don't recommend it. I'm against overly use of extension methods unless there's a good reason. I think it's much cleaner to contain this code in a separate class whose only purpose is path combining. Now devs are going to be confused when they sometimes see the CombinePath method in Intellisense and not at other times, depending on whether the namespace has been imported. Also, I think the PathCombiner.Combine syntax is the cleanest on top of that, but you be the judge:

```csharp
string partOne = @"C:";
string partTwo = "Windows";
string partThree = @"System32\drivers";
string partFour = @"etc\hosts";
string combinedPath;

// Using System.IO.Path
combinedPath = Path.Combine(Path.Combine(Path.Combine(partOne, partTwo), partThree), partFour);
Console.WriteLine(combinedPath);

// Using PathCombiner
combinedPath = PathCombiner.Combine(partOne, partTwo, partThree, partFour);
Console.WriteLine(combinedPath);

combinedPath = partOne.CombinePath(partTwo).CombinePath(partThree).CombinePath(partFour);
Console.WriteLine(combinedPath);

Console.Read();
```

```csharp
C:\Windows\System32\drivers\etc\hosts
C:\Windows\System32\drivers\etc\hosts
C:\Windows\System32\drivers\etc\hosts
```
