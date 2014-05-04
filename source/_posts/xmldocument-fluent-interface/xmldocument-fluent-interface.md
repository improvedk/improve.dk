---
permalink: xmldocument-fluent-interface
title: XmlDocument Fluent Interface
date: 2007-10-20
tags: [.NET]
---
I do a lot of backend programming for Flash frontends. That basically means a lot of ASPX pages that simply return some XML and accept some incoming XML for parameters. Most of the UI logic ends up getting cluttered with manual XML stringbuilding, so I saw this as an obvious opportunity to play around with a fluent interfaces.

<!-- more -->

Now, here's an example of a typical boolean yes/no result from a Flash query:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<result type="boolean">true</result>
</root>
```

I'd usually create this bit of XML using a simple StringBuilder like so:

```cs
StringBuilder output = new StringBuilder();
output.Append("<?xml version="1.0" encoding="utf-8"?>");
output.Append("<root>");
output.Append("<result type="boolean">true</result>");
output.Append("</root>");
```

This has the advantage of being very fast to write, but readability suffers from the escaped quotes, lack of indentation and there's a whole lot of text when the XML becomes just a bit more advanced.

A "prettier" way is to use the DOM through the XmlDocument like so:

```cs
XmlDocument xd = new XmlDocument();
xd.AppendChild(xd.CreateXmlDeclaration("1.0", "utf-8", ""));

XmlNode root = xd.CreateElement("root");
xd.AppendChild(root);

XmlNode result = xd.CreateElement("result");
result.InnerText = "true";

XmlAttribute type = xd.CreateAttribute("type");
type.Value = "boolean";

result.Attributes.Append(type);
root.AppendChild(result);
```

While this does produce exactly the same XML, it takes up twice as many lines of code, excluding the whitespace lines. Without whitespace it is even more unreadable.

Let me introduce you to my quick'n'simple fluent interface that uses XmlDocument internally, XmlOutput:

```cs
XmlOutput xo = new XmlOutput()
	.XmlDeclaration()
	.Node("root").Within()
		.Node("result").Attribute("type", "boolean").InnerText("true");
```

Using XmlOutput we're down to four lines, the shortest example yet. While linecount is not, and should not be, a measurement of quality, it is preferred. I believe using XmlOutput is, by far, the most readable example.

Basically, using Node() creates a new node within the current node. If no node has been created previously, it will automatically be the root node. Any time a new node is created, it automatically becomes the "current node". Calling Within() moves the context into the current node, thus any newly created nodes will be created within that node. Attribute() will add an attribute to the current node, likewise will InnerText() set the InnerText of the current node. EndWithin() moves the context to the parent node, it is not mandatory for "closing" the nodes, it is only required when you actually need to move the scope.

Let me present you with a couple of examples. Dynamic data:

```cs
XmlOutput xo = new XmlOutput()
	.XmlDeclaration()
	.Node("root").Within()
		.Node("numbers").Within();

for (int i = 1; i <= 10; i++)
	xo.Node("number").Attribute("value", i.ToString()).InnerText("This is the number: " + i);
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<numbers>
		<number value="1">This is the number: 1</number>
		<number value="2">This is the number: 2</number>
		<number value="3">This is the number: 3</number>
		<number value="4">This is the number: 4</number>
		<number value="5">This is the number: 5</number>
		<number value="6">This is the number: 6</number>
		<number value="7">This is the number: 7</number>
		<number value="8">This is the number: 8</number>
		<number value="9">This is the number: 9</number>
		<number value="10">This is the number: 10</number>
	</numbers>
</root>
```

And complex structures:

```cs
XmlOutput xo = new XmlOutput()
	.XmlDeclaration()
	.Node("root").Within()
		.Node("user").Within()
			.Node("username").InnerText("orca")
			.Node("realname").InnerText("Mark S. Rasmussen")
			.Node("description").InnerText("I'll handle any escaping (like < & > for example) needs automagically.")
			.Node("articles").Within()
				.Node("article").Attribute("id", "25").InnerText("Handling DBNulls")
				.Node("article").Attribute("id", "26").InnerText("Accessing my privates")
				.EndWithin()
			.Node("hobbies").Within()
				.Node("hobby").InnerText("Fishing")
				.Node("hobby").InnerText("Photography")
				.Node("hobby").InnerText("Work");
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
	<user>
		<username>orca</username>
		<realname>Mark S. Rasmussen</realname>
		<description>I'll handle any escaping (like < & > for example) needs automagically.</description>
		<articles>
			<article id="25">Handling DBNulls</article>
			<article id="26">Accessing my privates</article>
		</articles>
		<hobbies>
			<hobby>Fishing</hobby>
			<hobby>Photography</hobby>
			<hobby>Work</hobby>
		</hobbies>
	</user>
