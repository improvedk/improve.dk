permalink: presenting-at-sqlbits-x
title: Presenting at SQLBits X
date: 2012-01-26
tags: [Conferences & Presenting]
---
[SQLBits X](http://www.sqlbits.com/" target="_blank) is coming up soon, and by the looks of it, it’ll feature a full house of excited attendees:

<!-- more -->

image_4.png

If you haven’t been before, just take a look at these [ten reasons, provided by Simon Sabin](http://sqlblogcasts.com/blogs/simons/archive/2012/01/26/ten-reason-to-attend-sqlbits-x.aspx?utm_source=twitterfeed&utm_medium=twitter&utm_campaign=Feed%3A+SimonsSqlServerStuff+%28SimonS+SQL+Server+Stuff%29" target="_blank). Not convinced yet? Take a look at [10 more reasons, provided by Jonathan Allen](http://www.simple-talk.com/community/blogs/jonathanallen/archive/2010/09/20/94522.aspx#105686" target="_blank) (though written for SQLBits 7, they’re just as applicable for SQLBits X).

## Revealing the Magic

To my big surprise, I’ve got another chance at speaking at SQLBits – I must’ve done [something right](http://improve.dk/archive/2011/12/05/sqlbits-9-session-evaluation.aspx" target="_blank) after all :)

I will once again be presenting a session based on my work with [OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank). Here’s the abstract:

<blockquote>Think SQL Server is magical? You're right! However, there's some sense to the magic, and that's what I'll show you in this level 500 deep dive session. Through my work in creating OrcaMDF, an open source parser for SQL Server databases, I've learned a lot of internal details for the SQL Server database file format. In this session, I will walk you through the internal storage format of MDF files, how we might go about parsing a complete database ourselves, using nothing but a hex editor. I will cover how SQL Server stores its own internal metadata about objects, how it knows where to find your data on disk, and once it finds it, how to read it. Using the knowledge from this session, you'll find it much easier to predict performance characteristics of queries since you'll know what needs to be done.</blockquote>

The basis of the session is the same as [the one I gave last year](http://improve.dk/archive/2011/08/03/sqlbits-9-agenda-published.aspx" target="_blank). However, based on the feedback I got, as well as the work I’ve done with OrcaMDF since then, I’ll be readjusting the content a bit. I’ll not be covering disaster recovery, just as I’ll trim some sections to leave room for some new stuff like compression internals.

Just as last time, the session is meant to **inspire**, not to teach. There’ll be far too much content to understand everything during the session. My hope is to reveal how SQL Server is really governed by a relatively small set of rules – and once you know those, you’ve got a powerful tool to add to your existing arsenal.
