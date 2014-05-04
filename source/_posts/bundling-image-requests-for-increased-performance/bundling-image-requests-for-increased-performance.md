---
permalink: bundling-image-requests-for-increased-performance
title: Bundling Image Requests for Increased Performance
date: 2010-06-07
tags: [.NET, AS/Flex/Flash]
---
A common scenario in RIA's is to show a large amount of small pictures on a single page. Let's say we want to show 100 images in a grid. While the simplest approach is to just put in 100 image objects and load in the images one by one, I believe it can be done smarter...

<!-- more -->

## The cost of a request

Each and every request will have a header overhead of about ~400 bytes outgoing and ~200 bytes ingoing - both varying depending on the host, cookies, headers etc. Multiply that by 100 requests and we've got about 60KB of data overhead, just for the headers. Even worse is the actual roundtrip time of sending the packets to the server and getting a reply back; Even with reuse of the connections, there's a large cost involved.

## Bundling requests

Imagine if we could just make a single request to the server - "Hey, please send me these 100 images, ty" - and then we'll get back a single response containing all the images. One way of doing this would be to zip the images on the server and then unzip them on the client - there's open zip libraries for both Silverlight and Flash. However, this has a large CPU cost on not only the server, but also on the client. Furthermore, images usually don't compress much so it's basically just a waste. In this post I'll present a C# class for bundling images as well as an AS3 class for reading the bundled image stream. While the RIA sample is in Actionscript, it's easily applicable to Silverlight as well - should anyone feel like implementing the client side in Silverlight, please let me know so I can link you.

## Generating sample images

Our first task is to generate some sample images. The following code will create 100 images named 1-100.jpg containing the greytones from #000000 (well, almost) to #FFFFFF.

```cs
for(int i=1; i<=100; i++)
{
    int rgb = Convert.ToInt32(i / 100d * 255);

    using(Bitmap bmp = new Bitmap(100, 100))
    using(Graphics g = Graphics.FromImage(bmp))
    {
        g.FillRectangle(new SolidBrush(Color.FromArgb(rgb, rgb, rgb)), 0, 0, 100, 100);
        bmp.Save(i + ".jpg");
    }
}
```

## On the server side: ImageStream.cs

The ImageStream class contains a dictionary that'll hold references to the files untill we're ready to write them out. Each added file consists of a key as well as a filepath. To keep things simple, I'm limiting the key names to ASCII codes between 32 and 126 to avoid unprintable characters.

The class has a Write method that'll write all the added images to the provided stream. Each image consists of four parts:


* 4 bytes (int) that contains the combined length of the key and payload plus two extra bytes for specifying the key length.
* 2 bytes (short) that contains the key length.
* X bytes containing the key using UTF8Encoding. I'll explain later why I'm using UTF8Encoding and not ASCIIEncoding.
* X bytes containing the actual file contents.


### ImageStream.cs

```cs
public class ImageStream
{
    IDictionary files = new Dictionary();

    public void AddFile(string key, string filePath)
    {
        files.Add(key, new FileInfo(filePath));

        if (key.ToCharArray().Any(x => x < 32 || x > 126))
            throw new ArgumentException("Invalid character used in key.");
    }

    public void Write(Stream stream)
    {
        // For each file, write the contents
        foreach (var file in files)
        {
            // Write payload length
            stream.Write(BitConverter.GetBytes((int)file.Value.Length + file.Key.Length), 0, 4);

            // Write key length
            stream.Write(BitConverter.GetBytes((ushort)file.Key.Length), 0, 2);

            // Write key
            stream.Write(Encoding.UTF8.GetBytes(file.Key), 0, file.Key.Length);

            // Write file
            stream.Write(File.ReadAllBytes(file.Value.FullName), 0, (int)file.Value.Length);
        }
    }
}
```

## On the server side: Image.ashx

All we need now is a file to serve the ImageStream. I'm using an HttpHandler called Image.ashx to loop through all the files (located in "/Imgs/") and add them to the ImageStream before writing them out to the output stream.

### Image.ashx

```cs
public class Image : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/octet-stream";
        context.Response.Buffer = false;

        var imgc = new ImageStream();

        for(int i=1; i<=100; i++)
            imgc.AddFile("img" + i, context.Server.MapPath("Imgs/" + i + ".jpg"));

        imgc.Write(context.Response.OutputStream);
    }

    public bool IsReusable
    {
        get { return true; }
    }
}
```

## On the client side: CombinedFileReader.as

The CombinedFileReader class takes a url in the constructor, pointing to the stream we want to retrieve. Once we call load() we spawn a URLStream and listen for the PROGRESS and COMPLETE events. The core of the class is the onProgress method, being invoked on both PROGRESS and COMPLETE events. We don't really care which event it is as both means there's new data for us to consume.

