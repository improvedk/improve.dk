permalink: sqlbits-9-session-evaluation
title: SQLBits 9 Session Evaluation
date: 2011-12-05
tags: [Conferences & Presenting]
---
I’d heard many rumors about the excellent SQLBits session evaluation results that speakers are sent. Knowing the SSRS geeks on the SQLBits team, I’d expect nothing short of data & graph filled reports – and I’m glad to say they didn’t disappoint!

<!-- more -->

Besides my [precon](/presenting-a-precon-at-sqlbits), I presented my [Knowing the Internals, Who Needs SQL Server Anyway](http://sqlbits.com/Sessions/Event9/Knowing_the_Internals_Who_Needs_SQL_Server_Anyway) session. Before I start – thanks to all of you who attended my session(s), and especially thanks to those of you who filled out the feedback forms! Unfortunately the A/V people had some troubles with the microphone, so the video cuts off after about 10 minutes. Hopefully my SQLRally Nordic presentation will be made public – if/when it does, I’ll post it here.

## The results

Looking at the overall results, I’m extremely pleased to see that I’m pretty much top rated for the demos, expectations and overall categories. While my knowledge trumps all the other ratings I received, someone clearly set the bar extremely high there – I suspect it might be [Thomas Kejser](http://blog.kejser.org/) as his session was simply out of this world. Note that the graph is kind of misleading as the scale goes from 1-9, not 10.

image_4.png

## Score distribution

Looking at the score distribution, I got a single 4 and a single 5, while the rest is 7+ with a heavy bias on 8’s and 9’s – can’t be anything but extremely satisfied. That being said – I’d love to weed out that single 4 & 5, thankfully they’ve included comments with their feedback.

image_6.png

## Comments

I’ve listed all of the comments I got, including the average score for that comment (on a scale of 1-9). There were 20 evaluations in total, and out of those, 14 were commented. I, as all speakers, really appreciate the comments, whether to critique or praise – thank you!

> 9 - Excellent content, great overview of what can be achieved.

> 9 – Was a lot to cover in one hour but Mark did a fantasic job, has really inspired me to learn a lot more.

> 9 – Very impressive!

> 9 – Thanks for sharing. This wasn't the most useful but a great deep dive into SQL internals and did inspire which I think is the purpose of a 1 hour session.

> 9 – Far too much spare time on your hands! Enjoyed it though, makes you think about how you store data and use the correct data types.

> 9 – While overwhelming at times this was probably my favourite session of SQLBits, It was a very insightful look at the computing principles that make SQL Server work. Considering the very technical content Mark did an excellent job of explaining his thought processes. The slides were very clear and the demos when stopping SQL Server, modifying the file in a Hex editor and then restarting to see the updated stats really joined everything together.

> 9 – One of the presentations of the few days for me. Whilst the material was probably a little bit too detailed for any practical use I may currently have, it's great to watch someone with great presentation skills (rare for a lot of techies?), sense of humour and a genuine enthusiasm for the work/hobby he is undertaking.

> 9 – Very impressive session. Technical level of the session was very high (as stated at the start). Watched Klaus Aschenbrenner's internals session from previous SQL bits and felt this was a good pre-cursor. Will definitely be downloading OrcaMDF.

> 8.66 – My favourite talk of the conference. Absolutley superb.

> 8.5 – Mark handled a topic with many grey areas with consummate ease. Lots of undocumented detail, quite incredible perseverance with a topic that is largely a black box.

> 8.5 – Excellent session - wished id have done the Full day thursday!!!! Marks knowledge in the subjects was second to none!!

> 8 – Very interesting session. Not entirely useful yet, but I can believe that by continuing to reverse engineering the file format this would be very useful knowledge to have.

> 7.66 – Really interesting stuff. Possibly not immediately usable in the work place, but it's good background information.

> 7.5 – Tough choice between this one and around the world session. Only negative was a bit heavy on the slides but this is a tough topic to demo in 1 hour so maximum respect for pulling this off. Also, it was made clear that the session was to inspire and not teach. He had to push through the session quickly to make the hour and Mark did it well.

## Conclusions

*This session is not meant to teach, but to inspire.*

That’s how I started out my session, and with good reason. Originally I held a 45 minute session at Miracle Open World earlier this year, which laid the foundation for my precon and session at SQLBits. While developing my precon, I suddenly ended up with a lot of material, way too much to present in just a single hour. I had to compress it somehow, so I basically had two choices – ditch 90% of the material, or ditch just 20% of the material and speed through the rest. I opted for the latter.

I made the choice that I’d rather give people the same experience that I had at my first SQL Server conference, several years ago. I went to a basic internals session, and suddenly it dawned upon me, how SQL Server wasn’t magic – it’s merely a bunch of deterministic rules for juggling with bytes on disk. Since then, I’ve been on a quest of learning how to SQL Server catches its fish, rather than just eating the fish it provides.

While I can’t deem any of the comments or ratings as negative, the only critique I got was on the direct practical usability of the content, as well as the amount of material presented in one hour. For both of those, I fully recognize what’s being said. As a result, some of my SQLBits X submissions are sessions that go in-depth on specific topics, that I’d otherwise just skimmed in my last session:

* [OrcaMDF – Real Life Usage](http://sqlbits.com/Sessions/Event10/OrcaMDF-Real_Life_Usage)
* [Creating the Optimal Schema for Storage Efficiency](http://sqlbits.com/Sessions/Event10/Creating_the_Optimal_Schema_for_Storage_Efficiency)
* [Storing Character Data Optimally](http://sqlbits.com/Sessions/Event10/Storing_Character_Data_Optimally)
* [Optimizing Performance Using Page and Row Compression](http://sqlbits.com/Sessions/Event10/Optimizing_Performance_Using_Page_and_Row_Compression)
* [Revealing the Magic](http://sqlbits.com/Sessions/Event10/Revealing_the_Magic)

Revealing the Magic is the same session as Knowing the Internals – Who Needs SQL Server Anyways? I’ve renamed it to better reflect what my purpose with the session is – to reveal the magic. To hopefully provide that revelation of what SQL Server really is, once we hit the disk.

I look forward to an excellent 2012 with, hopefully, a lot of presentations to come!