</root>
```

Finally, say hello to XmlOutput:

```cs
using System.Xml;
using System.Collections.Generic;

public class XmlOutput
{
	// The internal XmlDocument that holds the complete structure.
	XmlDocument xd = new XmlDocument();

	// A stack representing the hierarchy of nodes added. nodeStack.Peek() will always be the current node scope.
	Stack<XmlNode> nodeStack = new Stack<XmlNode>();

	// Whether the next node should be created in the scope of the current node.
	bool nextNodeWithin;

	// The current node. If null, the current node is the XmlDocument itself.
	XmlNode currentNode;

	/// <summary>
	/// Returns the string representation of the XmlDocument.
	/// </summary>
	/// <returns>A string representation of the XmlDocument.</returns>
	public string GetOuterXml()
	{
		return xd.OuterXml;
	}

	/// <summary>
	/// Returns the XmlDocument
	/// </summary>
	/// <returns></returns>
	public XmlDocument GetXmlDocument()
	{
		return xd;
	}

	/// <summary>
	/// Changes the scope to the current node.
	/// </summary>
	/// <returns>this</returns>
	public XmlOutput Within()
	{
		nextNodeWithin = true;

		return this;
	}

	/// <summary>
	/// Changes the scope to the parent node.
	/// </summary>
	/// <returns>this</returns>
	public XmlOutput EndWithin()
	{
		if (nextNodeWithin)
			nextNodeWithin = false;
		else
			nodeStack.Pop();

		return this;
	}

	/// <summary>
	/// Adds an XML declaration with the most common values.
	/// </summary>
	/// <returns>this</returns>
	public XmlOutput XmlDeclaration() { return XmlDeclaration("1.0", "utf-8", ""); }

	/// <summary>
	/// Adds an XML declaration to the document.
	/// </summary>
	/// <param name="version">The version of the XML document.</param>
	/// <param name="encoding">The encoding of the XML document.</param>
	/// <param name="standalone">Whether the document is standalone or not. Can be yes/no/(null || "").</param>
	/// <returns>this</returns>
	public XmlOutput XmlDeclaration(string version, string encoding, string standalone)
	{
		XmlDeclaration xdec = xd.CreateXmlDeclaration(version, encoding, standalone);
		xd.AppendChild(xdec);

		return this;
	}

	/// <summary>
	/// Creates a node. If no nodes have been added before, it'll be the root node, otherwise it'll be appended as a child of the current node.
	/// </summary>
	/// <param name="name">The name of the node to create.</param>
	/// <returns>this</returns>
	public XmlOutput Node(string name)
	{
		XmlNode xn = xd.CreateElement(name);

		// If nodeStack.Count == 0, no nodes have been added, thus the scope is the XmlDocument itself.
		if (nodeStack.Count == 0)
		{
			xd.AppendChild(xn);

			// Automatically change scope to the root DocumentElement.
			nodeStack.Push(xn);
		}
		else
		{
			// If this node should be created within the scope of the current node, change scope to the current node before adding the node to the scope element.
			if (nextNodeWithin)
			{
				nodeStack.Push(currentNode);

				nextNodeWithin = false;
			}

			nodeStack.Peek().AppendChild(xn);
		}

		currentNode = xn;

		return this;
	}

	/// <summary>
	/// Sets the InnerText of the current node without using CData.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public XmlOutput InnerText(string text)
	{
		return InnerText(text, false);
	}

	/// <summary>
	/// Sets the InnerText of the current node.
	/// </summary>
	/// <param name="text">The text to set.</param>
	/// <returns>this</returns>
	public XmlOutput InnerText(string text, bool useCData)
	{
		if (useCData)
			currentNode.AppendChild(xd.CreateCDataSection(text));
		else
			currentNode.AppendChild(xd.CreateTextNode(text));

		return this;
	}

	/// <summary>
	/// Adds an attribute to the current node.
	/// </summary>
	/// <param name="name">The name of the attribute.</param>
	/// <param name="value">The value of the attribute.</param>
	/// <returns>this</returns>
	public XmlOutput Attribute(string name, string value)
	{
		XmlAttribute xa = xd.CreateAttribute(name);
		xa.Value = value;

		currentNode.Attributes.Append(xa);

		return this;
	}
}
```

Enjoy!
