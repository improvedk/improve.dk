---
permalink: getting-a-process-from-a-handle
title: Getting a Process From a Handle
date: 2007-04-03
tags: [.NET]
---
So we have a handle, what process does it belong to? Our goal is to obtain a .NET System.Diagnostics.Process object that corresponds to the owner process of the handle we input.

<!-- more -->

Let's first open up an Internet Explorer window, just leave it at the start page, whatever yours is.

win32_1_1_2.jpg

Now fire up Winspector and locate the Internet Explorer window, you'll see the handle in HEX format in the treeview.

win32_1_2_2.jpg

```cs
using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;

namespace Get_process_from_handle
{
	class Program
	{
		// The DllImport attribute specifies the Win32 DLL that contains the function we're importing,
		// in this case it's the user32.dll file that resides in the C:WindowsSystem32 directory.
		// The function we're importing is GetWindowThreadProcessId, it takes a handle and a reference
		// to an outgoing integer that'll return the process ID of the handle.
		[DllImport("user32")]
		public static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

		static void Main(string[] args)
		{
			// First, read the handle from the console, remember this has to be in HEX format!
			int handle = int.Parse(Console.ReadLine(), NumberStyles.HexNumber);

			// Now that we have the handle, create an uninitialized integer that'll hold the process ID
			// of the handle process.
			int processID;
			GetWindowThreadProcessId(handle, out processID);

			// Now that we have the process ID, we can use the built in .NET function to obtain a process object.
			Process p = Process.GetProcessById(processID);

			// Finally we'll write out the process name to confirm success.
			Console.Write(p.ProcessName);
			Console.Read();
		}
	}
}
```

And the result:

win32_1_3_2.jpg
