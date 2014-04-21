---
permalink: multi-monitoring-with-style
title: Multi Monitoring With Style
date: 2006-11-29
tags: [Tools of the Trade]
---
As a developer I strive to increase my productivity. In contrast to most people I know, I'm able to multitask at an unusual level. I often sit at my desktop, working on several projects at the same time (literally), I may also watch multiple movies/tv-series at the same time and so forth.

<!-- more -->

So how do we increase productivity? I'll stick with the hardware/software aspect and ignore the psychological issues (which are not to be neglected, the definitely play a big part as some people just seem to be unable to multitask at any level).

The one thing that has increased my productivity the most was when I bought my my dual-monitor setup (two 20" Dells). Having dual monitors is a world of difference in contrast to having just a single monitor, no matter the size of that single monitor. I've had several discussions with friends who would rather have a single 24" monitor instead of two 20" or two 17-19" (there's no real difference between 17-19" monitors as they all support the same resolution). I would any day choose two low grade 17" monitors over a single 24" monitor, simply because ones productivity is a lot greater when having dual monitors. But why?

The problem with multi tasking on a single monitor is that we tend to - and Windows is built for - have our windows maximized. Maximizing windows is great, it saves us a lot of work, moving windows around so all the open windows fit, resizing them and so forth. By maximizing them we simply have to double click the titlebar and we're done. But that's not all good. The problem with maximizing windows is that we can only have a single active window at a time, requiring us to alt-tab around between the windows, or even worse, moving our mouse between the start menu and the actual window content area, wasting a lot of time.

Having multiple monitors enables you to have multiple maximized windows at the same time, as maximizing a window will only maximize it on the monitor it's currently at. You'd be surprised at how often having just two maximized windows is all you need when working. As a developer we'll usually have Visual Studio and Internet Explorer running (for web development), or possibly VS + SQL Server Management Studio and so forth. It also gives one a much better perception of the current work task as you can have a lot of arbitrary information lying around on the secondary monitor while working on the primary monitor.

Let me talk a bit about how to optimally place your monitors.

## Dual monitor setup

The most intuitive thing to do when setting up a dual monitor setup is to place both monitors on each side of the center point, in effect, placing you directly in between both monitors, having a great view on both of them.

monitorsetup_2.jpg

This however, is not optimal! You should place yourself directly in front of one of the monitors, the *primary* one. You will be concentrating your focus on just one of the monitors for 90% of the time, very very rarely will you need to actually look at both monitors at the same time. Placing yourself in the middle will (although you may not be consciously aware of it) cause you to twist your neck to one side continuously. By placing the primary monitor straight ahead of you, you can sit at a normal angle for most of the time, and it feels natural to look over on the other monitor when you actually need it.
## Quad monitor setup
What's the next natural step after dual monitors? Quad monitors of course!

monitor_old_2.jpg

This was my second setup, after my dual monitor setup. Let me give you a warning right now and here, you should *not* get a setup like this unless you have a really explicit need for quad monitors. I loved this setup while I was setting it up, I loved it for about a week (simply because I had to, after all it wasn't really that cheap). But the fact is that it's utterly useless. Again, you'll have to position the primary monitor right in front of you, but on most quad monitor stands, it's not really possible to customize the setup very much. Also the lower monitors will simply be too low for you to look at them naturally (and the top ones will be too high), so this setup will cause you to constantly sit in a bad position, not really having any monitors feeling natural to look at.

## Triple monitor setups

Before I had my quad setup, I had three of those 20" Dell's. That was actually a quite good setup, and that's what I'm going to describe here. Unfortunately you'll have to do with a picture of my current setup (two 20"'s with a center 30") as I can't find any pictures of my trip-20"'s setup.

monitor_current1_2.jpg

Having three monitors is a great solution if you really need a lot of monitor real estate. The obvious advantage is that there's a center monitor that will quite naturally become the primary monitor. The two side/satellite monitors will become secondary monitors where you'll keep all the arbitrary info while working. Again, the most important factor here is that you'll be able to work on your primary monitor while looking at a an angle that feels natural.

## Required hardware

Most people think it requires a lot of expensive hardware to run multiple monitors - that's a myth! Most graphics cards today actually come with dual monitor outputs. It doesn't matter if ones VGA and the other's a DVI output, as long as you're able to connect your monitors, it'll work. If you're going to buy a new graphics card, please do yourself a favor and get one with DVI outputs as it delivers a better picture on TFT's.

As for monitors, you really should get yourself monitors that support the same height resolution. It's a pain when you can't move your mouse clearly from one monitor to the other since the resolution doesn't allow you to do so.

## Software

Windows is great at automatically setting up your x-monitor system. It'll automatically recognize the monitors and stretch your desktop across all monitors. You don't need any 3rd party software to run multiple monitors.

monitorproperties_2.jpg

In the usual display properties control panel, you'll be able to arrange your monitors and setup how they should behave when you move your mouse from one monitor to the other. If the physical left monitor appears to be on the right side of your center monitor, simply switch them around here.

You should note that there essentially three different multi monitor modes (I'll be using nVidia terms as that's what I'm using).


* DualView makes each monitor act as an individual monitor, that means you'll be able to maximize windows on that monitor and make it behave as a stand alone monitor.
* Clone does what the name implies, it simply clones the contents of your primary monitor onto your secondary monitor.
* Horizontal span will combine all of your monitors to one giant monitor. It's highly unusable as maximizing a window will cause it to be maximized across all monitors - which isn't really that useful as you'll probably have some space between the displays, causing an interruption in the middle of your favorite application.


## UltraMon

While you don't *need* any 3rd party software to run multiple monitors, you're going to need some to do it effectively! When you connect multiple monitors to Windows, it'll only show your taskbar on the primary monitor, and it's a pain in the ass to move your mouse all the way from the secondary monitor to the primary monitor, just to switch applications. UltraMon will put a taskbar on all of your monitors. The UltraMon taskbar works just like the Windows one, it'll only show the windows that are currently on that particular monitor. Also UltraMon enables you to setup keyboard shortcuts for moving a window from one monitor to another, I use Ctrl+Alt+Shift+Left/Right for moving a window to the previous/next monitor. UltraMon does a lot more, but these are the most important features. Oh yeah, it also enables you to make multi monitor wallpapers:

monitor_current2_2.jpg

UltraMon is a bargain at $39 for a non time limited version. [Click here to go the the UltraMon website](http://www.realtimesoft.com/ultramon/).

There's a free alternative to UltraMon, MultiMon. While in my experience MultiMon did not work as stable and feature complete as UltraMon, it's great if you don't wanna spend the $39. [Click here to go the the MultiMon website](http://www.mediachance.com/free/multimon.htm).

## allSnap

allSnap makes all your window snap to the edges of your monitor(s), it may also snap to other windows, it can be customized to any way you like. allSnap is great when you have to have several windows open on one monitor, it makes it a lot quicker for you to resize the windows so you utilize your full monitor area.[Click here to go the the allSnap website](http://www.cs.utoronto.ca/~iheckman/allsnap/).
