permalink: activating-windows
title: Activating Windows Using PInvoke
date: 2007-04-12
tags: [.NET]
---
Now we'll see how to activate windows and sending them to the foreground. I will be using the [WindowFinder class](http://www.improve.dk/blog/2007/04/07/finding-specific-windows) that I introduced in the blog [Finding specific windows](http://www.improve.dk/blog/2007/04/07/finding-specific-windows).

```csharp

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Activating_windows
{
	class Program
	{
		// This enumeration holds all the possible values that can be passed onto the ShowWindow function.
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

		// The SetForegroundWindow will activate the window, setting the window thread to the foreground thread, as
		// well as activating keyboard input for the specified window.
		[DllImport("user32.dll")]
		public static extern long SetForegroundWindow(int hWnd);

		// The ShowWindow function can do the same as SetForegroundWindow, but it gives much greater control
		// over what happens, by customizing the parameters sent through the cmd parameter.
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(int hWnd, int cmd);

		static void Main(string[] args)
		{
			// Introduced in the "Finding specific windows" blog, we use the WindowFinder class to find all Internet Explorer main window instances.
			Finding_specific_windows.WindowFinder wf = new Finding_specific_windows.WindowFinder();
			wf.FindWindows(0, null, new Regex("- (Windows|Microsoft) Internet Explorer"), new Regex("iexplore"), new Finding_specific_windows.WindowFinder.FoundWindowCallback(foundWindow));

			Console.Read();
		}

		static bool foundWindow(int handle)
		{
			// We'll activate the window by calling the SetForegroundWindow function, passing in the handle to the window.
			SetForegroundWindow(handle);

			// Calling the ShowWindow function with the SHOWNA parameter will put the window in the foreground,
			// but it won't be activated.
			ShowWindow(handle, (int)SW.SHOWNA);

			Console.WriteLine("Window activated.");

			return false;
		}
	}
}

```

And the result:

win32_11_1_2.jpg
