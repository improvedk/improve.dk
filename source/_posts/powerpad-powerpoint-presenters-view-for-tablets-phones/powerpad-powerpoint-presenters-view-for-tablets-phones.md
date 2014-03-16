permalink: powerpad-powerpoint-presenters-view-for-tablets-phones
title: PowerPad - Powerpoint Presenters View for Tablets & Phones
date: 2013-10-28
tags: [.NET, Conferences & Presenting, Tools of the Trade]
---
I love presenting, especially so when it's possible for me to do so alongside Powerpoints presenters view. Unfortunately I'm an even bigger fan of [ZoomIt](http://technet.microsoft.com/en-us/sysinternals/bb897434.aspx) and I use it extensively when presenting. Why is that an issue? To use ZoomIt effectively, not just in demos but when showing slides as well, I need to duplicate my screen rather than extending it. Duplicating the screen means presenters view is not an option :(

<!-- more -->

## Introducing PowerPad

Seeing [as I've already got my iPad next to me when presenting](/keeping-track-of-time-while-presenting/) it seems obvious to use that for the presenters view. However, even though I've scoured the app store for solutions, I have yet to find something that doesn't require me to install invasive clients on my computer or suffice with a fixed & lagging UI on the iPad. Even worse, most require me to pay up front, meaning I can't perform a meaningful trial.

And so I decided to [do something about it](https://github.com/improvedk/PowerPad). PowerPad is a simple console application that runs on your computer, detects when you run a presentation and automatically provides a "presenters view" served over HTTP. The overall goal for PowerPad is to provide a Powerpoint presenters view for tablets & phones.

presentation_started.png

As soon as you're running PowerPad, and a presentation, you'll now be able to access the host IP through any device with a browser. I personally use my iPad:

screen_notes.png

And in a pinch I might even use my phone:

screen_mobile.png

## Getting Started
PowerPad is open source and completely free to use, licensed under the MIT license. It currently supports Powerpoint 2013 and only requires you to have the .NET 2.0 Framework installed. As long as your devices are on the same network, you can hook up any number of secondary monitors to your presentation - even your attendees, should you want to.

For more screenshots as well as the code & downloads, please check out the [PowerPad page on Github](https://github.com/improvedk/PowerPad).
