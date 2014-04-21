---
permalink: linqpad-ndash-the-perfect-playground-for-amazon-web-services
title: LINQPad - The Perfect Playground for Amazon Web Services
date: 2012-02-17
tags: [.NET, Amazon Web Services]
---
I've previously written on [why I don't want to rely on third party GUIs](/how-to-set-up-and-serve-private-content-using-s3) for managing my AWS services. Assuming I'll be interacting with AWS through the SDK later on, I much prefer doing the initial setup using the SDK as well, to ensure I fully understand what I've done. [In](/how-to-set-up-and-serve-private-content-using-s3) [previous](/pushing-the-limits-of-amazon-s3-upload-performance) [posts](/optimizing-single-instance-amazon-s3-delete-performance), I've shown full console application examples on how to use the SDK for various tasks; however, creating a console application project, compiling and running it can be kind of cumbersome, especially if you're just doing some quick testing or API exploration.

<!-- more -->

## Enter LINQPad

If you haven't tried [LINQPad](http://www.linqpad.net/) out before, go do it now! It's an awesome scratchpad style application for writing and running C#/VB.NET/F# code, either as a single expression, a set of statements or a full application. There are also various plugins for easily connecting to databases, Azure services and lots more. Probably just a matter of time before someone writes a [custom provider](http://www.linqpad.net/extensibility.aspx) for AWS as well.

### Getting started with the AWS SDK in LINQPad

First up, you need to [download](http://aws.amazon.com/sdkfornet/) and install the AWS SDK. Once installed, open LINQPad and press F4. In the following dialog, click Add...

image_2.png

You can either browse to the AWSSDK.dll file manually, or you can pick it from the GAC – which would be the easiest option if you're just doing some quick testing.

image_4.png

After adding it, as a final step, go to the *Additional Namespace Imports* tab and add Amazon to the list.

image_61.png

## Discoverability

There are generally two ways to approach a new and unknown API. You [could read the documentation](http://aws.amazon.com/documentation/), or you could just throw yourself at it, relying on intellisense. Unfortunately the free version of LINQPad does not come with intellisense/auto completion, however, at a price of $39 it's a bargain.

image_121.png

### Exception output

Inevitably, you'll probably run into an exception or two. Take the following piece of code for example; it works perfectly, but I forgot to assign the IAM user access to the bucket I'm trying to interact with (and before you remind me to remove the keys, don't worry, the IAM user was temporary and no longer exists):

```csharp
var accessKeyID = "AKIAIKB6FI6SSOGGOP7A";
var secretKey = "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";

using (var client = AWSClientFactory.CreateAmazonS3Client(accessKeyID, secretKey))
{
	var request = new Amazon.S3.Model.ListObjectsRequest()
		.WithBucketName("improve-us.dk");

	using (var result = client.ListObjects(request))
	{
		result.Dump();
	}
}
```

What you'll see is a clear notification saying an exception has occurred, as well as a repeated exception in the output pane:

image_141.png

But look what happens once we expand the exception:

image_161.png

We get all of the output! Any object, returned or thrown from the code, will automatically be displayed in the output, and can easily be drilled into.

### Dumping objects

In case you're code neither throws exceptions (by some miracle, in my case) nor returns objects, you can always call the .Dump() extension method on any object. Once I granted the IAM user the correct permissions, result.Dump() was called and the result of the ListObjectsRequest was output:

image_181.png

Once again it's easy to drill down and see all of the properties of the result.

## Conclusion

Adhering to my original statement of not liking third party GUIs, using tools like LINQPad is an excellent option for easily writing test code, without the added overhead of starting Visual Studio, compiling, running, etc. The output is neatly presented and really helps you to get a feeling for the API.

Just to finish off – LINQPad has plenty of uses besides AWS, this is just one scenario for which I've been using it extensively lately. I'm really considering creating a LINQPad provider for [OrcaMDF](https://github.com/improvedk/OrcaMDF) as one of my next projects.
