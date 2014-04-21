---
permalink: getting-key-state
title: Getting Key State Using PInvoke
date: 2007-04-11
tags: [.NET]
---
Here's an example of how to retrieve the state of any keyboard key.

<!-- more -->

```csharp
using System;
using System.Runtime.InteropServices;

namespace Getting_key_state
{
	class Program
	{
		// The GetAsyncKeyState takes a virtual key code as the nVirtKey parameter. It then checks on the state of
		// this key (down/up). The return code is either zero for up or any non-zero value for pressed,
		// thus it's easiest to convert the result to a boolean and use that result.
		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int nVirtKey);

		// These are all the possible values in the VK enumeration. It covers most of the special buttons on a keyboard.
		// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/WindowsUserInterface/UserInput/VirtualKeyCodes.asp
		// for full documentation.
		public enum VK : int
		{
			NUMPAD7 = 0x67,
			NUMPAD8 = 0x68,
			NUMPAD9 = 0x69,
			MULTIPLY = 0x6A,
			ADD = 0x6B,
			SEPARATOR = 0x6C,
			SUBTRACT = 0x6D,
			DECIMAL = 0x6E,
			DIVIDE = 0x6F,
			F1 = 0x70,
			F2 = 0x71,
			F3 = 0x72,
			F4 = 0x73,
			F5 = 0x74,
			F6 = 0x75,
			F7 = 0x76,
			F8 = 0x77,
			F9 = 0x78,
			F10 = 0x79,
			F11 = 0x7A,
			F12 = 0x7B,
			NUMLOCK = 0x90,
			SCROLL = 0x91,
			LSHIFT = 0xA0,
			RSHIFT = 0xA1,
			LCONTROL = 0xA2,
			RCONTROL = 0xA3,
			LMENU = 0xA4,
			RMENU = 0xA5,
			BACK = 0x08,
			TAB = 0x09,
			RETURN = 0x0D,
			SHIFT = 0x10,
			CONTROL = 0x11,
			MENU = 0x12,
			PAUSE = 0x13,
			CAPITAL = 0x14,
			ESCAPE = 0x1B,
			SPACE = 0x20,
			END = 0x23,
			HOME = 0x24,
			LEFT = 0x25,
			UP = 0x26,
			RIGHT = 0x27,
			DOWN = 0x28,
			PRINT = 0x2A,
			SNAPSHOT = 0x2C,
			INSERT = 0x2D,
			DELETE = 0x2E,
			LWIN = 0x5B,
			RWIN = 0x5C,
			NUMPAD0 = 0x60,
			NUMPAD1 = 0x61,
			NUMPAD2 = 0x62,
			NUMPAD3 = 0x63,
			NUMPAD4 = 0x64,
			NUMPAD5 = 0x65,
			NUMPAD6 = 0x66,
			A = 0x41,
			B = 0x42,
			C = 0x43,
			D = 0x44,
			E = 0x45,
			F = 0x46,
			G = 0x47,
			H = 0x48,
			I = 0x49,
			J = 0x4A,
			K = 0x4B,
			L = 0x4C,
			M = 0x4D,
			N = 0x4E,
			O = 0x4F,
			P = 0x50,
			Q = 0x51,
			R = 0x52,
			S = 0x53,
			T = 0x54,
			U = 0x55,
			V = 0x56,
			W = 0x57,
			X = 0x58,
			Y = 0x59,
			Z = 0x5A
		}

		static void Main(string[] args)
		{
			// We'll call GetAsyncKeyState passing in the A keycode as a parameter.
			Console.WriteLine(Convert.ToBoolean(GetAsyncKeyState((int)VK.A)));

			// And then we'll pass the shift keycode as a control check.
			Console.WriteLine(Convert.ToBoolean(GetAsyncKeyState((int)VK.SHIFT)));

			Console.ReadLine();
		}
	}
}
```

And the result:

win32_10_1_2.jpg
