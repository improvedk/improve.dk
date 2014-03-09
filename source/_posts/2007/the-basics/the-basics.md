permalink: the-basics
title: The Basics
date: 2007-04-02
tags: [.NET]
---
First of all, to manipulate and use the Win32 API, we must know about the system itself, the windows, the controls and so forth. Visual Studio comes bundled with Spy++ which enables us to identity the various windows and controls of application, but honestly, it's pretty bad. Instead you should [download Winspector](http://www.windows-spy.com/).

## Threads &amp; processes

Each application/window in Windows belongs to a given thread under a given process. A process may have multiple threads and windows, but a thread and a window can only belong to a single process.

## Windows &amp; handles

Usually we refer to a window as the overall container of visual representation of data in Windows, in this case it's a bit more detailed though. In all API work a window is a standard Win32 control, that could be a button, a toolbar, a label, a window (as per the usual definition) and so forth. Each window is assigned a handle, and by using this handle we can uniquely reference any given window. A window may have any number of child windows, that could be the controls of a dialog box, and the children may have children themselves and so forth.

Try and mess around with Winspector a bit. Identify the various windows of the system (including those that are not visible). Inspect their properties, the process they belong to, their parent windows and so forth. Most, if not all, of the API tips &amp; tricks posts will include references to windows, handles, processes and threads - so get used to them.
