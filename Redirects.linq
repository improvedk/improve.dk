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

string template = @"layout: false
---
<!DOCTYPE html>
<html>
	<head>
		<title>[Title]</title>
  		<link rel=""canonical"" href=""[Permalink]""/>
		<meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
		<meta http-equiv=""refresh"" content=""0;url=[Permalink]"" />
	</head>
	<body>
		[Title]...
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
		var content = item.SelectSingleNode("content:encoded", nsmgr).InnerText;
		var slug = item.SelectSingleNode("wp:post_name", nsmgr).InnerText;
		
		var indexHtml = template
			.Replace("[Permalink]", "http://improve.dk/" + slug + "/")
			.Replace("[Title]", "Redirecting to " + HttpUtility.HtmlEncode(title));
		
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