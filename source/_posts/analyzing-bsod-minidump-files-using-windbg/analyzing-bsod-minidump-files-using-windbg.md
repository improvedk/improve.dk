permalink: analyzing-bsod-minidump-files-using-windbg
title: Analyzing BSOD Minidump Files Using Windbg
date: 2013-06-04
tags: [Windbg]
---
Unfortunately, once in a while, computers fail. If you're running Windows you've probably witnessed the dreaded [Blue Screen of Death](http://en.wikipedia.org/wiki/Blue_Screen_of_Death), commonly referred to as a BSOD. Once the BSOD occurs, some machines will immediately restart, before you've got a chance to actually see what happened. Other times users will just report that the BSOD happened, without noting anything down about what the message actually said. In this post I'll show you how analyzing BSOD minidump files using [Windbg](http://en.wikipedia.org/wiki/WinDbg) will enable you to find the cause of the BSOD after the fact.

<!-- more -->


## Enabling Dump Files

By default, never Windows installs will automatically create minidump files once a BSOD occurs. Once restarted, you should be able to see a .dmp file here:

```
C:\Windows\Minidump
```

If you don't see any .dmp files there, or if the directory doesn't exist, you may have to tell Windows to create minidump files when the BSOD occurs. To do so, press the **Win+Break** keys to open up the System control panel. Now click **Advanced system settings** in the left menu. Once there, go to the **Advanced** tab and click the **Settings...** button under the **Startup and Recovery** section. Now make sure the **Write debugging information** setting is set to **anything but "none"**:

Capture.png


## Analyzing BSOD Minidump Files Using Windbg

Once a dump file has been created, you can analyze it using Windbg. Start by opening Windbg and pressing the **Ctrl+D** keys. Now select the .dmp file you want to analyze and click **Open**. This should yield something like this:

```
Microsoft (R) Windows Debugger Version 6.12.0002.633 AMD64
Copyright (c) Microsoft Corporation. All rights reserved.


Loading Dump File [C:\Windows\Minidump\040813-15974-01.dmp]
Mini Kernel Dump File: Only registers and stack trace are available

Symbol search path is: symsrv*symsrv.dll*c:\symbols*http://msdl.microsoft.com/download/symbols
Executable search path is: 
Windows 7 Kernel Version 7601 (Service Pack 1) MP (12 procs) Free x64
Product: WinNt, suite: TerminalServer SingleUserTS
Built by: 7601.18044.amd64fre.win7sp1_gdr.130104-1431
Machine Name:
Kernel base = 0xfffff800`0300c000 PsLoadedModuleList = 0xfffff800`03250670
Debug session time: Mon Apr  8 22:17:47.016 2013 (UTC + 2:00)
System Uptime: 0 days 1:36:19.860
Loading Kernel Symbols
...............................................................
................................................................
........................
Loading User Symbols
Loading unloaded module list
...............
*******************************************************************************
*                                                                             *
*                        Bugcheck Analysis                                    *
*                                                                             *
*******************************************************************************

Use !analyze -v to get detailed debugging information.

BugCheck FE, {4, fffffa803c3c89e0, fffffa803102e230, fffffa803e765010}

Probably caused by : FiioE17.sys ( FiioE17+1d21 )

Followup: MachineOwner
```

Already this tells us a couple of things - your OS details, when exactly the problem occurred as well as what module probably caused the issue (FiioE17.sys in this case). Also, it tells you how to proceed:

> Use !analyze -v to get detailed debugging information.

As suggested, let's try and run the !analyze -v command:

```
11: kd> !analyze -v
*******************************************************************************
*                                                                             *
*                        Bugcheck Analysis                                    *
*                                                                             *
*******************************************************************************

BUGCODE_USB_DRIVER (fe)
USB Driver bugcheck, first parameter is USB bugcheck code.
Arguments:
Arg1: 0000000000000004, IRP_URB_DOUBLE_SUBMIT The caller has submitted an irp
	that is already pending in the USB bus driver.
Arg2: fffffa803c3c89e0, Address of IRP
Arg3: fffffa803102e230, Address of URB
Arg4: fffffa803e765010

Debugging Details:
------------------

CUSTOMER_CRASH_COUNT:  1

DEFAULT_BUCKET_ID:  VISTA_DRIVER_FAULT

BUGCHECK_STR:  0xFE

PROCESS_NAME:  audiodg.exe

CURRENT_IRQL:  2

LAST_CONTROL_TRANSFER:  from fffff88008326f4b to fffff80003081c40

STACK_TEXT:  
fffff880`0e482fd8 fffff880`08326f4b : 00000000`000000fe 00000000`00000004 fffffa80`3c3c89e0 fffffa80`3102e230 : nt!KeBugCheckEx
fffff880`0e482fe0 fffff880`0833244a : fffffa80`3ae97002 fffffa80`3b8caad0 00000000`00000000 fffffa80`3ae97050 : USBPORT!USBPORT_Core_DetectActiveUrb+0x127
fffff880`0e483030 fffff880`0833ae74 : fffffa80`3c3c89e0 fffffa80`3af7000a fffffa80`3c3c89e0 fffffa80`3102e230 : USBPORT!USBPORT_ProcessURB+0xad6
fffff880`0e4830e0 fffff880`08314af4 : 00000000`00000000 fffffa80`3af7b050 fffffa80`3e5d1720 fffffa80`3c3c89e0 : USBPORT!USBPORT_PdoInternalDeviceControlIrp+0x138
fffff880`0e483120 fffff880`00fa97a7 : fffffa80`3c3c89e0 fffffa80`31192040 fffffa80`3c3c89e0 fffffa80`3c3c89e0 : USBPORT!USBPORT_Dispatch+0x1dc
fffff880`0e483160 fffff880`00fb1789 : fffff880`00fcfb50 fffffa80`3d944ed1 fffffa80`3c3c8d38 fffffa80`3c3c8d38 : ACPI!ACPIDispatchForwardIrp+0x37
fffff880`0e483190 fffff880`00fa9a3f : fffff880`00fcfb50 fffffa80`316a7a90 fffffa80`3c3c89e0 fffffa80`3ab6c050 : ACPI!ACPIIrpDispatchDeviceControl+0x75
fffff880`0e4831c0 fffff880`088ca566 : 00000000`00000000 00000000`00000004 fffffa80`3ab6c050 fffffa80`3c2bd440 : ACPI!ACPIDispatchIrp+0x12b
fffff880`0e483240 fffff880`088fad8f : 00000000`00000000 00000000`00000000 fffffa80`3c2bd440 00000000`00000000 : usbhub!UsbhFdoUrbPdoFilter+0xde
fffff880`0e483270 fffff880`088c8fb7 : fffffa80`3c3c89e0 fffffa80`3a976ce0 fffffa80`3c3c89e0 fffffa80`3c3c89e0 : usbhub!UsbhPdoInternalDeviceControl+0x373
fffff880`0e4832c0 fffff880`00fa97a7 : fffffa80`3c3c89e0 fffff800`031b630d fffffa80`3b7be100 00000000`00000801 : usbhub!UsbhGenDispatch+0x57
fffff880`0e4832f0 fffff880`00fb1789 : fffff880`00fcfb50 00000000`00000001 fffffa80`3c393b58 fffffa80`3c3c8d38 : ACPI!ACPIDispatchForwardIrp+0x37
fffff880`0e483320 fffff880`00fa9a3f : fffff880`00fcfb50 fffffa80`316a8a90 fffffa80`3c3c89e0 fffffa80`3c393b58 : ACPI!ACPIIrpDispatchDeviceControl+0x75
fffff880`0e483350 fffff880`08c9bec4 : 00000000`00000000 fffffa80`3c326938 fffffa80`3c393b58 00000000`00000000 : ACPI!ACPIDispatchIrp+0x12b
fffff880`0e4833d0 fffff880`08c98812 : fffffa80`3c393b58 fffffa80`3c3c89e0 fffffa80`00000324 fffffa80`3c3c89e0 : usbccgp!UsbcForwardIrp+0x30
fffff880`0e483400 fffff880`08c98aba : fffffa80`3c326838 00000000`00220003 fffffa80`3c3c89e0 fffffa80`3c393b58 : usbccgp!DispatchPdoUrb+0xfa
fffff880`0e483440 fffff880`08c9672e : 00000000`0000000f fffffa80`3c393b50 fffffa80`3c393b58 fffffa80`3c3c89e0 : usbccgp!DispatchPdoInternalDeviceControl+0x17a
fffff880`0e483470 fffff880`08cb3d21 : fffffa80`3c393a00 fffffa80`3c3c8901 fffffa80`3c3c8900 00000000`00000000 : usbccgp!USBC_Dispatch+0x2de
fffff880`0e4834f0 fffffa80`3c393a00 : fffffa80`3c3c8901 fffffa80`3c3c8900 00000000`00000000 fffffa80`3c373010 : FiioE17+0x1d21
fffff880`0e4834f8 fffffa80`3c3c8901 : fffffa80`3c3c8900 00000000`00000000 fffffa80`3c373010 00000000`00000000 : 0xfffffa80`3c393a00
fffff880`0e483500 fffffa80`3c3c8900 : 00000000`00000000 fffffa80`3c373010 00000000`00000000 fffffa80`3c3b7f30 : 0xfffffa80`3c3c8901
fffff880`0e483508 00000000`00000000 : fffffa80`3c373010 00000000`00000000 fffffa80`3c3b7f30 fffff880`08cb47fd : 0xfffffa80`3c3c8900


STACK_COMMAND:  kb

FOLLOWUP_IP: 
FiioE17+1d21
fffff880`08cb3d21 ??              ???

SYMBOL_STACK_INDEX:  12

SYMBOL_NAME:  FiioE17+1d21

FOLLOWUP_NAME:  MachineOwner

MODULE_NAME: FiioE17

IMAGE_NAME:  FiioE17.sys

DEBUG_FLR_IMAGE_TIMESTAMP:  50b30686

FAILURE_BUCKET_ID:  X64_0xFE_FiioE17+1d21

BUCKET_ID:  X64_0xFE_FiioE17+1d21

Followup: MachineOwner
```

This tells us a number of interesting things:

* The BSOD error was: **BUGCODE_USB_DRIVER**
* This is the error caused by the driver: IRP_URB_DOUBLE_SUBMIT **The caller has submitted an irp that is already pending in the USB bus driver**.
* The process that invoked the error: **audiodg.exe**
* The stack trace of the active thread on which the error occurred. Note that Windbg can't find the right symbols as this is a proprietary driver with no public symbols. Even so, to the developer of said driver, the above details will help immensely.
* The driver name: **FiioE17.sys**

With the above options, you've got a lot of details that can be sent to the developer, hopefully enabling him/her/them to fix the issue. For now, I'll have to unplug my [Fiio E17 USB DAC](http://www.amazon.com/FiiO-Headphone-Amplifier-Docking-Interface/dp/B0070UFMOW) :(
