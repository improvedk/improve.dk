---
permalink: animating-windows
title: Animating Windows Using PInvoke
date: 2007-04-13
tags: [.NET]
---
Let's be a bit more graphic. This time I'll show you how to use the Windows API to make your forms fade in/out, slide in from the side or do various other animations. For this example we'll have to use a Windows Forms project as we have to utilize a Form object in the example.

<!-- more -->

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Animating_windows
{
	public partial class Form1 : Form
	{
		// The possible AW flags for use with the AnimateWindow function.
		public enum AW : int
		{
			SLIDE = 262144,
			ACTIVATE = 131072,
			BLEND = 524288,
			HIDE = 65536,
			CENTER = 16,
			HOR_POSITIVE = 1,
			HOR_NEGATIVE = 2,
			VER_POSITIVE = 4,
			VER_NEGATIVE = 8
		}

		// The AnimateWindow function enables you to produce special effects when showing or hiding windows. The hWnd parameter
		// is the handle to the window - note that this window HAS to be in the same thread as the thread calling the AnimateWindow
		// function - thus the windows project so we have a Form to experiment with. The time flag is the duration of the
		// animation, and finally the flags parameter sets the type of animation to perform.
		// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowfunctions/animatewindow.asp
		// for full documentation.
		[DllImport("user32.dll")]
		public static extern bool AnimateWindow(IntPtr hWnd, int time, int flags);

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Fade in the form over a period of 3 seconds.
			AnimateWindow(this.Handle, 3000, (int)AW.BLEND);

			// Hide the form so we can perform the next animation.
			this.Hide();

			// Make the window expand outward.
			AnimateWindow(this.Handle, 3000, (int)AW.CENTER);

			// And collapse inward...
			AnimateWindow(this.Handle, 3000, (int)AW.CENTER | (int)AW.HIDE);

			// Let's slide in the form from the left side to the right.
			AnimateWindow(this.Handle, 3000, (int)AW.SLIDE | (int)AW.HOR_POSITIVE);
		}
	}
}
```

And the result:

win32_12_1_2.jpg

win32_12_2_2.jpg

win32_12_3_2.jpg
