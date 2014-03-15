permalink: cascading-windows
title: Cascading Windows Using PInvoke
date: 2007-04-14
tags: [.NET]
---
In Photoshop we often work with multiple windows open. They can be cascaded to more easily be able to view the different windows and tell them apart. There's an API function that does the same to any windows you specify, you can even define the rectangle where they should be cascaded within.

<!-- more -->

```csharp
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;

namespace Cascading_Windows
{
	class Program
	{
		// The CascadeWindows function cascades the specified child windows of the specified parent window. It can be used to
		// cascade all windows (as in this example) or just the child windows of a specific window by passing in a handle to that
		// window. You can also define the rectangle wherein they should be cascaded.
		// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowfunctions/animatewindow.asp
		// for full documentation.
		[DllImport("user32.dll")]
		public static extern int CascadeWindows(int hWnd, int wHow, ref Rectangle lpRect, int cKids, ref ArrayList lpKids);

		static void Main(string[] args)
		{
			// As the function expects references to both a Rectangle and an ArrayList, we'll have to hack a couple of null values
			// as we can't pass null into the function.
			Rectangle nilRect = Rectangle.Empty;
			ArrayList nilList = null;

			// Cascade all windows that are children of the Desktop (handle = 0).
			CascadeWindows(0, 0, ref nilRect, 0, ref nilList);

			Console.Read();
		}
	}
}

```

And the result:

*Having three monitors generates quite a large screenshot so I'll leave this one up for you to try / your imagination :)*
