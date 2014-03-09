permalink: creating-a-short-link-internet-explorer-addin
title: Creating a "Shorten Link" Internet Explorer Addin
date: 2006-12-16
tags: [.NET]
---
You probably already know of the short link services that are around, [www.makeashorterlink.com](http://makeashorterlink.com/index.php) for instance. While the idea behind these sites are indeed good, I personally find it quite cumbersome to actually create the short links when I'm typing a forum post somewhere. In this post I'll give an example of a short link system that enables one to easily create short links while typing in Internet Explorer, using an Internet Explorer addon that conveniently places itself in the context menu whenever we right click on some text and a .NET webservice that handles the short link functionality.

shortlink1_2.jpg

## What's a short link?

I'll keep this part short (pun intended). Often when writing in forums, emails, newsgroups and so forth, we need to post links. The problem arises when we have to post a link like this: [http://www.experts-exchange.com/Programming/Programming_Languages/C_Sharp/Q_20801558.html](http://www.experts-exchange.com/Programming/Programming_Languages/C_Sharp/Q_20801558.html), it's quite large! This may easily ruin the desing of the website and annoy people as it takes up a lot of space. A short link equivalence is: [http://link.improve.dk/8](http://link.improve.dk/8).

## The idea

So how can we make it more convenient, so we won't have to open up a new browser, go to the short link website, create the short link and copy it back into the other browser window? My proposal is that we create an Internet Explorer addin for the context menu like shown in the picture above. That means we can simply type the long url as normally, select it and the right click and select "Replace with short link" whereafter it'll automatically get replaced by a corresponding short link.

## The components

I won't be hooking into any of the existing short link services as I really wan't to show how such a service could be created from the bottom up, it's actually quite simple. The complete service covers four separate projects. We have the actual Internet Explorer addin which is written in JavaScript. Then we have a .NET DLL that'll be installed on the client machine, this is the one that'll take care of the communication with the short link server. Furthermore we have the "Create a short link" webservice that'll handle the actual creation of short links. And finally we have the short link redirection website which ensures that http://link.improve.dk/* redirects to the URL that's behind it.

## The database

For storing the short links I'm using a MS SQL server database, of course you can use any kind of storage you want, whether that be Access, XML or some other format. The table I'll be using can be seen here:

<pre lang="sql">CREATE TABLE [dbo].[tblShortLinks](
	[LinkID] [int] IDENTITY(1,1) NOT NULL,
	[URL] [varchar](512) NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_tblShortLinks_Created] DEFAULT (getdate()),
	[IP] [varchar](50) NOT NULL,
	[Visits] [int] NOT NULL CONSTRAINT [DF_tblShortLinks_Visits] DEFAULT (0))</pre>

We have an identity column, the URL, a column reprensenting the creation time of the link, the IP of the creator (for abuse checking) and finally a simple counter that counts how many times the link has been visited.

## The "Create a short link webservice" – ShortLinkService

I'll dive right into it and present the complete code for the webservice, ShortLinkService.cs:

```csharp
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System;
using System.Text.RegularExpressions;

[WebService(Namespace = "http://link.improve.dk")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ShortLinkService : WebService
{
	[WebMethod]
	public string CreateShortLinkFromURL(string url)
	{
		// We won't handle invalid URL's
		if (!Regex.IsMatch(url, @"[-w.]+://([-w.]+)+(:d+)?(:w+)?(@d+)?(@w+)?([-w.]+)(/([w/_.]*(?S+)?)?)?"))
			return "BAD URL";

		// Create a SqlCommand that'll lookup the URL to see if it already exists
		SqlCommand cmd = new SqlCommand("SELECT LinkID FROM tblShortLinks WHERE URL = @URL");
		cmd.Parameters.Add("@URL", SqlDbType.VarChar, 512).Value = url;

		object result = DB.GetScalar(cmd);

		// If it exists, just return the existing short link
		if (result != null)
			return "http://link.improve.dk/" + result;
		else
		{
			// Since it doesn't exist, create it and return the new link
			cmd = new SqlCommand("INSERT INTO tblShortLinks (URL, IP) VALUES (@URL, @IP); SELECT @@IDENTITY");
			cmd.Parameters.Add("@URL", SqlDbType.VarChar, 512).Value = url;
			cmd.Parameters.Add("@IP", SqlDbType.VarChar, 50).Value = HttpContext.Current.Request.UserHostAddress;

			return "http://link.improve.dk/" + Convert.ToInt32(DB.GetScalar(cmd));
		}
	}
}
```

First of all we have to make a crude check to see if it's a valid URL, if not, we won't handle it. Next we'll try and see if the requested URL already exists as a short link. If it does we might as well just return that instead of creating a new one.

Please don't get confused by my call to another class that I use (which I won't be presenting): DB.GetScalar(). What DB.GetScalar() does is to simply connect to my database and run the SqlCommand on it, returning the scalar value of the query. I've kept this code separate as it would only clutter the real purpose of this code.

