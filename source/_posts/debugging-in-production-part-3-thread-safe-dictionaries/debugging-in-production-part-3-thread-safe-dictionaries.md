permalink: debugging-in-production-part-3-thread-safe-dictionaries
title: Debugging in Production Part 3 - Thread-Safe Dictionaries
date: 2013-04-30
tags: [.NET, Windbg]
---
[In part 2 we found out that the concurrent access to a generic dictionary triggered a race condition bug](/debugging-in-production-part-2-latent-race-condition-bugs) that caused threads to get stuck at 100% CPU usage. In this part, I'll show how easy it is to rewrite the code, using the new thread-safe dictionaries in .NET 4.0, so it's protected from race condition bugs like the one I encountered.

<!-- more -->


## Enter ConcurrentDictionary

The problem can be solved by changing just two lines of code. Instead of using a generic Dictionary, we'll change it to a generic ConcurrentDictionary like so:

```csharp
private static readonly ConcurrentDictionary<short, ConcurrentDictionary<SettingDescription, SettingDescriptionContainer>> cache =
	new ConcurrentDictionary<short, ConcurrentDictionary<SettingDescription, SettingDescriptionContainer>>();
```

As described by this [MSDN article on adding and removing items from a ConcurrentDictionary](http://msdn.microsoft.com/en-us/library/dd997369.aspx), it's fully thread-safe:

<blockquote>ConcurrentDictionary<TKey, TValue> is designed for multithreaded scenarios. You do not have to use locks in your code to add or remove items from the collection.</blockquote>

Performance wise ConcurrentDictionary is about 50% slower (anecdotally) than the regular Dictionary type but even if this code is run very often, that is absolutely negligible compared to making just a single database access call.

Besides switching the Dictionary out with a ConcurrentDictionary, we also need to modify the init function since the ConcurrentDictionary way of adding items is slightly different:

```csharp
private static object syncRoot = new object();

private static void init(IPartnerConfig partnerConfig)
{
	// We only want one inside the init method at a time
	lock (syncRoot)
	{
		if (cache.ContainsKey(partnerConfig.PartnerID))
			return;

		var dict = new ConcurrentDictionary<SettingDescription, SettingDescriptionContainer>();

		... // Populate the dict variable with data from the database

		cache.AddOrUpdate(partnerConfig.PartnerID, dict, (k, ov) => dict);
	}
}
```

The syncRoot lock ensures that only one initialization is going on at the same time. While not necessary in regards of avoiding the race condition, this will avoid hitting the database multiple times if the init method is being called concurrently. This could be optimized in that there could be a syncRoot object per PartnerID to allow concurrently initializing the cache for each PartneriD. But, alas, I opt to keep it simple as the init method is only called once in the lifetime of the application.

Instead of just adding an item to the cache, we have to use the AddOrUpdate() signature that takes in the key, value and a lambda that returns a new value, in case the key already exists in the dictionary. In this case, no matter if the key exists or not, we want to set it to the new value, so the lambda just returns the same value as passed in the second parameter.
