---
permalink: securing-dotnet-code-article
title: Securing .NET Code
date: 2006-10-02
tags: [.NET]
---
*When you write your code, compile it, and distribute the exe/dll's, is your source safe? We're not talking about protection against buffer overruns, SQL injection and various other code hacking techniques, we're talking protection of the source code itself, protection of intellectual properties.*

<!-- more -->

*This article is the result of me touring the danish universities as a [Microsoft Student Partner](http://www.microsoft.com/uk/academia/students/student-partners/default.mspx), giving lectures on the subject of securing code and intellectual properties in the realm of the .NET Framework.*

Download source code for the examples:  
[securing_dotnet.zip](/securing_dotnet.zip)

## The problem

Why even bother protecting our source code? There are millions of reasons for why we would want to protect our source code. Although security does not derive from obfuscation, but instead from writing secure code, there are a lot of situations where we simply cannot distribute our source code for various reasons.

You might be developing code that does not belong to you, due to you developing the code at your job.
It could be that you're developing an application that will be commercialised, it wouldn't be appropriate to release the source code as that would most likely lower your sales and open up all sorts of risks of counterfeited copies (I won't go into the topic of open source enterprises as they are vastly out of scope, but of course they do have their basis for existance).
It could also simply be a matter of security. As mentioned before, security shouldn't come from obfuscation, but any extra bit of unneccesary information that a potential hacker can get about your application will add up to the overall risk.
### Why is this an inherent problem with .NET?
To understand why this problem is especially explicit in .NET, one must understand how the .NET framework works. Let me introduce you to a simple illustration of the .NET framework levels:

framework_2.jpg

One of the coolest parts of the .NET framework is that we can write in more or less any language we want. C#, VB.NET, C++.NET, Delphi.NET, SmallTalk.NET, you name it! What enables us to do this is the Microsoft Intermediate Language (or MSIL in short). The MSIL compiler compiles your .NET language code into an intermediate language called MSIL. The MSIL language is common for all .NET languages, they all compile into this intermediate stage. Compared to native code, MSIL is a high level language. Although it is not easily read, it doesn't take long to get a grip of what's happening if you look at it. When an application (application being any .NET code, ASP.NET, DLL, EXE and so forth) is run, the Just-In-Time (or JIT in short) compiler takes over and compiles the MSIL code into native code that the CPU can run.

### Java, C++, Pascal etc

The MSIL language can easily be compared to the Java bytecode intermediate code. All Java code is compiled into Java bytecode which is then run by the Java Virtual Machine (or JVM in short). Although Java bytecode is not nearly as highlevel as MSIL, it is still easily parsable, making it easy to reverse engineer it back into real Java code. Although the reasoning behind the JVM and the MSIL/CLR compiler are not the same, Java has the same disadvantage as .NET. Other native languages like C++ and Pascal (and a lot others) do not have this problem. Instead of compiling into an intermediate language, they simply compile directly into native code. This has the advantage that code execution is usually faster and the disk footprint is smaller. Also while it is possible, it is unfeasible to reverse engineer large amounts of native code.

### So why not just dump the MSIL?

Having the MSIL/JIT combination has a lot of advantages, some of them being that the .NET framework can boast of a vast amount of supported languages and that the JIT compiler can make CPU/platform specific optimizations when it compiles from MSIL to native code. I won't be making any in depth analysis of the advantages and disadvantages of JIT compilation versus direct native compilation in this article, I'll leave that for a followup article later on.

## Tools

Alright, before we start, let me introduce you to a couple of tools that I will be using to demonstrate the weaknesses of the .NET application protection schemes that I will be showcasing.

### Reflector

You will have to learn to live with, love, hate and embrace Reflector, it's a godsend! [Click here to open Lutz Roeder's 'Programming .NET' website where you can download Reflector](http://www.aisto.com/roeder/dotnet/). Reflector is used to decompile existing .NET applications, whether that be EXE's or DLL's or any other .NET code. I won't be describing how to use Reflector, I will simply show the results of using Reflector.

### IL DASM

If you have the .NET Framework SDK installed then you will also have installed the Microsoft 'MSIL Disassembler' tool, called IL DASM in short. IL DASM is used to decompile .NET code into the underlying MSIL code. Reflector does the same, though Reflector can go a step further and reverse engineer the MSIL code into more humanly readable .NET code in the most commong .NET languages.

### Wireshark

[Wireshark, formerly known as Etheral](http://www.wireshark.org/) is the number one network sniffer for more or less any platform, and it's free!

### Dotfuscator

[Preemptive's Dotfuscator](http://www.preemptive.com/products/dotfuscator/index.html) is a great .NET code obfuscation tool. A lightweight community edition ships with Visual Studio .NET 2003 & 2005. The professional edition has a lot more options for code obfuscation and code compression.

I demoed the evaluation version of Dotfuscator during my MSP lectures. Just before my demo at the last university on the tour, I had an emergency. "Your evaluation copy has expired"... Not what you want to read when you are to show the demo in just under two hours. I got hold of the european office department by phone and within minutes I had a new evaluation license that I could use. Great support!

## Simple password protection

In this example we will be creating a simple .NET application that includes a "secret" algorithm that we want to protect, as well as protect the actual appliction by requiring a password before it can be used.

### Creating the application

Start out by creating a new Windows Application project, call it "SecureApplication". Add a TextBox and a Button to the form, Form1.cs like so:

1_2.jpg

```cs
private void button1_Click(object sender, EventArgs e)
{
	if (textBox1.Text == "password")
		MessageBox.Show(answerToAllLife().ToString());
	else
		MessageBox.Show("Error");
}
```

Now insert the following two functions into the form:

```cs
private void button1_Click(object sender, EventArgs e)
{
	if (textBox1.Text == "password")
		MessageBox.Show(answerToAllLife().ToString());
	else
		MessageBox.Show("Error");
}

string answerToAllLife()
{
	string result = "";

	result += (char)(Math.Pow(2, 6) + 8);

	for (int i = 1; i <= 100; i++)
		if (i == 100)
			result += (char)++i;

	result += (char)(117 - 9);

	result += result[result.Length - 1];

	result += typeof(List<string>).ToString().Split('.')[1][1];

	return result;
}
```

The answerToAllLife() method is the secret algorithm we want to secure. Never mind the complexity of it, is simply to make it a bit less readable when we decompile our application, to make things a bit more realistic. Now if you run the application, it will only allow you to run the algorithm if you provide the correct passowrd: 'password'. Simple and effective eh? Oh, you don't think that's protection enough?

### Decompiling the application

Now, let's see what happens when we open the application using the ILDASM tool that ships with every installation of the .NET Framework:

```cs
.method private hidebysig instance void  button1_Click(object sender, class [mscorlib]System.EventArgs e) cil managed
{
	// Code size       60 (0x3c)
	.maxstack  2
	.locals init ([0] bool CS$4$0000)
	IL_0000:  nop
	IL_0001:  ldarg.0
	IL_0002:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox SecureApplication.Form1::textBox1
	IL_0007:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
	IL_000c:  ldstr      "password"
	IL_0011:  call       bool [mscorlib]System.String::op_Equality(string, string)
	IL_0016:  ldc.i4.0
	IL_0017:  ceq
	IL_0019:  stloc.0
	IL_001a:  ldloc.0
	IL_001b:  brtrue.s   IL_0030
	IL_001d:  ldarg.0
	IL_001e:  call       instance string SecureApplication.Form1::answerToAllLife()
	IL_0023:  callvirt   instance string [mscorlib]System.Object::ToString()
	IL_0028:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [System.Windows.Forms]System.Windows.Forms.MessageBox::Show(string)
	IL_002d:  pop
	IL_002e:  br.s       IL_003b
	IL_0030:  ldstr      "Error"
	IL_0035:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [System.Windows.Forms]System.Windows.Forms.MessageBox::Show(string)
	IL_003a:  pop
	IL_003b:  ret
} // end of method Form1::button1_Click
```

I am by no means an MSIL expert, but we can pretty easily spot the strings "password" and "Error" and by putting two and two together, I wonder what the "password" might be? This is a crude example, but still, it shows the point, we cannot embed our password into the application.

Not only can we not include our password in our application, we also can't exchange the password with some kind of registration key pattern matching algorithm as that one could also be reverse engineered, opening up for the possibility of key generators and the likes.

But hey, this is the least of our problems. In case you didn't notice, the complete source code of our algorithm is also available!

```cs
.method private hidebysig instance string answerToAllLife() cil managed
{
	// Code size 205 (0xcd)
	.maxstack  5
	.locals init ([0] string result,
			[1] int32 i,
			[2] string CS$1$0000,
			[3] bool CS$4$0001,
			[4] char[] CS$0$0002)
	IL_0000:  nop
	IL_0001:  ldstr      ""
	IL_0006:  stloc.0
	IL_0007:  ldloc.0
	IL_0008:  ldc.r8     2.
	IL_0011:  ldc.r8     6.
	IL_001a:  call       float64 [mscorlib]System.Math::Pow(float64,
															float64)
	IL_001f:  ldc.r8     8.
	IL_0028:  add
	IL_0029:  conv.u2
	IL_002a:  box        [mscorlib]System.Char
	IL_002f:  call       string [mscorlib]System.String::Concat(object,
																object)
	IL_0034:  stloc.0
	IL_0035:  ldc.i4.1
	IL_0036:  stloc.1
	IL_0037:  br.s       IL_005b
	IL_0039:  ldloc.1
	IL_003a:  ldc.i4.s   100
	IL_003c:  ceq
	IL_003e:  ldc.i4.0
	IL_003f:  ceq
	IL_0041:  stloc.3
	IL_0042:  ldloc.3
	IL_0043:  brtrue.s   IL_0057
	IL_0045:  ldloc.0
	IL_0046:  ldloc.1
	IL_0047:  ldc.i4.1
	IL_0048:  add
	IL_0049:  dup
	IL_004a:  stloc.1
	IL_004b:  conv.u2
	IL_004c:  box        [mscorlib]System.Char
	IL_0051:  call       string [mscorlib]System.String::Concat(object,
																object)
	IL_0056:  stloc.0
	IL_0057:  ldloc.1
	IL_0058:  ldc.i4.1
	IL_0059:  add
	IL_005a:  stloc.1
	IL_005b:  ldloc.1
	IL_005c:  ldc.i4.s   100
	IL_005e:  cgt
	IL_0060:  ldc.i4.0
	IL_0061:  ceq
	IL_0063:  stloc.3
	IL_0064:  ldloc.3
	IL_0065:  brtrue.s   IL_0039
	IL_0067:  ldloc.0
	IL_0068:  ldc.i4.s   108
	IL_006a:  box        [mscorlib]System.Char
	IL_006f:  call       string [mscorlib]System.String::Concat(object,
																object)
	IL_0074:  stloc.0
	IL_0075:  ldloc.0
	IL_0076:  ldloc.0
	IL_0077:  ldloc.0
	IL_0078:  callvirt   instance int32 [mscorlib]System.String::get_Length()
	IL_007d:  ldc.i4.1
	IL_007e:  sub
	IL_007f:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
	IL_0084:  box        [mscorlib]System.Char
	IL_0089:  call       string [mscorlib]System.String::Concat(object,
																object)
	IL_008e:  stloc.0
	IL_008f:  ldloc.0
	IL_0090:  ldtoken    class [mscorlib]System.Collections.Generic.List`1<string>
	IL_0095:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
	IL_009a:  callvirt   instance string [mscorlib]System.Object::ToString()
	IL_009f:  ldc.i4.1
	IL_00a0:  newarr     [mscorlib]System.Char
	IL_00a5:  stloc.s    CS$0$0002
	IL_00a7:  ldloc.s    CS$0$0002
	IL_00a9:  ldc.i4.0
	IL_00aa:  ldc.i4.s   46
	IL_00ac:  stelem.i2
	IL_00ad:  ldloc.s    CS$0$0002
	IL_00af:  callvirt   instance string[] [mscorlib]System.String::Split(char[])
	IL_00b4:  ldc.i4.1
	IL_00b5:  ldelem.ref
	IL_00b6:  ldc.i4.1
	IL_00b7:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
	IL_00bc:  box        [mscorlib]System.Char
	IL_00c1:  call       string [mscorlib]System.String::Concat(object,
																object)
	IL_00c6:  stloc.0
	IL_00c7:  ldloc.0
	IL_00c8:  stloc.2
	IL_00c9:  br.s       IL_00cb
	IL_00cb:  ldloc.2
	IL_00cc:  ret
} // end of method Form1::answerToAllLife
```

Now this code isn't especially readable when it get's large, but what if we try using Reflector instead? This is what we get when we decompile our buttons Click event:

```cs
private void button1_Click(object sender, EventArgs e)
{
	  if (this.textBox1.Text == "password")
	  {
			MessageBox.Show(this.answerToAllLife().ToString());
	  }
	  else
	  {
			MessageBox.Show("Error");
	  }
}
```

Looks familiar? This should make it pretty obvious that our code is NOT safe, it is simply too easy to decompile unprotected .NET code. So what can we do?

## Using native code to protect vital parts

I gave a comparison between .NET and the native languages earlier on. What if we were to create our vital code parts in native code, and then keep all the non-vital code as .NET? Let's try to protect our algorithm by coding that one in native code while presevering the user interface in .NET. Of course we ought to also code the password check in native code, but for this example I'll stick with our algorithm.

### Creating a VC++ DLL and calling it from .NET

Create a new VC++ Win32 Project to the solution, call it "SecureComponent". When prompted, select "DLL" as application type and make sure that you check the "Export symbols" checkbox.

Now replace the whole SecureComponent.cpp file with the following code:

```c
#include "stdafx.h"
#include "SecureComponent.h"

extern "C" SECURECOMPONENT_API char* AnswerToAllLife(void)
{
	return "Hello";
}
```

And replace the corresponding header file SecureComponent.h with the following code:

```c
#ifdef SECURECOMPONENT_EXPORTS
	#define SECURECOMPONENT_API __declspec(dllexport)
#else
	#define SECURECOMPONENT_API __declspec(dllimport)
#endif

extern "C" SECURECOMPONENT_API char* AnswerToAllLife(void);
```

<p>I am by no means a C++ expert, and you need not be either. This C++ DLL project contains a single function, AnswerToAllLife() of type char* which corresponds to a .NET string. Instead of writing the actual algorithm we'll suffice with returning the alogrithm result, "Hello".

Compile the SecureComponent project and locate the resulting SecureComponent.dll file in the output directory. We will be calling this DLL through platform invoke (p/invoke in short) as that is the simplest and quickest way to call our native code from .NET.

Copy the SecureComponent.dll file into the %windir%system32 directory. Now modify the SecureApplication form1.cs code so it matches the following:

```cs
[DllImport("SecureComponent.dll")]
private static extern string AnswerToAllLife();

public Form1()
{
	InitializeComponent();
}

private void button1_Click(object sender, EventArgs e)
{
	if (textBox1.Text == "password")
	{
		MessageBox.Show(AnswerToAllLife());
	}
	else
		MessageBox.Show("Error!");
}
```

Make sure that you import the correct namespace (System.Runtime.InteropServices) for the DllImport attribute to work.

Now try running the SecureApplication project and test that it works (messagebox saying "Hello" when you provide the correct password and click the button).

Now comes the interesting part. Open up Reflector and add the SecureApplication.exe file to the list of loaded assemblies (make sure you remove it if it's already loaded as Reflector will otherwise show an old cached version). Browse to the AnswerToAllLife() function est voilá:

disassembly1_2.gif

Now our algorithm is securely hidden from the usual means of decompilation. This however does not mean that our code is universally secure. Sure it's somwhat more difficult to get to our algorithm than before, but it ain't possible. Native code can be reverse engineered but it's a very tough job, especially as the code get's larger and more complicated.

A drawback with this method is that we'll have to write our vital code parts in native code. Naturally we'd prefer to write in managed code, but depending on the project, native code is an opportunity. So what am I saying? No matter what you do, if your code is physically at the clients machine, it is not safe. No matter what you do. As long as the code is physically at the client, all we can do is to make it as cumbersome as possible to get to the original source as possible, hopefully evading most reverse engineering attempts.

## Creating a webservice that will host our algorithm

Let's make our code 100% secure. How? By not supplying the clients with our algorithm at all, but simply allowing them to invoke the algorithm remotely. The easiest way to do this in the .NET framework is by using web services.

### Creating the web service

Add a new ASP.NET Web Service to the solution, call it "AlgorithmService". Now open the Service.asmx codebehind file and replace the contents with this code:

```cs
using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Service : System.Web.Services.WebService
{
	[WebMethod]
	public string AnswerToAllLife()
	{
		return "Hello";
	}
}
```

Our webservice contains a single function, AnswerToAllLife that represents our algorithm, just like our C++ version. Try running the webservice (you may need to set the Service.asmx file as the start page) and check that the web service works by invoking the AnswerToAllLife function:

servicetest_2.gif

Now add a web reference to the SecureApplication project. Click the "Web services in this solution" link and select the AlgorithmService web service. Name it "AlgorithmService" and click the "Add reference" button.

Now modify the button1_click method as follows:

```cs
private void button1_Click(object sender, EventArgs e)
{
	if (textBox1.Text == "password")
	{
		AlgorithmService.Service algorithm =
			new SecureApplication.AlgorithmService.Service();

		MessageBox.Show(algorithm.AnswerToAllLife());
	}
	else
		MessageBox.Show("Error!");
}
```

Now, when the user provides the correct password, we invoke the AlgorithmService web service and show the result. If you decompile the SecureApplication exe file using Reflector, this is the code you'll see for our button's click event:

```cs
private void button1_Click(object sender, EventArgs e)
{
	  if (this.textBox1.Text == "password")
	  {
			MessageBox.Show(new Service().AnswerToAllLife());
	  }
	  else
	  {
			MessageBox.Show("Error!");
	  }
}
```

And this is the code we see if we go to the "AnswerToAllLife" method in the Service class:

```cs
[SoapDocumentMethod("http://tempuri.org/AnswerToAllLife",
	RequestNamespace="http://tempuri.org/",
	ResponseNamespace="http://tempuri.org/",
	Use=SoapBindingUse.Literal,
	ParameterStyle=SoapParameterStyle.Wrapped)]
public string AnswerToAllLife()
{
	  object[] objArray1 = base.Invoke("AnswerToAllLife", new object[0]);
	  return (string) objArray1[0];
}
```

Neat eh? Now there is no way that our source code can be compromised. There are however some drawbacks to using web services. First of all, we'll need to require that our clients are connected to the internet, or at least to some kind of network that'll allow access to our web service. Also web services impose a significant overhead on each algorithm call compared to direct managed/native calls. Finally there may also be security issues, for instance, a credit card validation service might not be the best of projects to provide via a web service.

## Recap

* Without any protection at all, our code is very unsafe. Any person is able to decompile a .NET application using only free tools within minutes.
* We may protect our code by writing it in a native language. Though it is not bulletproof, it severely complicates the process of reverse engineering our code. It is however also morecumbersome for us to write our vital code in the native language as oposed to writing it in managed code.
* The only 100% secure way to protect our code is to not supply it to our clients in any way. Web services and remoting are two ideal methods of accomplishing this separation. It isn't always possible separate parts of our code due to certain circumstances, so it'll be a matter of judgement whether it's possible.


## Obfuscation

Ok, so we can remove our algorithm code by utilizing web services, but what about the password check? And what about our other code that we'd also prefer to keep to ourselves? Let's look at obfuscation.


Obfuscation is a technique used to complicate code. Obfuscation makes code harder to understand when it is decompiled, but it typically has no affect on the functionality of the code.

### PreEmptive Dotfuscator

I have tested several tools that boast of being able to obfuscate .NET code without providing a lot of overhead on the developers... Us. The most effective and efficient obfuscation tool that I have tested so far is PreEmptive's Dotfuscator. You can request a free 14 day evaluation at their website. Don't worry if you haven't gotten it up and running while following this article, I'll be showing screenshots of the most important aspects of using the Dotfuscator product.

### Seamless Visual Studio integration

What's cool with Dotfuscator is that it integrates directly into Visual Studio (2003 and 2005) by providing a new project type:

dotfuscatorproject_2.gif

After creating the project, we can add any number of assemblies to obfuscate, or even project outputs - Dotfuscator will then make sure to obfuscate the resulting assemblies automatically.

projectoutput_2.gif

We can set the individual obfuscation settings by using the six setting categories. I won't go into major details as that is out of scope of this article.

renaming_2.gif

Under the renaming category we can set the options for how our assembly members should be renamed. We can choose to rename using numeric names, lower/uppercase alpha names, or my favorite - the unprintable characters. Although we can rename our members to our liking, there are still certain restrictions. In .NET every function must have a unique signature. Normally the signature consists of the name of the function as well as the parameter types. Dotfuscator can use the "Enhanced overload induction" to also identify functions by their return type, effectively causing us to give a lot more functions the same names, confusing our potential decompilers even more.

We can also choose to alter the control flow, causing a number of lables and goto calls to be embedded in our code, I haven't observed any noticable performance hits. Dotfuscator also supports automatic encryption of strings in our application without us having to do anything. You can choose which strings encrypt and which to keep unencrypted. The strings are decrypted on-the-fly as the application is running, so keeping unimportant strings in cleartext will be the most efficient.

### The process of obfuscating

Obfuscating our SecureApplication is very easy. Simply add the SecureApplication project output to the "Input Assemblies" part of our Dotfuscator project and compile the project as we would compile any other project. After compiling, try to open the project in Reflector:

obfuscatedreflector_2.gif

Notice how our namespace and class structure is completely unreadable, thanks to the unprintable characters. We can still find our buttons click method by looking at the method signature - as our methods still have to have a unique signature. We can identify our buttons click event by looking for a method with the same signature: void(object, EventArgs).

IL DASM can still decompile our application, this is the resulting code from out buttons click method:

```cs
.method private hidebysig instance void  'eval_?'(object A_0, class [mscorlib]System.EventArgs A_1) cil managed
{
	// Code size       171 (0xab)
	.maxstack  3
	.locals init ([0] class 'eval_?' 'algorithm',
		   [1] bool CS$4$0000,
		   [2] int32 EGSwitchVar,
		   [3] int32 V_3)
	IL_0000:  ldc.i4     0x6
	IL_0005:  stloc      V_3
	IL_0009:  br.s       IL_0024
	IL_000b:  ldloc      EGSwitchVar
	IL_000f:  switch     ( 
						IL_0060,
						IL_0052,
						IL_00a8,
						IL_0081)
	IL_0024:  nop
	IL_0025:  ldarg.0
	IL_0026:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox eval_a::'eval_?'
	IL_002b:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
	IL_0030:  ldstr      bytearray (14 65 16 76 18 6A 1A 68 1C 6A 1E 70 20 53 22 47 ) // .e.v.j.h.j.p S"G
	IL_0035:  ldloc      V_3
	IL_0039:  call       string a$PST06000001(string,
											int32)
	IL_003e:  call       bool [mscorlib]System.String::op_Equality(string,
																 string)
	IL_0043:  ldc.i4.0
	IL_0044:  ceq
	IL_0046:  stloc.1
	IL_0047:  ldc.i4     0x1
	IL_004c:  stloc      EGSwitchVar
	IL_0050:  br.s       IL_000b
	IL_0052:  ldloc.1
	IL_0053:  brtrue.s   IL_0062
	IL_0055:  ldc.i4     0x0
	IL_005a:  stloc      EGSwitchVar
	IL_005e:  br.s       IL_000b
	IL_0060:  br.s       IL_0083
	IL_0062:  ldstr      bytearray (14 50 16 65 18 6B 1A 74 1C 6F 1E 3E )             // .P.e.k.t.o.>
	IL_0067:  ldloc      V_3
	IL_006b:  call       string a$PST06000001(string,
											int32)
	IL_0070:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [System.Windows.Forms]System.Windows.Forms.MessageBox::Show(string)
	IL_0075:  pop
	IL_0076:  ldc.i4     0x3
	IL_007b:  stloc      EGSwitchVar
	IL_007f:  br.s       IL_000b
	IL_0081:  br.s       IL_00aa
	IL_0083:  nop
	IL_0084:  newobj     instance void 'eval_?'::.ctor()
	IL_0089:  stloc.0
	IL_008a:  ldloc.0
	IL_008b:  callvirt   instance string 'eval_?'::'eval_?'()
	IL_0090:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [System.Windows.Forms]System.Windows.Forms.MessageBox::Show(string)
	IL_0095:  pop
	IL_0096:  nop
	IL_0097:  br.s       IL_009a
	IL_0099:  break
	IL_009a:  ldc.i4     0x2
	IL_009f:  stloc      EGSwitchVar
	IL_00a3:  br         IL_000b
	IL_00a8:  br.s       IL_00aa
	IL_00aa:  ret
} // end of method eval_a::'eval_?'
```

Notice how our "password" string now looks like this: bytearray (14 65 16 76 18 6A 1A 68 1C 6A 1E 70 20 53 22 47 ), neat eh?

## Checking our password through a web service

Although our password is encrypted, it still isn't safe to have the password check on the client machine. Why don't we check the password via a web service? Again, this is only possible if we can require a network connection to our web service. If that isn't possible, we might want to code our password check in a native language.

### Creating our password checking web service

Add a new ASP.NET web service project to our solution, call it "PasswordService". Modify the Service.asmx.cs file so it contains the following code:

```cs
using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Service : System.Web.Services.WebService
{
	[WebMethod]
	public bool ValidPassword(string password)
	{
		return password == "password";
	}
}
```

Our password checking web service only contains a single method, ValidPassword. ValidPassword takes a single parameter, the password to check. It returns true if the password is valid, and false otherwise. Now, this time we'll publish our web service to an actual web server as the local hosting won't be sufficient for the next section of this article. If you don't have access to a web server yourself, you can use my web password checking web service running at [http://service.improve.dk/Service.asmx](http://service.improve.dk/Service.asmx).

Add a web reference to the SecureApplication project, remember to use the location "[http://service.improve.dk/Service.asmx"](http://service.improve.dk/Service.asmx&quot;) this time. Call the service "PasswordService" and click "Add reference". Now modify our buttons click event so it matches the following:

```cs
private void button1_Click(object sender, EventArgs e)
{
	PasswordService.Service password =
		new SecureApplication.PasswordService.Service();

	if (password.ValidPassword(textBox1.Text))
	{
		AlgorithmService.Service algorithm = 
			new SecureApplication.AlgorithmService.Service();

		MessageBox.Show(algorithm.AnswerToAllLife());
	}
	else
		MessageBox.Show("Error!");
}
```

Notice how we this time use our PasswordService to check our password while still using our AlgorithmService to run the algorithm. So what's the advantage? Both our password checking code as well as our algorithm code is 100% secured this time. Combine this with the obfuscation and you've got some really tough to read MSIL code.

## Cracking the password check

Now that our code is totally safe, how can we get around the password check? Let's ignore the fact that it is possible to decompile our application to MSIL code, remove the password checking parts and then reconnect the bits to create our complete application, except the password check. While this is possible, it's more or less unfeasible for a major project, especially if you've obfuscated it effectively. Let's change the scenario.

Now we're the hacker who's trying to beat our password check. We do not have access to the code of our SecureApplication, but we do have the distributed exe file (that utilizes our two previous web services).

### Analyzing the SecureApplication

We want to know what happens under the hood of the SecureApplication that we've got. Fire up Wireshark and start a new capture.

wiresharksettings_2.gif

Be aware that if you're using a WIFI, you may have to turn off promiscuous mode. We'll cheat a tiny bit, since we know the application uses webservices, let's setup a filter so we only see TCP HTTP (port 80) traffic, this'll reduce a lot of unwanted traffic.

Now start the capture, run the SecureApplication and type in a false password (remember, we're the hacker, we haven't got the correct password). Close down the SecureApplication and stop the capture, hopefully you'll see a result like this:

wiresharkresults_2.gif

Right click on one of the green lines and select "Follow TCP stream", you should see a stream like this:

```
POST /service.asmx HTTP/1.1
User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client
														Protocol 2.0.50727.42)
Content-Type: text/xml; charset=utf-8
SOAPAction: "http://tempuri.org/ValidPassword"
Host: service.improve.dk
Content-Length: 329
Connection: Keep-Alive

<!--?xml version="1.0" encoding="utf-8"?-->
<soap:envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	<soap:body>
		<validpassword xmlns="http://tempuri.org/">
			<password>asd</password>
		</validpassword>
	</soap:body>
</soap:envelope>

HTTP/1.1 200 OK
Date: Thu, 26 Oct 2006 22:08:34 GMT
Server: Microsoft-IIS/6.0
X-Powered-By: ASP.NET
X-AspNet-Version: 2.0.50727
Cache-Control: private, max-age=0
Content-Type: text/xml; charset=utf-8
Content-Length: 369

<!--?xml version="1.0" encoding="utf-8"?-->
<soap:envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	<soap:body>
		<validpasswordresponse xmlns="http://tempuri.org/">
			<validpasswordresult>false</validpasswordresult>
		</validpasswordresponse>
	</soap:body>
</soap:envelope>
```

Analyzing the contained HTTP request and response gives us som valuable information. The top part is the POST request and the bottom is the response.

In the request we can gather that the host is "service.improve.dk" and that we're requesting the file "/service.asmx". The SOAP XML tells us that a function called ValidPassword is being called, and it expects the parameter "password". Heck, we just found out the structure of the password checking web service. Now, what can we gather from the response?

Well, I wonder what <ValidPasswordResult>false</ValidPasswordResult> means... If only we somehow could switch that "false" to a true.

### Faking the web service

Our goal here is to setup our own web service and somehow make the SecureApplication call our own service instead of the real one. We cannot change the code of the SecureApplication, so we'll have to work around it somehow. Start by adding a new ASP.NET web service to our solution, call it "FakePassword". Modify the Service.asmx.cs file so it matches the following code:

```cs
using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class Service : System.Web.Services.WebService
{
	[WebMethod]
	public bool ValidPassword(string password)
	{
		return true;
	}
}
```

From the HTTP stream we gathered the function name, type and parameter(s). In our fake service we'll just return "true" everytime, effectively bypassing the password check by ignoring the password parameter. Now open up your IIS and set the default website home directory to the folder where your fake password service lies.

iis_2.gif

Now if you go to [http://localhost/Service.asmx](http://localhost/Service.asmx), you should see our fake password service. What we need now is for our service to somehow respond to the address [http://service.improve.dk/Service.asmx](http://service.improve.dk/Service.asmx). Open up the file "hosts" in the following directory: %windir%system32driversetchosts, note that it does not have an extension, simply open it in notepad. Now add the following two lines:

```cs
127.0.0.1		service.improve.dk
127.0.0.1		http://service.improve.dk
```

You have to ensure that you keep the tabs just like the default line in the hosts file. Now if you go to [http://service.improve.dk/Service.asmx](http://service.improve.dk/Service.asmx), you'll no longer get to the real web service as we've just setup our system so that 127.0.0.1 (our local machine) responds to this domain name instead. You can't test the webservice locally through this address as it will fail if you don't use the proper .NET classes. Now think about what happens when we run our SecureApplication again... It will connect to the address it was built to connect to, [http://service.improve.dk](http://service.improve.dk), but it's no longer the original web service, this is now our fake service. This should be the result:

hello_2.gif

No matter what password we supply, our algorithm will be run as our fake web service will tell the application that our password is valid.

## Recap


* Obfuscation vastly improves the safety of our application since strings and member names are no longer visible. Also it forces people to look at the MSIL code or break the obfuscation before they have access to our code. Obfuscation isn't safety in itself, but combined with other methods of protection, it's a big plus.
* We really shouldn't have any registration methods lying in the client application as they can and eventually will be cracked, resulting in either keygens or password floating around on the internet. If possible, use remote checking of registration codes.
* Don't put a lot of trust in your solution just because you use remote checking, it can be broken.


## A proposed solution

My proposed solution is a mix of the earlier mentioned methods, combined together it is rather effective. It does however require a connection to our remote registration server.

For this demo I won't be creating any code as it gets rather complex. Instead I will be describing the overall thought behind the solution. I am considering making a general purpose registration framework that will encompass my proposed solution. When I do make this framework, I will surely be releasing an article describing it in more detail.

### Public key cryptography

The solution is built upon a publick key cryptosystem, in my implemented solution I've used the RSA cryptosystem, but which specific system is used really isn't important.

Public key cryptosystems are built upon the idea that every person get's a set of keys, a public key that anyone knows about as well as a secret (normally the word private is used, by to avoid confusion due to both public and private starting with a "p", I'll use the term secret for the private key) key. The secret key is to be kept totally secret from everybody else, only you should know the contents of that one.

The correlation between the keys is that any data encrypted with a public key can only be decrypted by using the matching secret key. Likewise, any data encrypted using the secret key can only be decrypted using the matching public key. This allows us for two very important scenarios of cryptography.

Say I want to send a secret message to Alice, I'll use Alice's public key to encrypt my message. The only way to decrypt the message is to use Alice's secret key, and only Alice knows that one, so only Alice can read my message. Now, Alice can't positively know who the message comes from, that is, unless I sign the message. Signing the message implies that I use my secret key to encrypt the already encrypted data (encrypted with Alice's public key as before) with my secret key. Now only my public key can decrypt the data, hence the message can only come from me if it's decryptable by using my public key.

Using these correlations it's possible for me to send a message to Alice that only Alice is able to read. Furthermore I can sign my message so that Alice can verify it is coming from me.

### Customer entity

First of all we have the Customer. The Customer consists of the customer data itself, not really important to this solution. Besides the trivial customer data, the customer has a Keyset.

customerentity_2.gif

### Keyset

The Keyset contains three keys (keys of the RSA cryptosystem):


* The customers public key.
* The customers secret key.
* The registration servers public key.


keysetentity_2.gif

The reason for the customer to have to registration servers public key is so that we can verify the messages being received from the registration server.

### The registration server

The registration server can be set up in any way you like. I've personally used an ASP.NET Webservice, but you could use remoting or any other means of communication. The registration server only has two functions, GetKeyset and GetLicense.

keyserverentity_2.gif

### Step 1

When the customer starts up the application for the first time, the GetKeyset function is invoked on the registration server. The GetKeyset function takes the users registration code as a parameter and returns the proper Keyset, provided that the registration code is valid. If a registration code at some point is misused, shared or in any other way has been used in an illegal way, simply inactivate the registration code on the registration server, ensuring that no new keysets will be returned for that registration code in the future.

step1_2.gif

### Step 2

After the keyset has been retrieved, we now have the means to decipher any licenses we retrieve from the registration server.

The next step is to invoke the GetLicense method, again passing in our registration code as a parameter. The license is in its most basic form simply a boolean value, true/false, telling us whether we're allowed to run the application or not. The important part here is that we first encrypt the license result using customers public key, then we encrypt/sign that data with the registration servers secret key.

step2_2.gif

### Step 3

Now that we have retrieved the license, all we need to do is to decipher the result and then verify that the result originates from the correct registration server. We do this by first deciphering the data using the customers secret key. Then we decipher that data using the registration servers public key. If the data is valid, we know it comes from the correct registration server.

step3_2.gif

### The FCKGW effect

One of the most common problems with registration protection of software is that users tend to share registration keys. As mentioned before, we can mark registration keys as invalid on the registration server. Marking keys as invalid will cause any new keyset requests for that specific key to be denied. But what if a thousand people had already requested - and successfully retrieved - keysets? Of course there could be implemented an automatic check ensuring a max number of keyset requests for a single registration key, IP protection and so forth. What we really want, is to be able to invalidate a keyset/license that a user has already retrieved.

A way to do this is to impose a time restriction on the licenses that a customer retrieves from the registration server. For instance, if a license was automatically invalidated on the client machine after a week, then the customer would have to retrieve a new license from the registration server. If the registration had been invalidated on the server in the meantime, the license wouldn't be able to update, and thus the client wouldn't be allowed to run the application any longer.

## Ending remarks

Deciding whether to protect your code, and to what degree, is not a simple task. You must carefully weigh the pros and cons of the various techniques, and to what degree you need the protection.

Always remember that there is no totally safe way to protect your code, unless you keep it away from the client computer.

Please feel free to post any comments you may have on my article, I'll appreciate any feedback you give.
