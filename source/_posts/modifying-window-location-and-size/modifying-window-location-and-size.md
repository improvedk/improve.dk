---
permalink: modifying-window-location-and-size
title: Modifying Window Location and Size Using PInvoke
date: 2007-04-09 20:00:00
tags: [.NET]
---
Last time we saw how to obtain a windows location and size. This time I'll show how to change a windows size and location. I will be using the [WindowFinder class](http://www.improve.dk/blog/2007/04/07/finding-specific-windows) that I introduced in the blog [Finding specific windows](http://www.improve.dk/blog/2007/04/07/finding-specific-windows).

<!-- more -->

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Modifying_window_location_and_size
{
	class Program
	{
		// The SetWindowPos function is used to both resize and change the location of windows. The uFlags parameter
		// can take any number of flags, with zero being a neutral flag, the same goes for the hWndInsertAfter parameter.
		// X, Y is the new location of the window, cx and cy is the new height / width of the window. Via uFlags it can
		// be set to ignore the new location and/or the new size of the window.
		// See http://msdn2.microsoft.com/en-us/library/ms633545.aspx for full documentation.
		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		// An enumeration containing all the possible HWND values.
		public enum HWND : int
		{
			BOTTOM = 1,
			NOTOPMOST = -2,
			TOPMOST = -1,
			TOP = 0
		}

		// And enumeration containing all the possible SWP values.
		public enum SWP : uint
		{
			ASYNCWINDOWPOS = 0x4000,
			DEFERERASE = 0x2000,
			FRAMECHANGED = 0x0020,
			HIDEWINDOW = 0x0080,
			NOACTIVATE = 0x0010,
			NOCOPYBITS = 0x0100,
			NOMOVE = 0x0002,
			NOOWNERZORDER = 0x0200,
			NOREDRAW = 0x0008,
			NOSENDCHANGING = 0x0400,
			NOSIZE = 0x0001,
			NOZORDER = 0x0004,
			SHOWWINDOW = 0x0040
		}

		static void Main(string[] args)
		{
			// Introduced in the "Finding specific windows" blog, we use the WindowFinder class to find all Internet Explorer main window instances.
			Finding_specific_windows.WindowFinder wf = new Finding_specific_windows.WindowFinder();
			wf.FindWindows(0, null, new Regex("- (Windows|Microsoft) Internet Explorer"), new Regex("iexplore"), new Finding_specific_windows.WindowFinder.FoundWindowCallback(foundWindow));

			Console.Read();
		}

		static bool foundWindow(int handle)
		{
			// After an Internet Explorer window has been found, relocate it to (50,50) and set it's size to 600x500px.
			SetWindowPos(handle, 0, 50, 50, 600, 500, 0);

			Console.WriteLine("Window resized and relocated");

			// Since we return true, this will be done for any and all Internet Explorer instances.
			return true;
		}
	}
}
```

And the result:

win32_8_1_2.jpg

win32_8_2_2.jpg
