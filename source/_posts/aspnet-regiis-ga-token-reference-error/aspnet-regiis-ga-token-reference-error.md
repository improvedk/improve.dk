permalink: aspnet-regiis-ga-token-reference-error
title: aspnet_regiis -ga Token Reference Error
date: 2008-09-02
tags: [IIS]
---
Some time ago Peter Loft Jensen wrote about how to easily give a user account the neccessary permissions to access the IIS metabase & required directories, and thus be used for running the IIS process.

<!-- more -->

We're running all x64 servers, but our IIS is running in 32 bit mode due to some non-x64 compatible 3rd party libraries. Usually this means we have to use the Frameworkversionaspnet_regiis.exe bin instead of the Framework64 version - otherwise it might interfere with our 32 bit IIS settings.

Doing that resulted in the following error:

```bash
C:\WINDOWS\microsoft.net\Framework\v2.0.50727>aspnet_regiis -ga [domain][user]
Start granting [domain][user] access to the IIS metabase and other directories used by ASP.NET.
An error has occurred: 0x800703f0 An attempt was made to reference a token that does not exist.
```

The solution was quite simple, it seems you must use the x64 version on an x64 system to run the -ga command. After using the binary in the Framework64 directory, the command ran perfectly.
