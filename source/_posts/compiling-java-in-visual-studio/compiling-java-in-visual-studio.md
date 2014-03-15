permalink: compiling-java-in-visual-studio
title: Compiling Java in Visual Studio
date: 2007-09-29
tags: [Visual Studio]
---
I often see my fellow comp. sci. students writing their (relatively) simple Java code in applications like Emacs, Nano or Eclipse. I'm not fond of either application. I much prefer Visual Studios text handling, solution overview, output windows and so forth. What most people don't know is that you can actually extend Visual Studio to a great extent. One way to extend Visual Studio is to write plugins using .NET, but there's a way that is much simpler (albeit also more limited). I will now show how you can make Visual Studio compile and run your Java applications all within Visual Studio itself.

<!-- more -->

First of all, you need to [download and install the Java JDK](http://java.sun.com/javase/downloads/index.jsp). Basically, you need to be able to call "javac" and "java" from any command prompt - which means you have to setup the environment settings so you have your JDK bin in the PATH variable.

javac_1_2.jpg

javac_2_2.jpg

Create a new Visual Studio project. It really doesn't matter much what type you choose as there is no native Java project types. Choosing J# will not give you any advantages over, say a C# project. In this example I'll use a C# Class Library project.

javac_3_2.jpg

Start out by deleting the automatically created Class1.cs file. Add a new text file instead, I'll call it MyApplication.java. You can write any standard Java code in the Java files, just like you'd ordinarily do.

javac_4_2.jpg

One of the really cool features of Visual Studio is that it actually includes Intellisense for a lot of the standard Java classes, so you're not left totally on your own.

javac_5_2.jpg

Now comes the compilation part. Add a new text file to the project and call it Compile.bat. This will be the bat file that manages the actual compilation and execution of the application afterwards. Leave the file empty for now, we'll enter the code in a short while.

javac_6_2.jpg

Go to Tools -> External Tools...

javac_7_2.jpg

Add a new entry called "Javac", set the command path to your Compile.bat file and make sure the directory is set to the ProjectDir macro path. Check the "Use Output window" checkbox, this ensures the output is output directly into the Visual Studio output window.

javac_8_2.jpg

Now enter the following into the Compile.bat file:

```java
del Output /S /Q
mkdir Output
javac *.java -d Output
cd Output
start java MyApplication
```

Modify the MyApplication.java file so it ends with a call to System.in.read(), this ensures the application will stay open after we start it.

```java
import java.io.*;

class MyApplication
{
	public static void main(String args[]) throws IOException
	{
		System.out.println("Hello World!");
		System.in.read();
	}
}
```

Now simply go to Tools -> Javac and watch your Java application compile and run.

javac_9_2.jpg

You can of course modify the build script in whatever way you wish to support larger applications. You could also use ANT build scripts, unit tests and so forth. To make compiling easier, you can create a key command (Tools -> Options -> Keyboard -> Tools.ExternalCommandX where X is the Javac commands index in the Tools menu) to the Javac command in the Visual Studio settings, I use Ctrl+Shift+J for Java compilation myself.
