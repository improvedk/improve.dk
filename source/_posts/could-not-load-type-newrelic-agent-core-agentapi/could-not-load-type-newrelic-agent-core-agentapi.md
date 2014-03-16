permalink: could-not-load-type-newrelic-agent-core-agentapi
title: Could Not Load Type 'NewRelic.Agent.Core.AgentApi'
date: 2013-05-23
tags: [.NET]
---
Recently I've begun using [New Relic](http://newrelic.com/), and so far it's been an excellent experience. About two weeks ago I started using their [.NET Agent API](https://newrelic.com/docs/dotnet/the-net-agent-api) to customize some of the data reported by our application to their servers. This makes the data way more valuable to us as we can now selectively ignore certain parts of our application while getting better reporting from other, more critical, parts of the application.

<!-- more -->

## Random Outages
Unfortunately, in the last couple of weeks, ever since introducing the .NET Agent API, we've had a number of outages (thankfully invisible to the customers due to a self-healing load-balancer setup shielding the individual application servers) where one of our applications servers would randomly start throwing the same exception on all requests:

```
System.TypeLoadException: Could not load type 'NewRelic.Agent.Core.AgentApi' from assembly 'NewRelic.Api.Agent, Version=2.5.112.0, Culture=neutral, PublicKeyToken=06552fced0b33d87'. 
at NewRelic.Api.Agent.NewRelic.SetTransactionName(String category, String name) 
at System.Web.HttpApplication.SyncEventExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute() 
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)
```

The error seemed to crop up randomly on all of our servers, though not at the same time and in with no predictable patterns - except it was always just after an application pool recycle. Once the error occurred it would continue happening until we either recycled the pool manually or it was recycled automatically according to its schedule.

## The Support Experience
To make a long story short, I opened a support case with New Relic as I couldn't find anything in neither their docs, nor on Google, related to the specific exception. After about a week of going back and forth between their engineers and me they managed to track down the root cause:

> It appears that some of the caching we do is not being correctly invalidated. I have removed the caching code and you should see this fix in our next release.

In the meantime I've had to stop using the .NET Agent API to avoid the issue from happening again. This doesn't mean we won't get any data; it's just not as well polished as before. I'm eagerly looking forward to the next agent release so we can get back to using the .NET Agent API again.

In conclusion I must say I'm impressed by the overall support experience. The responses have been quick and professional. Naturally I'd prefer not to have had any issues, but we all know they can happen, and in those cases it's a matter of having a solid triage process - and in this case I'm just happy to be able to assist in identifying the cause.
