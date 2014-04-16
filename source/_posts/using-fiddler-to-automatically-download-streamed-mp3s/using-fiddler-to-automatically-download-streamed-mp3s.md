permalink: using-fiddler-to-automatically-download-streamed-mp3s
title: Using Fiddler to Automatically Download Streamed MP3s
date: 2011-06-01
tags: [.NET]
---
Eric Lawrence's [Fiddler](http://www.fiddler2.com/fiddler2/version.asp) has many uses. I use it every day for debugging our client/server interaction, caching behavior, etc. What many don't realize is that Fiddler is also an excellent platform for scripting, enabling you to modify requests and responses as they go out and come back. I made a quick script to automatically download streamed MP3 files as they were played, naming them automatically from the ID3 information contained in them.

<!-- more -->

Before we get started, head on over and download the [FiddlerScript Editor](http://www.fiddler2.com/fiddler/fse.asp).

## Parsing ID3 tags

As I'm lazy, and most likely you are too, we'll use the excellent TagLib Sharp library for parsing the ID3 information in the downloaded MP3 files. You can get the latest version (2.0.4.0 as of this writing) [from here](http://download.banshee.fm/taglib-sharp/).

To gain access to the TagLib Sharp library from Fiddler, add a reference to it in the Fiddler Options dialog:

image_210.png

## Setting up the script

Now go to to Rules menu and click "Customize Rules…" to open the CustomRules.js file in the FiddlerScript Editor that we installed before.

image_44.png

Go to the OnBeforeResponse function and add the following bit of code to the end:

```csharp
if(oSession.url.Contains("SomeStream.aspx")) {
	var directory: String = "D:\Files\MP3";
	var path: String = System.IO.Path.Combine(directory, Guid.NewGuid() + ".mp3");

	oSession.SaveResponseBody(path);
	var file: TagLib.File = TagLib.File.Create(path);

	if(file.Tag.Title.Length > 0)
	{
		var target: String = System.IO.Path.Combine(directory, file.Tag.AlbumArtists + " - " + file.Tag.Title + ".mp3");

		if(!System.IO.File.Exists(target))
			System.IO.File.Move(path, target);
		else
			System.IO.File.Delete(path);
	}
}
```

The first line identifies the requests that are for MP3 files. Depending on where you're streaming from, you'll obviously need to change this line to match your specific requirements.

Once an MP3 response has been detected, we save the file using a GUID as the name. If TagLib Sharp detects a song title, the file is renamed in the "AlbumArtists – Title.mp3" form. If no title is present, we just let the file stay with the GUID name for manual renaming later on.

Save the CustomRules.js file and Fiddler will automatically pick up on the changes and start saving those precious MP3s!

## Disclaimer

Obviously the above code should only be used to save MP3 files from streams to which you own the rights.
