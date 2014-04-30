---
permalink: getting-window-location-and-size
title: Getting Window Location and Size Using PInvoke
date: 2007-04-09 19:00:00
tags: [.NET]
---
This time I'll show how to obtain the size and location of a window. I will be using the [WindowFinder class](http://www.improve.dk/blog/2007/04/07/finding-specific-windows) that I introduced in the blog [Finding specific windows](http://www.improve.dk/blog/2007/04/07/finding-specific-windows).

<!-- more -->

Note that the location is not in relation to it's parent windows location, it is always the absolute screen position.

```csharp
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;
using System;
using System.Text;
using System.Globalization;

namespace Getting_window_location_and_size
{
	class Program
	{
		// Win32 constants.
		const int WM_GETTEXT = 0x000D;
		const int WM_GETTEXTLENGTH = 0x000E;

		// Win32 functions that have all been used in previous blogs.
		[DllImport("User32.Dll")]
		private static extern void GetClassName(int hWnd, StringBuilder s, int nMaxCount);

		[DllImport("User32.dll")]
		private static extern Int32 SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);

		[DllImport("User32.dll")]
		private static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

		// The GetWindowRect function takes a handle to the window as the first parameter. The second parameter
		// must include a reference to a Rectangle object. This Rectangle object will then have it's values set
		// to the window rectangle properties.
		[DllImport("user32.dll")]
		public static extern long GetWindowRect(int hWnd, ref Rectangle lpRect);

		static void Main(string[] args)
		{
			// Introduced in the "Finding specific windows" blog, we use the WindowFinder class to find all Internet Explorer main window instances.
			Finding_specific_windows.WindowFinder wf = new Finding_specific_windows.WindowFinder();
			wf.FindWindows(0, null, new Regex("- (Windows|Microsoft) Internet Explorer"), new Regex("iexplore"), new Finding_specific_windows.WindowFinder.FoundWindowCallback(foundWindow));

			Console.Read();
		}

		static bool foundWindow(int handle)
		{
			// First we intialize an empty Rectangle object.
			Rectangle rect = new Rectangle();

			// Then we call the GetWindowRect function, passing in a reference to the rect object.
			GetWindowRect(handle, ref rect);

			// And then we get the resulting rectangle. The tricky part here is that this rectangle includes
			// not only the location of the window, but also the size, but not in the form we're used to.
			Console.WriteLine(rect.ToString());

			// If the window is 100 x 100 pixels and is located at (10,10), then the rectangle would look like this:
			// rect.X = 10;
			// rect.Y = 10;
			// rect.Width = 110;
			// rect.Height = 110;
			// We simply have to subtract the rect.X value from the rect.Width value to obtain the "real" width of
			// the window, similarly we have to subtract the Y value from the Height value to obtain the real height.
			// After this we have the real window properties through the X, Y, Width and Height values.
			rect.Width = rect.Width - rect.X;
			rect.Height = rect.Height - rect.Y;

			// Lets print the rectangle after we've fixed it so we can confirm it's correct.
			Console.WriteLine(rect.ToString());

			// As used earlier, we print the basic properties of the window.
			printWindowInfo(handle);

			return true;
		}

		// Prints basic properties of a window, uses function already used in previous blogs.
		private static void printWindowInfo(int handle)
		{
			// Get the class.
			StringBuilder sbClass = new StringBuilder(256);
			GetClassName(handle, sbClass, sbClass.Capacity);

			// Get the text.
			int txtLength = SendMessage(handle, WM_GETTEXTLENGTH, 0, 0);
			StringBuilder sbText = new StringBuilder(txtLength + 1);
			SendMessage(handle, WM_GETTEXT, sbText.Capacity, sbText);

			// Now we can write out the information we have on the window.
			Console.WriteLine("Handle: " + handle);
			Console.WriteLine("Class : " + sbClass);
			Console.WriteLine("Text  : " + sbText);
			Console.WriteLine();
		}
	}
}
```

And the result:

win32_7_1_2.jpg

win32_7_2_2.jpg
