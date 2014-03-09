permalink: the-internals-of-spam-mails-part-one
title: The Internals of Spam Mails - Part 1
date: 2006-12-27
tags: [Miscellaneous]
---
Is your name William? Do you normally write mails with the subject "Re: hi"? Are your mails usually 11,304 characters in length? Guess what, you're a spammer! I present to you, an article containing a textual analysis of about 15,000 spam mails.

Before you ask, what can these numbers be used for? Good question, I'm open to suggestions!

The following numbers are based on 15,073 spam mails in total. Naturally demographics will play a part, such as my interests, part of the world and so forth. Let me give you the 5 word tour of myself: Denmark, male, 21, development, poker.

## Importance

All mails can be marked with an importance flag ranging from low to high through normal. I'd think spammers would consider their mails rather important, guess not.

spam1_2.jpg

## Body format

Most anti spam applications today are extra sensitive towards HTML mails so it's no big surprise that they're not that common.

spam2_2.jpg

## Number of recipients

A lot of mail servers as well as anti spam clients will filter out mass mailed emails, that is, emails that have a lot of recipients. Some mails will figure as having zero recipients, these may be caused by bad header formats / values in the mail.

spam3_2.jpg

## Reply-to field usage

The reply-to field enables you to specify an email address used for replies other than the one that was used to send the email with. How many mails utilize this field? (0 = not used, 1 = used)

spam4_2.jpg

## Mail size

Don't you just hate it when you startup Outlook and you have to wait for ages due to several terabytes of spam mails waiting to be downloaded? But how many bytes does an average mail actually take up?

spam5_2.jpg

## Mail lengths

How long is the average spam mail? The more text, the less chance one will actually read it, right? As for the subject (in number of characters):

spam6_211.jpg

Note that the 255 upper limit is usually caused by either the mail servers or the mail clients capping the subject at 255 characters. As for the body:

spam7_2.jpg

## Spammer names

What's William and Richard got to do with each other? They both send a major junkload of spam.

spam8_2.jpg

## Subjects

I'm quite surprised that Viagra is not even in the top 10 subjects, almost disappointed.

spam9_2.jpg

## Common body words

What's the number one used word in the body of spam mails? Pretty boring: "the".

spam10_2.jpg

## Unusual body words

I've defined an unusual word as one that's not among the top 300 most common english words - according to: [http://www.esldesk.com/esl-quizzes/most-used-english-words/words.htm](http://www.esldesk.com/esl-quizzes/most-used-english-words/words.htm).

spam11_2.jpg

## Common subject words

How about the subject?

spam12_2.jpg

## Unusual subject words

Same definition as for the unusual body words, this time in regards of the subject.

spam13_2.jpg

## Common sentences

Trying to make any sense out of a common spam mail is not an easy task. Hence the following top 20 common sentences may not make a whole lot of sense either.

Count: **494**  
The attention to detail is paramount and they are comparable to the originals in every way.

Count: **481**  
Having had this same model for several years i was hesitant to spend the money again.

Count: **481**  
You will be very impressed with the quality.

Count: **475**  
Lange sohne glashute original audemars piguet jaeger-lecoultre officine panerai alain silbersteini got my watch yesterday and love it.

Count: **464**  
Replicated to the smallest detail 98 a accuracy includes all proper markings wide selection and fast worldwide shipping authentic weight true-to-original self winding and quartz mechanismsour faithful duplications include these prestige brands rolex mens rolex ladys rolex boys watch box sets patek philippe iwc cartier bvlgari frank muller breitling omega tag heuer chopard vacheron constantin a.

Count: **344**  
Comprestige replicasregards-mens and ladies prestige watches for all occasionsthese replicas have all the presence and poise of the originals after whome they were designed at a fraction of the cost.

Count: **168**  
General fewer your on can try answers for expert help with search.

Count: **135**  
The only way to solve it is to take viagra or cialis super viagra medications before you have sex.

Count: **135**  
You can spend thousands and buy them at your local drug store.

Count: **134**  
03 this problem is called erectile dysfunctioned.

Count: **132**  
Globally the potential market is a staggering 45 billion.

Count: **132**  
Video streams to wherever you choose to watch it.

Count: **105**  
20as the population ages the economic value in the us market for adult daycare is projected to grow nearly 600.

Count: **95**  
Major discoveries are happening all the time and wbrs is in the thick of it.

Count: **95**  
With the array of drilling projects wild brush has going on at the moment tension is building.

Count: **95**  
As the drilling gets closer to completion insiders are accumulating ahead of that major discovery announcement.

Count: **94**  
Get the latest messages emailed to alerts.

Count: **94**  
All we can say is that this one is going to see amazing appreciation in a very short period of time this is your opportunity.

Count: **90**  
You have seen it on 60 minutes and read the bbc news report -- now find out just what everyone is talking about.

Count: **90**  
Suppress your appetite and feel full and satisfied all day long increase your energy levels lose excess weight increase your metabolism burn body fat burn calories attack obesity and more.

## To be continued...

That's all for part 1. Hopefully I'll have come up with some more interesting analysis for the next part :) Feel free to post a suggestion if there's something specific you'd like to see some numbers of. Also, if you have access to large quantities of spam mail (no, not in the form of signing me up for billions of sites) in Outlook PST format, please de send me a message, I'd really like to have a larger sample to base my analysis upon.
