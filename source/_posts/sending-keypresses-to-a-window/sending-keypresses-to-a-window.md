---
permalink: sending-keypresses-to-a-window
title: Sending Keypresses to a Window Using PInvoke
date: 2007-04-06 19:00:00
tags: [.NET]
---
Now to complete the toolset required to make a great spyware / browser hijacking application, we'll make Internet Explorer navigate to the address we set.

<!-- more -->

[Like before](http://www.improve.dk/blog/2007/04/03/getting-text-from-handle), open an Internet Explorer browser and obtain a handle to the address field.

```cs
using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Sending_keypresses_to_a_window
{
	class Program
	{
		// A Win32 constant
		const int WM_SETTEXT = 0x000C;
		const int WM_KEYDOWN = 0x0100;
		const int VK_RETURN  = 0x0D;

		// An overload of the SendMessage function, this time taking in a string as the lParam.
		[DllImport("User32.dll")]
		public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, string lParam);

		// PostMessage is very similar to SendMessage. They both send a message to the given
		// handle / window, the difference being that SendMessage sends the message and waits
		// for the window to "handle" the message and return a return code. PostMessage on the
		// other hand simply posts the message and returns instantly, whether the window
		// handles the message or not, we don't care.
		[DllImport("User32.Dll")]
		public static extern Int32 PostMessage(int hWnd, int msg, int wParam, int lParam);

		static void Main(string[] args)
		{
			// First, read the handle from the console, remember this has to be in HEX format!
			int handle = int.Parse(Console.ReadLine(), NumberStyles.HexNumber);

			// Now we'll send the WM_SETTEXT message to the window, passing the text
			// through the lParam parameter.
			SendMessage(handle, WM_SETTEXT, 0, "http://www.improve.dk");
			Console.WriteLine("Text set!");

			// Now send a message telling the Edit box that the Return key has been pressed,
			// resulting in Internet Explorer navigating to the page.
			PostMessage(handle, WM_KEYDOWN, VK_RETURN, 1);
			Console.WriteLine("Return keypress sent!");

			// And we're done
			Console.Read();
		}
	}
}
```

And the result:

win32_4_1_2.jpg

win32_4_2_2.jpg
