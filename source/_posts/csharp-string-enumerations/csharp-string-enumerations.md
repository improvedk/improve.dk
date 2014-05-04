---
permalink: csharp-string-enumerations
title: C# String Enumerations
date: 2008-03-13
tags: [.NET]
---
Switches are rarely nice in an architectural aspect, but they are often required none the less. One of the ways we can reduce the risk of errors as well as increase readability is to use enumeration values instead of constants. Unfortunately this only works for numeric types, we cannot create a string enumeration. Here's a workaround.  This is a typical console application, taking in an input value (stored in the input variable) and switching on the content:

<!-- more -->

```cs
using System;

namespace StringEnumeration
{
	class Program
	{
		static void Main(string[] args)
		{
			string input = "Hello";

			switch (input)
			{
				case "Hello":
					Console.WriteLine("Hello world!");
					break;
				case "Goodbye":
					Console.WriteLine("Goodbye world!");
					break;
				default:
					Console.WriteLine("Does not compute!");
					break;
			}
		}
	}
}
```

The first step is to define the enumeration of values we need to have in our switch statement:

```cs
enum Input
{
	Hello,
	Goodbye
}
```

We cannot convert from strings to the Input enumeration type directly, so we'll have to use a magic function like this:

```cs
class EnumHelper
{
	public static T Parse<T>(string input)
	{
		return (T)Enum.Parse(typeof(T), input, true);
	}
}
```

Using the above function, we can refactor our initial code like so:

```cs
string input = "Hello";

switch (EnumHelper.Parse<Input>(input))
{
	case Input.Hello:
		Console.WriteLine("Hello world!");
		break;
	case Input.Goodbye:
		Console.WriteLine("Goodbye world!");
		break;
	default:
		Console.WriteLine("Does not compute!");
		break;
}
```

Take notice that I'm passing in true as the third parameter of the Enum.Parse method, this means the type conversion will not be case sensitive, you can change this parameter as needed, or maybe refactor it into a parameter of the function. If the conversion fails - if a matching enumeration does not exist - an ArgumentException is thrown.
