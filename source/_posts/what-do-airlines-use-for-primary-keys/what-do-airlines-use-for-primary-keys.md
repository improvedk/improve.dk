permalink: what-do-airlines-use-for-primary-keys
title: What Do Airlines Use for Primary Keys?
date: 2011-10-20
tags: [SQL Server]
---
On my way home from the PASS Summit in Seattle, I had a layover in Amsterdam before continuing onto Copenhagen. For various reasons, we were about one and a half hours delayed, and I arrived in AMS at 9:30, my CPH flight departing at 9:35. As you'd probably guessed, I missed my flight.

<!-- more -->

To fix it, I was told to do a check-in at one of the self-service counters at the transfer desk. Apparently these both do normal check-ins, as well as (supposedly), get you a replacement flight in case you missed the normal one. I stuck my passport into the scanner and it gladly popped up with a new flight to Copenhagen about 4 hours later. Great! Well, except my name wasn't on the check-in list, two other (unknown to me) Danish names were. However, I did share a surname with one of them.

delta-air-france-klm_2.jpg

After asking one of the attendants I was told, “Oh, that's because your flight has already departed, it'll try to find the nearest match”. Áha, so if there's no 1:1 match, it'll just try to find the best match of an existing booking to a non-departed flight – and apparently the best it could find was a two-person ticket for a flight 4 hours later. Now, it was for the same destination, and I did share my surname with one of the passengers, but it surely wasn't my ticket. Still, I was able to check-in as them and confirm “my” ticket.

So this leaves me the question, what in the world do they (Delta/KLM/AF) use as the primary key for their ticketing system? Once my passport is scanned, and they've got my passport number, how can there be *any* doubt as to who I am? How in the world can I get to check-in two completely, to me, unrelated passengers on a flight I'm not going on myself?

I ended up getting booked for another flight to another (closer to my home) destination in Denmark, but it did require manual intervention from the service desk.
