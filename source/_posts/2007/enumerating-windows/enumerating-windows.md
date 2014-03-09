permalink: enumerating-windows
title: Enumerating Windows Using PInvoke
date: 2007-04-06
tags: [.NET]
---
Until now we've seen how to retrieve basic properties of windows as well as how to interact with them by sending keypresses. Up until now we've had to find the handle by using Winspector or a similar program. This time I'll present a way of finding the handles programmatically.

```csharp
using System.Runtime.InteropServices;
using System.Text;
using System;

namespace Enumerating_windows
{
	class Program
	{
		// These are two Win32 constants that we'll need, they were explained in an earlier blog.
		const int WM_GETTEXT		= 0x000D;
		const int WM_GETTEXTLENGTH	= 0x000E;

		// SendMessage overload.
		[DllImport("User32.dll")]
		public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

		// SendMessage overload.
		[DllImport("User32.dll")]
		public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);

		// The GetClassName function takes a handle as a parameter as well as a StringBuilder
		// and the max capacity of the StringBuilder as parameters. It'll return the windows
		// class name by filling up the StringBuilder - though not any longer than the max capacity.
		// If the class is longer than the max capacity it will simply be cropped. Having a larger
		// capacity than necessary is simply a matter of performance.
		[DllImport("User32.Dll")]
		public static extern void GetClassName(int hWnd, StringBuilder s, int nMaxCount);

		// The EnumWindows function will enumerate all windows in the system. Each window will cause
		// the PCallBack callback function to be called.
		[DllImport("user32.Dll")]
		static extern bool EnumWindows(PCallBack callback, int lParam);

		// This is the delegate that sets the signature for the callback function of the EnumWindows function.
		private delegate bool PCallBack(int hwnd, int lParam);

		static void Main(string[] args)
		{
			// All we'll do is to invoke the EnumWindows function, passing in a new delegate specifying the EnumWindowsCallback
			// function as the callback function. The lParam parameter can be used to send in an integer that will be passed
			// onto the callback function unmodified. It's optional, but it may be useful in some situations.
			EnumWindows(new PCallBack(EnumWindowsCallback), 0);

			Console.Read();
		}

		// This function will be called exactly once for each window the EnumWindows function finds. We have no idea what
		// window / type of windows it'll get called for, so we'll have to identify the windows somehow. The lParam
		// parameter contains the value we passed when we called the EnumWindows function.
		private static bool EnumWindowsCallback(int handle, int lParam)
		{
			// First we'll find the class of the window as that is usually the parameter that narrows our search down the furthest.
			// As classes are usually rather short, a capacity of 256 ought to be plenty.
			StringBuilder sbClass = new StringBuilder(256);
			GetClassName(handle, sbClass, sbClass.Capacity);

			// As explained in an earlier blog we then get the text of the window.
			int txtLength = SendMessage(handle, WM_GETTEXTLENGTH, 0, 0);
			StringBuilder sbText = new StringBuilder(txtLength + 1);
			SendMessage(handle, WM_GETTEXT, sbText.Capacity, sbText);

			// Now we can write out the information we have on the window:
			Console.WriteLine("Handle: " + handle);
			Console.WriteLine("Class : " + sbClass);
			Console.WriteLine("Text  : " + sbText);
			Console.WriteLine();

			// When we return true, the EnumWindows function will proceed to call the callback function with the next window.
			// If we returned false, the EnumWindows function would stop and the callback function wouldn't get called again.
			// This can be useful if we're looking for a specific window - once it's found we can just return false, no reason
			// to handle any other windows.
			return true;
		}
	}
}
```

And the result:

win32_5_1_2.jpg
