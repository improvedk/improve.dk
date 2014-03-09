permalink: using-idispoable-to-write-indented-text
title: Using IDisposable to Write Indented Text
date: 2008-06-02
tags: [.NET]
---
I often need to output indented text in one way of the other, it could be HTML, XML, source code etc (please look beyond the actual problem domain - I'd enver write XML this way, it's just an example). Usually that involved me writing tab characters manually (or by calling a function that returned the current indentation string), cluttering the actual output. An example might look like this:

<!-- more -->

```csharp
StringBuilder sb = new StringBuilder();
sb.AppendLine("public static void Hello()");
sb.AppendLine("{");
sb.AppendLine("\tif(true)");
sb.AppendLine("\t\tConsole.WriteLine("World!");");
sb.AppendLine("}");

Console.Write(sb.ToString());
Console.Read();
```

This ought to result in the following snippet:

```csharp
public static void Hello()
{
	if(true)
		Console.WriteLine("World!");
}
```

Pretty simple code, but it's a bit hard for the eyes, especially if there's a lot of it.

By utilizing the IDisposable interface, we can create a StringBuilder-esque class that handles the indentation for us. Here's an example of how we might write the previous snippet using the IndentedStringBuilder (note that it's not really a StringBuilder since StringBuilder's a sealed class):

```csharp
using (IndentedStringBuilder sb = new IndentedStringBuilder())
{
	sb.AppendLine("public static void Hello()");
	sb.AppendLine("{");

	using (sb.IncreaseIndent())
	{
		sb.AppendLine("if(true)");

		using (sb.IncreaseIndent())
			sb.AppendLine("Console.WriteLine("World!");");
	}

	sb.AppendLine("}");

	Console.Write(sb.ToString());
	Console.Read();
}
```

Each time Dispose() is called on the instance, the indentation level is decreased. Calling IncreaseIndent() increases the indentation level, as well as returning a reference to the IndentedStringBuilder instance itself. The using construct will make sure Dispose is called on the reference each time we exit the using block - and calling Dispose does not do anything to the object other than calling the Dispose method - which'll decrease the indentation level.

Here's the IndentedStringBuilder class:

```csharp
using System;
using System.Text;

namespace Improve.Framework.Text
{
	public class IndentedStringBuilder : IDisposable
	{
		private StringBuilder sb;
		private string indentationString = "\t";
		private string completeIndentationString = "";
		private int indent = 0;

		/// <summary>
		///  Creates an IndentedStringBuilder
		/// </summary>
		public IndentedStringBuilder()
		{
			sb = new StringBuilder();
		}

		/// <summary>
		/// Appends a string
		/// </summary>
		/// <param name="value"></param>
		public void Append(string value)
		{
			sb.Append(completeIndentationString + value);
		}

		/// <summary>
		/// Appends a line
		/// </summary>
		/// <param name="value"></param>
		public void AppendLine(string value)
		{
			Append(value + Environment.NewLine);
		}

		/// <summary>
		/// The string/chars to use for indentation, t by default
		/// </summary>
		public string IndentationString
		{
			get { return indentationString; }
			set
			{
				indentationString = value;

				updateCompleteIndentationString();
			}
		}

		/// <summary>
		/// Creates the actual indentation string
		/// </summary>
		private void updateCompleteIndentationString()
		{
			completeIndentationString = "";

			for (int i = 0; i < indent; i++)
				completeIndentationString += indentationString;
		}

		/// <summary>
		/// Increases indentation, returns a reference to an IndentedStringBuilder instance which is only to be used for disposal
		/// </summary>
		/// <returns></returns>
		public IndentedStringBuilder IncreaseIndent()
		{
			indent++;

			updateCompleteIndentationString();

			return this;
		}

		/// <summary>
		/// Decreases indentation, may only be called if indentation > 1
		/// </summary>
		public void DecreaseIndent()
		{
			if (indent > 0)
			{
				indent--;

				updateCompleteIndentationString();
			}
		}

		/// <summary>
		/// Decreases indentation
		/// </summary>
		public void Dispose()
		{
			DecreaseIndent();
		}

		/// <summary>
		/// Returns the text of the internal StringBuilder
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return sb.ToString();
		}
	}
}
```