After getting the value of the SQL query we check if the link exists. If it does we return the short link URL, if not, we create the short link while selecting the newly created identity value at the same time. And then finally we return the URL, just as if it already existed.

That was the complete short link webservice code. All it does is to simply take a URL and return a corresponding short url.

## The redirector website

This is the website running at http://link.improve.dk. I've setup [wildcard mapping](http://scottwater.com/blog/articles/Wildcard.aspx) so all requests passes through to the ASPNET engine. This enables me to write the redirection functionality in the Global.asax file like so:

```csharp
using System.Web;
using System;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;

public class Global : HttpApplication
{
	private void Application_BeginRequest(object sender, EventArgs e)
	{
		string input = Request.Path.Replace("/", "");

		// Let's do a simple validation, the input has to be a number
		if (!Regex.IsMatch(input, @"^d+$"))
			DB.Debug("Not a valid short link.");

		int linkID = Convert.ToInt32(input);

		// Lookup URL in database
		SqlCommand cmd = new SqlCommand("UPDATE tblShortLinks SET Visits = Visits + 1 WHERE LinkID = @LinkID; SELECT URL FROM tblShortLinks WHERE LinkID = @LinkID");
		cmd.Parameters.Add("@LinkID", SqlDbType.Int).Value = linkID;

		object url = DB.GetScalar(cmd);

		if (url != null)
			Response.Redirect(url.ToString());
		else
			DB.Debug("Link does not exist.");
	}
}
```

First of all we retrieve the input value which consists of the requested path - exluding the leading forward slash. Then we perform a very quick input validation, checking that the requested path is a number. Most short link services use a combination of numbers and alphanumeric characters to keep the actual URL as short as possible. While this is a much better solution, I've stuck to the simple version by simple using numbers.

After retrieving the link ID from the requested path, we look it up in the database - and incrementing the visits count at the same time. If it exists then we perform a Response.Redirect to the requested URL, if it doesn't exist then we write an error output.

The DB.Debug() function is very simple. All it does is to perform a Response.Write() of the passed object and then a Response.End() afterwards, very retro debugging stylish. This concludes the complete short link redirection website.

## The managed clientside library – ShortLinkClient

This is where things start to get interesting. This library is the .NET class that'll take care of communicating with the ShortLinkService webservice from the client computer. The ShortLinkClient library will be invoked from the Internet Explorer addin through COM (as we *have* to use JavaScript for the actual addin, unfortunately).

