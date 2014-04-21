---
permalink: blog-revamp
title: Blog Revamp
date: 2013-02-20
tags: [Life, Miscellaneous]
---
Having been at it for almost 11 years, I've been through a number of blog revamps through the time. And here we are, once again.

<!-- more -->

[I first launched the blog back in 2002](http://web.archive.org/web/20020929133654/http://www.improve.dk/), though it was in Danish back then. In 2006 I decided to start over, removed all my Danish content and began blogging in English - at that time, I wrote my own blog engine in a couple of days on a couch in Henderson, Nevada, anxiously waiting to turn 21 so I could participate in the World Series of Poker.</a>

Back in 2011 I chose to migrate all my content onto [Subtext](http://subtextproject.com/). What I failed to notice was that the project was pretty much dead, so it was a migration doomed to be still born. It did however allow me access to [Windows Live Writer](http://en.wikipedia.org/wiki/Windows_Live_Writer) which served its purpose - it got me blogging again due to its simplicity in use. However, I quickly started to struggle with Subtext as templating was beyond difficult and I had to resort to ugly hacks to make it work the best I could.

wp-blue-640x9601.png

And so it was time for yet another revamp, this time, hopefully the last. Converting everything into Wordpress ought to be a simple task, if it wasn't for the fact that I had almost 250 posts and 700 comments. Most of them stemming from my homebrewed system, later haphazardly migrated to Subtext, and now lying before Wordpress. I had to write a custom exporter to get most of my content from Subtext into the [Wordpress WXR format](http://ipggi.wordpress.com/2011/03/16/the-wordpress-extended-rss-wxr-exportimport-xml-document-format-decoded-and-explained/). Dealing with attachments, encoding and comments was a major pain. Once I had everything into Wordpress, I had to go through each and every post and manually format the contents - having seven years of legacy left little to no structure, making it impossible to style.

At this point, all I needed to do was to find a good theme and I was up and running. Unfortunately I tend to be quite picky, so I couldn't find anything I really liked. In the end, I ended up writing my own theme, [which I've just published to Github](https://github.com/improvedk/improve.dk). My main priority has been to create a very simplistic theme that worked great across devices, while allowing me to post easily readable code snippets. If you have any suggestions for improvements, I'd love to hear them!
