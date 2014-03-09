permalink: orcamdf-row-compression-support
title: OrcaMDF Row Compression Support
date: 2012-02-07
tags: [SQL Server - OrcaMDF]
---
After two months of on and off work, I finally merged the [OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank) compression branch into master. This means OrcaMDF now officially supports data row compression!

## Supported data types

Implementing row compression required me to modify nearly all existing data types as compression changes the way they’re stored. Integers are [compressed](http://improve.dk/archive/2012/01/30/the-anatomy-of-row-amp-page-compressed-integers.aspx" target="_blank), decimals are [variable length](http://improve.dk/archive/2011/12/13/how-are-vardecimals-stored.aspx" target="_blank) and variable length values are generally truncated instead of padded with 0’s. All previous data types implemented in OrcaMDF supports row compression, just as I’ve added a few. The current list of supported data types is as follows:

<ul>
	<li>bigint</li>
	<li>binary</li>
	<li>bit</li>
	<li>char</li>
	<li>date</li>
	<li>datetime</li>
	<li>mal/numeric (including vardecimal, both with and without row compression)</li>
	<li>image</li>
	<li>int</li>
	<li>money</li>
	<li>nchar</li>
	<li>ntext</li>
	<li>nvarchar</li>
	<li>smallint</li>
	<li>smallmoney</li>
	<li>text</li>
	<li>time</li>
	<li>uniqueidentifier</li>
	<li>varbinary</li>
	<li>varchar</li>
</ul>


## Unicode compression

Nchar and nvarchar proved to be slightly more tricky than the rest as they use the [SCSU](http://unicode.org/reports/tr6/" target="_blank) unicode compression format. I found a single [.NET implementation of SCSU](http://gautam-m.blogspot.com/2010/09/standard-compression-scheme-for-unicode.html" target="_blank), but while I was allowed to use the code, it didn’t come with a license and that’s a no go when I want to embed it in OrcaMDF. Furthermore had a lot of Java artifacts lying around (relying on goto for instance) that I didn’t fancy. I chose to implement my own SCSU decompression based on the reference implementation provided by Unicode, Inc. I’ve only implemented decompression to end up with a very slim and simple SCSU decompressor.

I’ll be posting a blog on the decompressor itself as it has some value as a standalone class outside of OrcaMDF.

## Architecture changes

I started out thinking I’d be able to finish off compression support within a week or two, after all, it was [rather well documented](http://www.amazon.com/Microsoft%C2%AE-SQL-Server%C2%AE-2008-Internals/dp/0735626243" target="_blank). It didn’t take long before it dawned upon me, just how many changes implementing compression would require. The record parser would obviously need to know whether the page was compressed or not. But where would it know that from? Previously all it got was a page pointer, now I had to lookup the compression (which varies by partition) metadata and make sure it was passed along all the way from the DataScanner to the page parser, to the record parser and onwards to the data type parsers.

I’ve had to implement multiple abstractions on top of the regular parsers to abstract away whether the records were stored in a compressed or non compressed format. Overall it’s resulted in a better architecture, but it took a lot more work than expected. Actually parsing the compressed record format was the smallest part of the ordeal – the documentation was spot on and it’s a simple format. The data types however, that took some more work until I had the format figured out.

## Taking it for a spin

As always, the [code is available at Github](https://github.com/improvedk/OrcaMDF" target="_blank), feel free to take a look! If you’re not the coding type, I’ve also uploaded the [binary download](https://github.com/improvedk/OrcaMDF/downloads" target="_blank) (dated 2012-02-06) of the OrcaMDF Studio, a GUI built on top of OrcaMDF.

## Stats

Being a lover of numbers myself, I love looking at statistics. Here’s a couple of random stats from OrcaMDF:

<ul>
	<li>123 commits, the first one being made on April 15th 2011 – that’s almost a year ago!</li>
	<li>~11.700 lines of C# code (excluding blanks).</li>
	<li>~1000 lines of comments.</li>
	<li>~35% of the code is dedicated to testing, with a test suite comprising of over 200 tests.</li>
	<li>[Ohloh](https://www.ohloh.net/p/OrcaMDF" target="_blank) estimates OrcaMDF has a development cost of $144,090 – makes me wonder if not my time would be better spent elsewhere...</li>
</ul>
