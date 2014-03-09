permalink: set-text-by-handle
title: Set Text By Window Handle Using PInvoke
date: 2007-04-04
tags: [.NET]
---
This time we won't be reading the text from a window, we'll be setting it.

[Like last time](http://www.improve.dk/blog/2007/04/03/getting-text-from-handle), open an Internet Explorer browser and obtain a handle to the address field.

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Set_text_by_handle
{
	class Program
	{
		// A Win32 constant
		const int WM_SETTEXT = 0x000C;

		// An overload of the SendMessage function, this time taking in a string as the lParam.
		[DllImport("User32.dll")]
		public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, string lParam);

		static void Main(string[] args)
		{
			// First, read the handle from the console, remember this has to be in HEX format!
			int handle = int.Parse(Console.ReadLine(), NumberStyles.HexNumber);

			// Now we'll send the WM_SETTEXT message to the window, passing the text
			// through the lParam parameter.
			SendMessage(handle, WM_SETTEXT, 0, "http://www.improve.dk");

			// And we're done
			Console.Write("Text set!");
			Console.Read();
		}
	}
}
```

And the result:

win32_3_2_2.jpg

Note that we have not navigated to the address, we have only set it!

win32_3_1_2.jpg
