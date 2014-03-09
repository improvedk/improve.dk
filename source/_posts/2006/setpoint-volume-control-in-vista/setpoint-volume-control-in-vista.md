permalink: setpoint-volume-control-in-vista
title: SetPoint Volume Control In Vista
date: 2006-12-04
tags: [Windows]
---
It seems Logitech has some pretty functional SetPoint drivers out for Vista already (SetPoint is the all-purpose driver software for all their keyboards and mice).

The ony thing that did not work was the volume control. I could turn it up and down and the visual volume meter would respond (the SetPoint generated one, not the Vista one), but nothing would actually happen.

Fortunately there's an easy solution.

setpoint_1_2.jpg

Navigate to "C:Program FilesLogitechSetPoint". Now make sure that the "SetPoint.exe" process is NOT running - you may have to click Ctrl+Shift+Escape and end the process in the task manager.

setpoint_2_2.jpg

Now rightclick SetPoint.exe and select "Properties". Navigate to the Compatibility pane and set it to run in compatibility mode as Windows XP (Service Pack 2). After this, double click SetPoint.exe to restart the process. Your volume control should now work!
