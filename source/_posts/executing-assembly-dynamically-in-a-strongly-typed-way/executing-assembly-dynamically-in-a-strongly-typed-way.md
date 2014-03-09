permalink: executing-assembly-dynamically-in-a-strongly-typed-way
title: Executing Assembly Dynamically In a Strongly Typed Way
date: 2006-12-26
tags: [.NET]
---
During a recent plugin based project that I worked upon, I had to dynamically load assemblies and instantiate objects from those assemblies. Usually that'd require a lot of typecasting and nasty code, but thanks to generics we can make it a lot smoother.

<!-- more -->

I present to you my LoadPlugin function of my PluginHandler module:

```csharp
using System.Reflection;
using System;

namespace MTH.Core
{
	internal class PluginHandler
	{
		public static T LoadPlugin<T>(string file)
		{
			Assembly ass = Assembly.LoadFile(file);

			foreach (Type t in ass.GetTypes())
				if (typeof(T).IsAssignableFrom(t))
					return (T)ass.CreateInstance(t.FullName);

			return default(T);
		}
	}
}
```

The generic parameter defines the type that should be created from the specified assembly file, this also means that the function will return a strongly typed result of this specific type.

Inside the function we load the assembly and load through each type it hosts. For each type we check the IsAssignableFrom function which'll check if the current type is of the specified type T, no matter if T is an abstract class, interface or any other type. If it matches, we return a new instance of that type. If no type match is found we return null so we can handle the problem in the calling function.

Note that this is a very simple example function, there are many unhandled parameters! Security for instance. In most scenarios you would probably want to check the identity of the loaded assembly and set the security permissions dependingly. Also it is not possible to pass constructor parameters, this would require an extension of the function to take in an array of objects as constructor parameters.

This is how easily we can load any number of plugins that have been placed in a certain plugin directory (in this case the plugins contains code that is to be run on application startup):

```csharp
foreach (string file in Directory.GetFiles(Application.StartupPath + "\Plugins\Startup", "*.dll"))
{
	IRunnable startupPlugin = PluginHandler.LoadPlugin<IRunnable>(file);

	if (startupPlugin != null)
		startupPlugin.Run();
}
```

All that is required is that the assemblies in the PluginsStartup folder contain a type that implements the IRunnable interface:

```csharp

public interface IRunnable
{
	void Run();
}

```
