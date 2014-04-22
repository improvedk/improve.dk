---
permalink: redirecting-old-permalinks-on-statically-generated-blogs
title: Redirecting Old Permalinks on Statically Generated Blogs
date: 2014-04-21 01:56:12
tags: [Miscellaneous]
---
Having [just migrated from Wordpress to Hexo](/migrating-from-wordpress-to-hexo/), I quickly realized I forgot something. I forgot to redirect my old permalinks to the new ones...

## Permalinks Aren't Necessarily Permanent
A permalink ought to live for the duration of your content, and most importantly, never change. However, having been through a number of different blog engines, not all of them support the same permalink structures, and might not even support redirecting old ones. As such, throgh the years my posts have ended up with multiple permalinks:

* http://improve.dk/archive/2008/03/23/sql-server-mirroring-a-practical-approach.aspx
* http://improve.dk/blog/2008/03/23/sql-server-mirroring-a-practical-approach/
* http://improve.dk/sql-server-mirroring-a-practical-approach/

As you can see, I've dropped both the /archive/ and the /blog/ prefixes, as well as the dates. Redirecting old incoming links to the new ones was easy enough when I ran Wordpress on Apache. All it requires were a couple of lines in the .htaccess file:

```
# Redirect old permalink structure
<IfModule mod_rewrite.c>
	RewriteEngine On
	RewriteRule ^archive/([0-9]{4})/([0-9]{2})/([0-9]{2})/([^\.]+)\.aspx$ http://improve.dk/$4/ [NC,R=301,L]
	RewriteRule ^blog/([0-9]{4})/([0-9]{2})/([0-9]{2})/([^\.]+)$ http://improve.dk/$4/ [NC,R=301,L]
</IfModule>
```

## Static Woes
Since I've migrated to [Hexo](http://hexo.io) it's not as simple, unfortunately. I no longer host my site on Apache, but on [GitHub Pages](https://pages.github.com/). GitHub Pages only allow static files to be served, so I'm no longer able to utilize the .htaccess rewriting rules. There's also no server-side functionality available, so I can't even manually send out a 302-redirect, needed to preserve my incoming links SEO value.

What I ended up doing was to write a small script that would parse my Wordpress backup file and then recreate the /blog/ and /archive/ directories, as if the posts were actually stored there:

```csharp
string template = @"layout: false
---
<!DOCTYPE html>
<html>
	<head>
		<title>Redirecting to [Title]</title>
  		<link rel=""canonical"" href=""[Permalink]""/>
		<meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
		<meta http-equiv=""refresh"" content=""0;url=[Permalink]"" />
	</head>
	<body>
		Redirecting to <a href=""[Permalink]"">[Title]</a>...
	</body>
</html>";

void Main()
{
	var outputPath = @"D:\Projects\improve.dk (GIT)\source\";
	var xmlPath = @"D:\Projects\improve.dk (GIT)\marksrasmussen-blog.wordpress.2014-03-08.xml";
	var xml = File.ReadAllText(xmlPath);
	var xd = new XmlDocument();
	xd.LoadXml(xml);
	
	var nsmgr = new XmlNamespaceManager(xd.NameTable);
	nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
	nsmgr.AddNamespace("wp", "http://wordpress.org/export/1.2/");
	
	foreach (XmlNode item in xd.SelectNodes("//item"))
	{
		var title = item.SelectSingleNode("title").InnerText;
		var date = Convert.ToDateTime(item.SelectSingleNode("pubDate").InnerText);
		var slug = item.SelectSingleNode("wp:post_name", nsmgr).InnerText;
		
		var indexHtml = template
			.Replace("[Permalink]", "http://improve.dk/" + slug + "/")
			.Replace("[Title]", HttpUtility.HtmlEncode(title));
		
		// First create the /archive/ entry
		var outputFolder = Path.Combine(outputPath, "archive", date.Year.ToString(), date.Month.ToString().PadLeft(2, '0'), date.Day.ToString().PadLeft(2, '0'), slug + ".aspx");
		var indexPath = Path.Combine(outputFolder, "index.html");
		Directory.CreateDirectory(outputFolder);
		File.WriteAllText(indexPath, indexHtml);
		
		// Then the /blog/ entry
		outputFolder = Path.Combine(outputPath, "blog", date.Year.ToString(), date.Month.ToString().PadLeft(2, '0'), date.Day.ToString().PadLeft(2, '0'), slug);
		indexPath = Path.Combine(outputFolder, "index.html");
		Directory.CreateDirectory(outputFolder);
		File.WriteAllText(indexPath, indexHtml);
	}
}
```

Now the post stored directly in the root, while placeholders have been put in place in the old /blog/ and /archive/ directories. The placeholder code is very simple:

```html
layout: false
---
<!DOCTYPE html>
<html>
	<head>
		<title>Redirecting to TxF presentation materials</title>
  		<link rel="canonical" href="http://improve.dk/txf-presentation-materials/"/>
		<meta http-equiv="content-type" content="text/html; charset=utf-8" />
		<meta http-equiv="refresh" content="0;url=http://improve.dk/txf-presentation-materials/" />
	</head>
	<body>
		Redirecting to <a href="http://improve.dk/txf-presentation-materials/">TxF presentation materials</a>...
	</body>
</html>
```

It's simply a small script that contains a meta refresh tag that sends the user on to the new URL. By utilizing the ´rel="canonical"` meta tag, I ensure that this retains the SEO value as if I had performed a 302 redirect.

## Going Forward
Creating the placeholder files is a one-off task, seeing as I'll only ever need to redirect posts that precede the time when I changed my URL structure to contain neither the /blog/ and /archive/ prefixes, nor the dates. All posts from the beginning of 2013 were published using the current URL scheme, which I intend to keep for the foreseeable future.