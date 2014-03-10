<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

void Main()
{
	var outputPath = @"D:\Projects\improve.dk (Hexo, GIT)\source\_posts";
	var xmlPath = @"D:\Projects\improve.dk (Hexo, GIT)\marksrasmussen-blog.wordpress.2014-03-08.xml";
	var testPath = @"D:\Projects\improve.dk (Hexo, GIT)\test.html";
	var xml = File.ReadAllText(xmlPath);
	var xd = new XmlDocument();
	xd.LoadXml(xml);
	
	var nsmgr = new XmlNamespaceManager(xd.NameTable);
	nsmgr.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
	nsmgr.AddNamespace("wp", "http://wordpress.org/export/1.2/");
	
	var testHtml = new StringBuilder();
	
	int count = 1;
	foreach (XmlNode item in xd.SelectNodes("//item"))
	{
		var title = item.SelectSingleNode("title").InnerText;
		var date = Convert.ToDateTime(item.SelectSingleNode("pubDate").InnerText);
		var content = item.SelectSingleNode("content:encoded", nsmgr).InnerText;
		var slug = item.SelectSingleNode("wp:post_name", nsmgr).InnerText;
		var categories = getCategories(item);
				
		string postOutputDir = Path.Combine(outputPath, slug);
		Directory.CreateDirectory(postOutputDir);
		
		string postFilePath = Path.Combine(postOutputDir, slug + ".md");
		var sb = new StringBuilder();
		
		sb.AppendLine("permalink: " + slug);
		sb.AppendLine("title: " + title);
		sb.AppendLine("date: " + date.ToString("yyyy-MM-dd"));
		sb.AppendLine("tags: [" + string.Join(", ", categories) + "]");
		sb.AppendLine("---");
		sb.AppendLine(formatContent(content));
		
		File.WriteAllText(postFilePath, sb.ToString());
		
		testHtml.AppendLine((count++).ToString().PadLeft(3, '0') + "&nbsp;&nbsp;&nbsp;<a href='http://127.0.0.1:4000/" + slug + "/'>Hexo</a>&nbsp;&nbsp;&nbsp;<a href='http://improve.dk/" + slug + "/'>WP</a>&nbsp;&nbsp;&nbsp;" + slug + "<br />");
		
		Console.WriteLine(date + ": " + slug);
	}
	
	File.WriteAllText(testPath, testHtml.ToString());
}

string formatContent(string content)
{
	var options = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled;

	// H2
	content = Regex.Replace(content, "^<h2>(.+?)</h2>(\n|\r|\r\n)", "## $1", options);
	
	// H3
	content = Regex.Replace(content, "^<h3>(.+?)</h3>(\n|\r|\r\n)", "### $1", options);
	
	// H4
	content = Regex.Replace(content, "^<h4>(.+?)</h4>(\n|\r|\r\n)", "### $1", options);
	
	// Image
	content = Regex.Replace(content, "^<div class=('|\")imgwrapper('|\")>.*?/\\d+/\\d+/(.*?)\"><img .*?</a></div></div>", "$3", options);
	
	// tsql
	content = Regex.Replace(content, "<pre lang=\"tsql\">(.*?)</pre>", m => "```sql" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	content = Regex.Replace(content, "<pre lang=\"tsql\" escaped=\"true\">(.*?)</pre>", m => "```sql" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	content = Regex.Replace(content, "<pre escaped=\"true\" lang=\"tsql\">(.*?)</pre>", m => "```sql" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
		
	// csharp
	content = Regex.Replace(content, "<pre lang=\"csharp\"( escaped=\"true\")?>(.*?)</pre>", m => "```csharp" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[2].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	content = Regex.Replace(content, "\\[csharp\\](.+?)\\[/csharp\\]", m => "```csharp" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	
	// plain
	content = Regex.Replace(content, "<pre class=\"plain\">(.*?)</pre>", m => "```" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	content = Regex.Replace(content, "<pre lang=\"text\">(.*?)</pre>", m => "```" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	
	// xml
	content = Regex.Replace(content, "<pre lang=\"xml\" escaped=\"true\">(.+?)</pre>", m => "```xml" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	content = Regex.Replace(content, "<pre lang=\"mxml\" escaped=\"true\">(.+?)</pre>", m => "```mxml" + Environment.NewLine + HttpUtility.HtmlDecode(m.Groups[1].Value) + Environment.NewLine + "```", RegexOptions.Singleline);
	
	// bash
	content = Regex.Replace(content, "<pre lang=\"bash\">(.+?)</pre>", m => "```bash" + Environment.NewLine + m.Groups[1].Value + Environment.NewLine + "```", RegexOptions.Singleline);
	
	// a
	content = Regex.Replace(content, "<a href=\"(.+?)\">(.+?)</a>", "[$2]($1)", RegexOptions.Singleline);
	
	// b
	content = Regex.Replace(content, "<b>(.+?)</b>", "**$1**", options);
	
	// strong
	content = Regex.Replace(content, "<strong>(.+?)</strong>", "**$1**", options);
	
	// p
	content = Regex.Replace(content, "<p>(.+?)</p>", "$1", RegexOptions.Singleline);
	
	// Entities
	content = content.Replace("&amp;", "&");
	content = content.Replace("&gt;", ">");
	content = content.Replace("&lt;", "<");
	
	// br
	content = Regex.Replace(content, "<br ?/>", "  ", options);
		
	// em
	content = Regex.Replace(content, "<em>(.+?)</em>", "*$1*", options);
	content = Regex.Replace(content, "<i>(.+?)</i>", "*$1*", options);
		
	// Exercerpt
	content = replaceFirst(content, Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine + "<!-- more -->" + Environment.NewLine + Environment.NewLine);
	
	// li
	content = Regex.Replace(content, "\t?<li>(.+?)</li>", "* $1", options);
	
	// ul
	content = Regex.Replace(content, "<ul>(.+?)</ul>", "$1", RegexOptions.Singleline);
	
	return content;
}

string replaceFirst(string content, string search, string replace)
{
	int pos = content.IndexOf(search);
	
	if (pos < 0)
		return content;
	
	return content.Substring(0, pos) + replace + content.Substring(pos + search.Length);
}

string[] getCategories(XmlNode item)
{
	return item.SelectNodes("category[@domain='category']")
		.Cast<XmlNode>()
		.Select(x => translateCategory(HttpUtility.HtmlDecode(x.InnerText)))
		.Distinct()
		.ToArray();
}

string translateCategory(string cat)
{
	switch (cat)
	{
		case "Community": return "SQL Server - Community";
		case "Data Types": return "SQL Server - Data Types";
		case "Internals": return "SQL Server - Internals";
		case "Misc": return "SQL Server";
		case "Optimization": return "SQL Server - Optimization";
		case "OrcaMDF": return "SQL Server - OrcaMDF";
		case "Tricks": return "SQL Server - Tricks";
		default: return cat;
	}
}