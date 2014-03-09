permalink: why-use-big-endian-for-compressed-integers
title: Why Use Big Endian For Compressed Integers?
date: 2012-02-29
tags: [SQL Server - Internals]
---
While implementing compression support for [OrcaMDF](https://github.com/improvedk/OrcaMDF" target="_blank), it stumped me when I discovered that integers (including datetime, money and all other type based on integers) were [stored in big endian](http://improve.dk/archive/2012/01/30/the-anatomy-of-row-amp-page-compressed-integers.aspx" target="_blank). As all other integers are stored in little endian and I couldn’t see why they’d want to change it, I assumed it must’ve been due to using different libraries or something to that extent.

<!-- more -->

However, at that time I was only implementing row compression, and thus I didn’t consider the implications endianness would have on page compression. You see, one of the techniques page compression uses to save space is column prefixing. When page compressing a page, SQL Server will look for any common column prefix values (at the byte level, page compression is data type agnostic). The best match for a column prefix will be stored as an overall prefix for the column and all other columns will base their value off of this, only storing the diff.

Let’s take a look at a sample identity column and it’s values, stored in little endian:

```

2312398493 = 0x9D66D489
2312398494 = 0x9E66D489
2312398495 = 0x9F66D489
2312398496 = 0xA066D489

```

In this case, the very first byte (the least significant in little endian) changes for each number and thus there’s no common prefix to use for page compression. If we instead store those same values in big endian, look what we get:

<pre class="plain" parse="false">
2312398493 = 0x**89D466**9D
2312398494 = 0x**89D466**9E
2312398495 = 0x**89D466**9F
2312398496 = 0x**89D466**A0
</pre>

Notice how the bolded bytes are all the same now – only the last, and least significant, byte is changed when the number increments. As such, we can now store the values using just two bytes instead of four:

```
Column prefix = 0x89D4669D
```

```

2312398493 =
2312398494 = 0x039E
2312398495 = 0x039F
2312398496 = 0x03A0

```

The first column actually takes up zero bytes, as it matches the stored column prefix exactly – a saving of four bytes! All the others store a single byte that identifies how many bytes to use from the stored column prefix (0x03), as well as the differential bytes coming afterwards.

Once you realize why we need to use big endian instead of little endian, it’s so obvious. Without the change of endianness, page compression would be way less effective.
