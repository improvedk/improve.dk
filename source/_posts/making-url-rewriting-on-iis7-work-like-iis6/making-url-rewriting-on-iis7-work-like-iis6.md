permalink: making-url-rewriting-on-iis7-work-like-iis6
title: Making URL Rewriting on IIS 7 Work Like IIS 6
date: 2006-12-11
tags: [IIS]
---
Upgrading to IIS 7 should be rather transparent, unfortunately that is not the case when it comes to URL rewriting as we knew it from IIS 6. In IIS 6 all we had to do was to add a wildcard mapping making sure that all requests went through the ASPNET ISAPI process. After this was done, one could create a global.asax file that would either pass requests directly through or rewrite the URL based on an internal algorithm.

<!-- more -->

**UPDATE: Please see my [updated post](/how-to-do-url-rewriting-on-iis-7-properly) on how to do proper URL rewriting using IIS 7.**

I didn't really expect this to be a problem when I first requested http://localhost for the first time after setting up my site on IIS 7 (all default settings).

iis7ur1_2.jpg

Unfortunately this was what I was presented with. Anyone having worked with wildcard mappings from IIS 6 will recognize this, this is the result you'll get after adding a wildcard mapping without having created your URL rewriting functionality. After adding a wildcard map the IIS will not automatically find a default file (by design).

This however, is not the problem cause here. I already have my URL rewriting functionality written in my global.asax BeginRequest method and I'd really like to just reuse my current code. Although the new IIS has a whole new bunch of features - one of them being a new "more correct" way of doing URL rewriting -, I really just wan't to get my website up and running again so I can continue my work.

What I present below is a quick'n'dirty hack that will get my old URL rewriting code to work again. It may not be the IIS 7 way of doing it, and it may not work in your case, it depends on the type of URL mapping you're doing in your project. In short, [YMMV](http://en.wikipedia.org/wiki/Your_mileage_may_vary).

## My scenario

For this website, improve.dk, all URL's except static files are requested as though they were folders. That means you will not see any pages ending in anything but a /. Any static files are requested as usual. That means I can be sure that a regular expression like *.* will catch all static files, while * will catch all pages - as well as the static files!

## How I got URL rewriting to work like IIS 6

Start by opening the IIS Manager and selecting your website.

iis7ur2_2.jpg

Now enter the "Handler Mappings" section:

iis7ur3_2.jpg

Notice the "StaticFile" handler. Currently it's set to match * and catch both File and Directory requests. If you look back at the first image, you'll notice that the error message details that the handler causing the 404 error is the StaticFile handler. As I know that all my static files will have a file extension (also I don't care for directory browsing), I'll simply change my StaticFile handler so it only matches *.* - and only files.

iis7ur4_2.jpg

iis7ur5_2.jpg

Your StaticFile handler should now look like this:

iis7ur6_2.jpg

Now, if you go back and make a request to http://localhost you'll still get the 404 error, but this time the request is not handled by the StaticFile handler, actually it doesn't get handled by any handler at all:

iis7ur7_2.jpg

What needs to be done now is that we need to map any and all requests to the aspnet_isapi.dll isapi file - just like we would usually do in IIS 6.

Add a new Script Map to the list of Handler Mappings and set it up like this:

iis7ur8_2.jpg

iis7ur9_2.jpg

Click OK and click *Yes* at the confirmation dialog:

iis7ur10_2.jpg

Now if you make a request to either http://localhost or any other file you'll get the following error:

iis7ur11_2.jpg

Looking throug the Event log reveals the cause of the error:

iis7ur12_2.jpg

The aspnet_isapi.dll file cannot be used as a Handler for websites running in the new IIS 7 Integrated Mode, thus we will need to make our website run in classic .NET mode. Right click your website node in the IIS Manager and select Advanced Settings. Select the "Classic .NET AppPool" and close the dialog boxes:

iis7ur13_2.jpg

Now you should be able to make a request to http://localhost and see it work. Your URL rewriting should work as a charm aswell:

iis7ur14_2.jpg

But obviously somethings wrong. Making a request to any static file will reveal the problem:

iis7ur15_2.jpg

"Failed to Execute URL", what a great descriptive error. Fortunately you won't have to spend hours ripping out hair... As I have already done that - at least I'll save a trip or two to the barber.

The problem is that the static files are being processed by the aspnet_isapi.dll file instead of simply sending the request along to the StaticFile handler. If you click the "View Ordered List..." link in the IIS Manager from the Handler Mappings view, you'll see the order in which the handlers are being executed for each request:

iis7ur16_2.jpg

When you add a new Script Map it'll automatically get placed at the very top of the line taking precedence over any other handlers, including the StaticFile one.

What we have to do is to move our Wildcard handler to the very bottom, below the StaticFile handler. By letting the StaticFile handler have precedence over our Wildcard handler we ensure that any static files (matching *.*) gets processed correctly while any other URL's gets passed along to our own Wildcard handler that'll do the URL rewriting and make business work as usual:

iis7ur17_2.jpg

After doing so, both static files as well as your custom URL's should execute as they would under IIS 6:

iis7ur18_2.jpg

## Disclaimer

Please notice that this is a hack. This is not the way URL rewriting is supposed to be done under IIS 7. But instead of spending hours upon hours investigating how to do it the right way, this is quick fix to make things work like they did before.

Also please note that this solution is intended to work for my specific situation. Your needs for URL rewriting may not necessarily match mine, so you may have to modify certain settings to suit your specific needs.