The onProgress method works as a simple state machine. This could be done much cleaner by abstracting away the state functionality, but it's simple enough to be easily understood. There are just two states we can be in:

### header

In this state we're currently waiting for there to be 4 bytes available, meaning we can read the first integer containing the number of bytes required to load the current file. Once this has been loaded into the currentFileLength variable, we change the state to "payload".

### payload

In this state we're waiting for the remaining bytes to be available. As soon as they become available, we read the key using the readUTF() method on the URLStream class. readUTF automatically reads a short first and expects these two bytes to contain the length of the string to be read in UTF format - thus the use of UTF8Encoding over ASCIIEncoding. As both encodings take up the same amount of bytes, it's purely a matter of convenience. After this we read in the payload - the image. It's important to explicitly set the ByteArray endianness to avoid problems since the ByteArray by default uses little endian while our ImageStream uses big endian. Note that the header bytes contains the combined length of the key + payload, thus we should only read in currentFileLength - currentKey.length bytes. Finally we dispatch a custom FileReadEvent (see code further down) taking in the key and payload bytes as parameters.

### CombinedFileReader.as

```actionscript
package dk.improve.net
{
    import flash.events.*;
    import flash.net.*;
    import flash.utils.*;

    import mx.events.FlexEvent;

    public class CombinedFileReader extends EventDispatcher
    {
        private var url:String;
        private var urlStream:URLStream;
        private var currentState:String = "header";
        private var currentFileLength:int;
        private var currentKey:String;

        public function CombinedFileReader(url:String)
        {
            this.url = url;
        }

        public function load():void
        {
            urlStream = new URLStream();
            urlStream.endian = Endian.LITTLE_ENDIAN;
            urlStream.addEventListener(ProgressEvent.PROGRESS, onProgress);
            urlStream.addEventListener(Event.COMPLETE, onProgress);
            urlStream.load(new URLRequest(url));
        }

        private function onProgress(evt:Event):void
        {
            switch(currentState)
            {
                case "header":
                    if(urlStream.bytesAvailable >= 4)
                    {
                        currentFileLength = urlStream.readInt();
                        currentState = "payload";

                        onProgress(evt);
                    }
                    break;

                case "payload":
                    if(urlStream.bytesAvailable >= currentFileLength)
                    {
                        currentKey = urlStream.readUTF();
                        var payloadLength = currentFileLength - currentKey.length;

                        var bytes:ByteArray = new ByteArray();
                        bytes.endian = Endian.BIG_ENDIAN;
                        urlStream.readBytes(bytes, 0, payloadLength);

                        dispatchEvent(new FileReadEvent(FileReadEvent.ON_LOADED, currentKey, bytes));

                        currentState = "header";

                        onProgress(evt);
                    }
                    break;
            }
        }
    }
}
```

### FileReadEvent.as

```actionscript
package dk.improve.net
{
    import flash.events.Event;
    import flash.utils.ByteArray;

    public class FileReadEvent extends Event
    {
        public static const ON_LOADED:String = "onLoaded";
        public var file:ByteArray;
        public var key:String;

        public function FileReadEvent(type:String, key:String, file:ByteArray):void
        {
            this.file = file;
            this.key = key;

            super(type);
        }
    }
}
```

## On the client side: Thumbnails.mxml

The final part of demoing the bundled image stream is to actually consume the stream by using the CombinedFileReader AS3 class. Once the application loads we instantiate a new CombinedFileReader, passing in the url to the Image.ashx HttpHandler I mentioned earlier. Before calling the load() method we subscribe to the ON_LOADED event that's dispatched by the CombinedFileReader.

Once we've read in a file and onFileLoaded() is called, we first need to create a new Loader object and pass the bytes into it using loadBytes(). Before loading the bytes we store a position object in a dictionary. The position object will contain the x & y coordinates for the image once it's loaded. We can count on the onFileLoaded function to be called in the same order as the images are streamed. Due to the asynchronous nature of loadBytes() the onLoadComplete() function will be called at random times and will thus not be sequential. Once the bytes are loaded in and onLoadComplete is called, we create a new Image, set the source to the loaded content, set the size and coordinate. Finally we add the image to the current application as an element. Note that the images are 100x100px but to conserve space I'm resizing the client side to 50x50px.

### Thumbnails.mxml

```xml
<?xml version="1.0" encoding="utf-8"?>
<?xml:namespace prefix = s />
    <?xml:namespace prefix = fx />
```

## The result

If all goes well, the result should look like this:

combinedimagereader_1_2.jpg

combinedimagereader_2_2.jpg
