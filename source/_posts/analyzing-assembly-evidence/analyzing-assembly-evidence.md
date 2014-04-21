---
permalink: analyzing-assembly-evidence
title: Analyzing Assembly Evidence
date: 2008-06-11
tags: [.NET]
---
When the CLR loads an assembly and needs to determine the appropriate permission set to apply, it's based on various evidence. Assembly evidence tells the CLR about the origins of the assembly, the zone it's loaded from and the file hash of the actual assembly file - these are just some of the more common evidence types the CLR uses, there are a lot more that are rarely used. Any object can be a piece of evidence, the CLR will only react on well known evidence types though.

<!-- more -->

There are two different overall origins of evidence, assembly provided and CLR provided. When the CLR loads an assembly, it recognizes the evidence based on the location the assembly is loaded from, file hash and so forth - this is the CLR provided evidence. The other type of evidence is custom evidence provided by the assembly itself. Although useful, we have to be careful not to blindly trust this evidence as it's provided by the assembly manufacturer. Thus an assembly manufacturer might provide a piece of evidence claiming that the assembly is provided by Microsoft / Google / some other trustworthy corporation - even though that might not be the case.

Any loaded assembly has an evidence property of type [System.Security.Policy.Evidence](http://msdn.microsoft.com/en-us/library/system.security.policy.evidence.aspx). The Evidence class has three relevant functions for obtaining the actual evidence, namely GetHostEnumerator, GetAssemblyEnumerator and GetEnumerator. GetHostEnumerator returns an IEnumerator containing the evidence policies, likewise the GetAssemblyEnumerator returns the assembly provided (and inherently untrustworthy) policies, while the GetEnumerator returns the union of the two.

Just a quick tip. I hate using enumerators, I much prefer foreach loops. The following method will take an IEnumerator and yield an IEnumerable, enabling you to foreach the collection:

```csharp
public static IEnumerable GetEnumerable(IEnumerator enumerator)
{
	while (enumerator.MoveNext())
		yield return enumerator.Current;
}
```

I've created a simple assembly containing an empty class (the contents of the assembly is not relevant in regards to evidence) called TestAssembly. The following code will load in the TestAssembly from the application directory and enumerate the CLR provided evidence. Note that I've got a special case for the[System.Security.Policy.Hash](http://msdn.microsoft.com/en-us/library/system.security.policy.hash(VS.80).aspx) type as it includes a rather lengthy hash of the file. The hash evidence can be used to validate the assembly contents against a pre-known hash of the assembly as a simple way of tamper protecting your applications assemblies.

```csharp
Assembly asm = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestAssembly.dll"));

foreach (object obj in EnumeratorHelper.GetEnumerable(asm.Evidence.GetHostEnumerator()))
	if (obj is Hash)
		Console.WriteLine(@"<System.Security.Policy.Hash version=""1""><RawData>4D5A900003...00000</RawData></System.Security.Policy.Hash>");
	else
		Console.WriteLine(obj.ToString());
```

This is the resulting output:

```xml
<System.Security.Policy.Zone version="1">
	<Zone>MyComputer</Zone>
</System.Security.Policy.Zone>

<System.Security.Policy.Url version="1">
	<Url>file:///D:/Webmentor Projekter/Security/AnalyzeEvidence2/bin/Debug/TestAssembly.DLL</Url>
</System.Security.Policy.Url>

<System.Security.Policy.Hash version="1">
	<RawData>4D5A900003...00000</RawData>
</System.Security.Policy.Hash>
```

As we can see, it gives us the URL from where the assembly was loaded, our security zone (MyComputer since it was loaded locally), and finally the dummy hash.

I've got a test server running called Mirage. Mirage is part of the Active Directory domain ipaper.lan and I've set a website up on it answering to the default address http://mirage.ipaper.lan. When loading the TestAssembly from this website, the evidence changes a bit:

```csharp
Assembly asm = Assembly.LoadFrom("http://mirage.ipaper.lan/TestAssembly.dll");
```

```xml
<System.Security.Policy.Zone version="1">
	<Zone>Internet</Zone>
</System.Security.Policy.Zone>

<System.Security.Policy.Url version="1">
	<Url>http://mirage.ipaper.lan/TestAssembly.dll</Url>
</System.Security.Policy.Url>

<System.Security.Policy.Site version="1">
	<Name>mirage.ipaper.lan</Name>
</System.Security.Policy.Site>

<System.Security.Policy.Hash version="1">
	<RawData>4D5A900003...00000</RawData>
</System.Security.Policy.Hash>
```

This time the SecurityZone is defined as Internet. Although my server is placed on my local network, it's known as the internet by my computer. This time we also get the Site policy since the assembly is loaded from a website, namely mirage.ipaper.lan. The security zone is actually provided by Windows (using the[IInternetSecurityManager::MapUrlToZone method](http://msdn.microsoft.com/en-us/library/ms537133(VS.85).aspx)) and not by the CLR itself, except in the case of locally loaded assemblies - in which it'll automatically set the zone to MyComputer. Thus, if I add mirage.ipaper.lan to my Internet Options control panel Intranet sites list, my SecurityZone changes to Intranet instead of Internet. Likewise I can add it to my trusted & untrusted sites list and modify the resulting SecurityZone.