Start by creating a new class library project. Now either delete the Class1.cs or rename it to your liking. Then add a web reference to the following web service:[http://linkservice.improve.dk/ShortLinkService.asmx](http://linkservice.improve.dk/ShortLinkService.asmx). As this library has to be exported to COM we have to sign it using a strong name key file. Fortunately this is quite simple in Visual Studio 2005:

shortlink2_2.jpg

Simply right click the ShortLinkClient project and select properties. Then go to the Signing tab, check "Sign the assembly" and choose "New..." from the dropdown list. This'll create a new strong name key file for the library. You do not have to password protect it.

Make sure the ShortLinkClient.cs code matches the following:

```csharp
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Improve
{
	[ComVisible(true)]
	public class ShortLinkClient
	{
		public ShortLinkClient()
		{
		}

		public string CreateShortLink(string url)
		{
			LinkService.ShortLinkService shortLinkService = new LinkService.ShortLinkService();

			return shortLinkService.CreateShortLinkFromURL(url);
		}

		public bool IsLink(string text)
		{
			return Regex.IsMatch(text, @"[-w.]+://([-w.]+)+(:d+)?(:w+)?(@d+)?(@w+)?([-w.]+)(/([w/_.]*(?S+)?)?)?");
		}
	}
}
```

By default our .NET classes are not visible to COM when it's exported, that why we have to set the [ComVisible(true)] attribute on the ShortLinkClient class. Also, for our class to be usable by COM, we have to create a constructor that takes no parameters, otherwise COM won't be able to use our class.

The IsLink function is used to determine whether the right clicked text is actually a valid URL or if it's just plain text.

CreateShortLink takes the URL as a parameter. It then creates an instance of our ShortLinkService proxy class whereafter it invokes the CreateShortLinkFromURL function of the webservice which should return the short link equivalence of the passed URL. This is the only managed code that will run on the client as part of the Internet Explorer addin.

After building the ShortLinkClient project, register it for COM interop using the following command from a command prompt:

```csharp
regasm /codebase ShortLinkClient.dll
```

To unregister, simply switch the /codebase switch with /unregister. You may have to close any open Internet Explorer windows in case the library has been invoked through the addin as these windows otherwise may lock the dll file.

## Creating the actual addin

Unfortunately Internet Explorer lacks a bit when it comes to creating add ins for the context menu. I'd really like to be able to write in managed code all the way, but as things are currently, this part has to be written in JavaScript.

To register the addin, create a new registry key in the following location: HKEY_CURRENT_USER/Software/Microsoft/Internet Explorer/MenuExt. The key should have the name that you want to be present in the context menu. In my case that'd result in the following key being created: "HKEY_CURRENT_USER/Software/Microsoft/Internet Explorer/MenuExt/Replace with short link".

shortlink3_2.jpg

The default value should point to the location where you want your javascript file to be located. For development purposes you can place this anywhere you want (I'm using the desktop). For deployment you might want to consider using the Program Files folder.

<pre lang="javascript"><script type="text/javascript">// <![CDATA[
	function main()
	{
		// Get document reference
		var doc = external.menuArguments.document;
		var range = doc.selection.createRange();

		// Check if any text has been selected
		if(!range.text)
		{
			alert('No text selected.');
			return;
		}

		// Create the ShortLinkClient object
		var shortLinkClient = new ActiveXObject('Improve.ShortLinkClient');

		// If link starts with www, let's add http:// for convenience
		var link = range.text.indexOf('www') == 0 ? 'http://' + range.text : range.text;

		// Check if selected text is actually a URL
		if(!shortLinkClient.IsLink(link))
		{
			alert('Selected text is not a valid URL.');
			return;
		}

		// Get a short link
		var shortLink = shortLinkClient.CreateShortLink(link);

		// Replace the selected text with the short link
		range.text = shortLink;
	}

	main();
// ]]></script></pre>

The JavaScript file contains absolutely standard JavaScript code inside a normal script element. In the main function we can access the relevant html Document by accessing the external.menuArguments.document property. We can retrieve the selected text by creating a range using the Document.selection.createRange() function.

First we have to check whether any text was selected at all. Then we create an instance of the ShortLinkClient library (remember, it'll have to be registered for COM interop for this to work - using regasm).

For convenience I'll add http:// in case the link starts with www. For it to be a valid link it has to be in the form protocol://*, so by adding the http:// I'll allow users to shortlink links like "www.improve.dk" instead of them having to type the complete [http://www.improve.dk](http://www.improve.dk).

Next we check whether the selected text is a link by using the ShortLinkClient.IsLink() function, passing in the selected text. If everything passes thus far, we invoke the ShortLinkClient.CreateShortLink() function which'll invoke the webservice and create the short link - and return the short link URL which we will the use the replace the selected text with in the last line of the main function.

If all goes well, this should be the result of our four components working together:

shortlink4_2.jpg

## Downloads

[short_link_code.zip - Sample solution](http://improve.dk/wp-content/uploads/2006/12/short_link_code.zip)

## Improvements

Many features of this example can be improved if this was meant to be deployed for real. For instance the regular expressions may not match all valid URL's. Also we should probably make some kind of "loading" text while the webservice creates the short link, instead of letting the user sit there for a second or two before the short link emerges (the first time it's called it'll take some time, afterwards it should be nearly instant).

Please let me know if you have any suggestions, questions or any other relevant comments, thanks!
