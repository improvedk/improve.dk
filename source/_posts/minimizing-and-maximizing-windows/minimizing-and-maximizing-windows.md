permalink: minimizing-and-maximizing-windows
title: Minimizing and Maximizing Windows Using PInvoke
date: 2007-04-11
tags: [.NET]
---
This time I will show how to maximize and minimize windows. I will be using the [WindowFinder class](http://www.improve.dk/blog/2007/04/07/finding-specific-windows) that I introduced in the blog [Finding specific windows](http://www.improve.dk/blog/2007/04/07/finding-specific-windows).

<!-- more -->

```csharp

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Minimizing_and_maximizing_windows
{
	class Program
	{
		// The ShowWindowAsync method alters the windows show state through the nCmdShow parameter.
		// The nCmdShow parameter can have any of the SW values.
		// See http://msdn.microsoft.com/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowfunctions/showwindowasync.asp
		// for full documentation.
		[DllImport("user32.dll")]
		public static extern bool ShowWindowAsync(int hWnd, int nCmdShow);

		// An enumeration containing all the possible SW values.
		public enum SW : int
		{
			HIDE = 0,
			SHOWNORMAL = 1,
			SHOWMINIMIZED = 2,
			SHOWMAXIMIZED = 3,
			SHOWNOACTIVATE = 4,
			SHOW = 5,
			MINIMIZE = 6,
			SHOWMINNOACTIVE = 7,
			SHOWNA = 8,
			RESTORE = 9,
			SHOWDEFAULT = 10
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
			// After an Internet Explorer window has been found, randomly either maximize or minimize it.
			if (new Random().Next(0, 2) == 0)
			{
				// Maximize the window.
				ShowWindowAsync(handle, (int)SW.SHOWMAXIMIZED);

				Console.WriteLine("Window maximized");
			}
			else
			{
				// Minimize the window.
				ShowWindowAsync(handle, (int)SW.MINIMIZE);

				Console.WriteLine("Window minimized");
			}

			// Since we return true, this will be done for any and all Internet Explorer instances.
			return true;
		}
	}
}

```

And the result:

win32_9_1_2.jpg
