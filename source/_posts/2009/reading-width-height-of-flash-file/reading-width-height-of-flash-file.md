permalink: reading-width-height-of-flash-file
title: Reading Width & Height of Flash File
date: 2009-11-11
tags: [.NET, AS/Flex/Flash]
---
Obtaining the movie height &amp; weidth of a Flash file is an easy task using the [swfdump](http://www.swftools.org/swfdump.html) tool that comes as part of the [swftools](http://www.swftools.org/) package. Here's an example of how to invoke swfdump from C# and read out the height &amp; width of a given Flash file.

Start out by downloading on of the [swftools releases](http://www.swftools.org/download.html). I'm using the latest development snapshot. I'll be using one of the Flash files I made in [a previous blog post](http://www.improve.dk/blog/2008/06/11/as3-numbers-get-real) as a test file, but you can use any .swf file you want. The test Flash file is called *test.swf*.

Once you've extracted the swftools package and copied both the test file and the swfdump.exe file into your solution directory, we can now test it out manually:

```
D:\Webmentor Projekter\Blog\RetrievingSwfProperties>swfdump -X -Y test.swf
-X 500 -Y 375
```

By providing the -X and -Y switches swfdump only prints out the movie height &amp; width. You can see all the switches on the [swfdump man page](http://www.swftools.org/swfdump.html). At this point it's a simple matter of spinning up a swfdump process and parsing the output:

```csharp
static void Main(string[] args)
{
    // Set process properties
    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("swfdump.exe", "-X -Y test.swf");
    p.StartInfo.CreateNoWindow = true;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.UseShellExecute = false;
    p.Start();

    // Read all output, waiting for process to end
    string output = p.StandardOutput.ReadToEnd();

    // Regex that'll match both the width and height output - has to take care of potential decimals
    Match m = Regex.Match(output, @"-X (?d+(.d+)?) -Y (?d+(.d+)?)");

    // Convert width & height to doubles forcing en-US culture
    double width = Convert.ToDouble(m.Groups["width"].Value, new CultureInfo("en-US"));
    double height = Convert.ToDouble(m.Groups["height"].Value, new CultureInfo("en-US"));

    Console.WriteLine("Width: " + width);
    Console.WriteLine("Height: " + height);
    Console.Read();
}
```

Result:

<blockquote>Width: 500  
Height: 375</blockquote>
