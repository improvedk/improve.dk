---
permalink: providing-custom-assembly-evidence
title: Providing Custom Assembly Evidence
date: 2008-06-13
tags: [.NET]
---
I recently [mentioned](/analyzing-assembly-evidence/) the possibility of having an assembly provide custom evidence alongside the CLR provided evidence. Let's see how to do it.

<!-- more -->

## Creating the evidence

The first step is to actually create the evidence itself. The evidence can be in any form, as long as it's serializable. That means you can use strings, complex types (provided they're serializable), or plain old'n'good XML. In lack of a better example, I'll create a piece of evidence that tells the birthdate and name of the developer behind the assembly. Really useful, I know.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<myEvidence>
	<birthDay>1985-07-25</birthDay>
	<name>Mark S. Rasmussen</name>
</myEvidence>
```

## Saving the evidence in binary form

To embed our XML as evidence, we first have to save it in binary form. To save a bit of typing, I use my SaveXmlAsBinaryEvidence function:

```csharp
/// <summary>
/// Saves the input XML in binary format that can be used when linking custom evidence to an assembly
/// </summary>
/// <param name="xml"></param>
/// <param name="outputFile"></param>
public static void SaveXmlAsBinaryEvidence(string xml, string outputFile)
{
	Evidence customEvidence = new Evidence();
	customEvidence.AddAssembly(xml);

	using (FileStream fs = new FileStream(outputFile, FileMode.Create))
	{
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs, customEvidence);
	}
}
```

This allows us to save out the evidence in binary format using a single line:

```csharp
EvidenceCreator.SaveXmlAsBinaryEvidence(File.ReadAllText("MyEvidence.xml"), "MyEvidence.evidence");
```

## Compiling the relevant assembly in netmodule form

I've created a simple assembly named TestAssembly that consists of a single file, MyClass:

```csharp
using System;

namespace TestAssembly
{
	public class MyClass
	{
		public void Test()
		{
			Console.Write("Hello world!");
		}
	}
}
```

If we compile it directly in Visual Studio, we end up with an assembly called TestAssembly.dll - this is not what we want. We need to compile the code into a .netmodule module file so we can manually link it together with our assembly into a finished assembly. Running the following command (with relevant path variables setup, or from the Visual Studio Command Prompt) will compile MyClass into a netmodule called MyClass.netmodule:

```csharp
csc /target:module MyClass.cs
```

You can actually load the .netmodule file into Reflector to verify the contents. It'll work nicely, though Reflector will give a warning since our netmodule does not contain an assembly manifest.

## Creating an assembly by linking the netmodule and evidence together

The final step is to link our evidence (MyEvidence.evidence) and netmodule (MyClass.netmodule) together. Make sure both files reside in the same directory (they don't have to, but it'll be easier). The following command will invoke AL.exe and do the final assembly linking:

```csharp
al /target:library /evidence:MyEvidence.evidence /out:MyClass.dll MyClass.netmodule
```

If you load up the resulting assembly in Reflector, you'll see the attached evidence under the Resources directory:

reflector_evidence_2.jpg

Now, if we [analyze the assembly evidence](/analyzing-assembly-evidence/) by loading the assembly and enumerating the assembly provided evidence like so:

```csharp
al /target:library /evidence:MyEvidence.evidence /out:MyClass.dll MyClass.netmodule
```

We see our custom evidence:

analyzing_custom_evidence_2.jpg

## Trustworthiness

[As mentioned earlier](/analyzing-assembly-evidence/), assembly provided evidence is inherently not trustworthy. There are however ways to secure it. We could use a public/private cryptosystem to sign the actual evidence. That way, anyone reading the evidence could verify the evidence providers signature and thus be sure that the entity linking the evidence into the assembly is trustworthy.
