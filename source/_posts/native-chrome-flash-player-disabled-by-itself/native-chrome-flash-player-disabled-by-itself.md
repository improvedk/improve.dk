permalink: native-chrome-flash-player-disabled-by-itself
title: Native Chrome Flash Player Disabled by Itself All of a Sudden
date: 2013-02-27
tags: [Miscellaneous]
---
At my job we've got a product that relies heavily on Flash. The last couple of days I've had a number of users complain that, all of a sudden, they couldn't view Flash content any more. Common for all of them were their browser - Chrome. It would seem that, somehow, the native Chrome Flash player got disabled by itself all of a sudden.

<!-- more -->

What's especially unusual about this is that Chrome has a built-in Flash player, so if anyone, Chrome users should be able to view Flash content. Digging deeper I found that the built-in Flash player extension had been disabled. To check if that's the case, see here:

```sql
Chrome Settings => Show advanced settings... => Privacy => Content settings... => Plug-ins => Disable individual plug-ins...
```

Flash.png

By just clicking "Enable", everything is working again. But how did it get disabled? This is such a convoluted place to find that I know the users haven't done so themselves. Looking at Twitter, it seems we're not alone in seeing this:

https://twitter.com/AnandaWoW/status/306751670258388992

https://twitter.com/RachofSuburbia/status/306426446438617088

https://twitter.com/linnysvault/status/306420799550660608

https://twitter.com/Astracius/status/306351364710219776

https://twitter.com/junctionette/status/306230350131130370

https://twitter.com/envyonthetoast/status/306210978201219073

... I think you get the picture. It seems that all of our users had just had their Flash player auto update itself. I'm wondering, could the Internet Explorer Flash plugin perhaps updated itself and, by mistake, disabled the Chrome plugin? If the built-in Chrome Flash player is disabled, Chrome will try to use the regular Flash plugin. However, the Internet Explorer version won't work in Chrome, so that won't work.

Anyone else experienced this? Any tips on what's causing it? The fix is simple, but I'd really like to understand what's causing this, as well as knowing how widespread the issue is.