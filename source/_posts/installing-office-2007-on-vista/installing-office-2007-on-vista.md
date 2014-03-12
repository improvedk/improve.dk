permalink: installing-office-2007-on-vista
title: Installing Office 2007 on Vista
date: 2006-12-04
tags: [Windows]
---
Ok, so I'd promised myself that I would at least give it a month before I'd install Vista on my desktop computer as I simply will not be able to cope with having major problems here. For work purposes I simply have to have a functional desktop computer.

<!-- more -->

Oh to heck with that! Having tried Vista on my laptop, I craved for seeing it expose it's full potential on a more capable machine. I fell into the pressure. I did it. I installed Vista on my desktop.

I've blogged the whole process, moving from the usual XP environment to Vista, all the new features, annoyances, problems and so forth. I won't be blogging about all of it as there's simply too much, instead I'll be releasing an article shortly describing the whole process. For now, I'll stick to a problem I had when I tried to install Office 2007 on the newly installed Vista machine (a problem which Google couldn't remedy). It was an absolutely fresh install on a formatted drive, all that I'd installed so far were some device drivers. Barring this, Office 2007 should install without a hitch right? Guess not.

officeinstall_1_2.jpg

I started the installation without problems, chose my setup configuration and began the actual installation. After about 6-7 minutes it was at the end of the installation according to the process bar. An hour later it was still at the end of the process bar without anything visibly happening. I checked the task manager, it was constantly using 10-15% CPU. I gave it another hour as I had to go eat anyways.

officeinstall_2_2.jpg

After the second hour it had still not moved anywhere, I'd guess you can call it dead by now. I cancelled the setup (by clicking the red X), but this time it hung again, halfways in the uninstall. Half an hour later I killed the process as nothing was happening.

officeinstall_3_2.jpg

Then this friendly fella popped up, teling me that the uninstallation probably didn't go as planned, well woope de doo. Clicking "Uninstall using recommended settings" performed the uninstall without problems (I seriously wonder what the "recommended" settings are, seeing as there are no settings in regards of an uninstall).

officeinstall_4_2.jpg

Reinitiating the Office 2007 install from the DVD resulted in this error - it gets more and more cryptic. I have sucessfully used this DVD to install Vista on Windows XP without problems, so it can't be the DVD. I then resorted to the true swizz army knife of any software developer - a reboot.

After rebooting I initiated the install again, and what do you know, it actually went all the way through this time without any problems. Finally! Or so I thought at least.

officeinstall_5_2.jpg

"Failed to register a VB Script DLL. Reinstall or run Regsvr32.exe Vbscript.dll to self register. 'C:Program FilesMicrosoft OfficeOffice12FORMS1033scdrestl.ico' is not a valid icon file."

When starting up Outlook, I receive this error message (during the "Setting up accounts" initialization phase). Clicking OK (or the red X) resulted in Outlook closing down and me receiving yet another almost identical message:

officeinstall_6_2.jpg

"'C:Program FilesMicrosoft OfficeOffice12FORMS1033scdrestl.ico' is not a valid icon file."

More or less the same error message, though this time the title of the dialog was "SetPoint" - the mouse/keyboard driver software for my Logitech diNovo keyboard - what the fudge?

officeinstall_7_2.jpg

OK, it told me to either "self register" or uninstall. As I have no idea what "self register" incorporates (I tried to regsvr32 Vbscript.dll to no avail, and I hardly think a .ico file can be regsvr32'ed), I went for the uninstall. But this all to familiar dialog box popped up. After about five reboots and several torn images of Bill Gates I realized the I was not going to get this bugger uninstalled (this dialog also appeared through the "Programs and Features" control panel when trying to uninstall from there). So what's the next step?

officeinstall_8_2.jpg

Please do not ask me how, I really can't remember how I dug myself in here, the important thing is that I did. I tried running both .MSI files. I have no idea what they did, the just executed with no GUI and then closed again in about one second. After running these MSI's, I was suddenly able to run the Office installer again. It acted as though Office wasn't even installed on my system so I couldn't uninstall. I decided to just do an install again, hopefully overwriting any buggy files that might have caused the problem.

officeinstall_5_21.jpg

Well, the good news is that the install went through problem free. The bad news is that the main problem persisted.

officeinstall_9_21.jpg

I dug out the 2KB .ICO file that seemingly was corrupt (and was able to bring down Microsoft Outlook 2007 - talk about David vs. Goliath!). Windows Photo Gallery (the Vista pendant of the XP Preview application) wasn't able to display the ICO file. As it was able to display all of the other .ICO's in the same folder I think it's safe to conclude that the file was somehow damaged.

officeinstall_10_2.jpg

Then I took the damaged SCDRESTL.ICO file and prepended the file name with an underscore. Afterwards I simply took a copy of one of the other working .ICO files (SCDRESTS.ICO) and renamed it SCDRESTL.ICO.

officeinstall_11_2.jpg
