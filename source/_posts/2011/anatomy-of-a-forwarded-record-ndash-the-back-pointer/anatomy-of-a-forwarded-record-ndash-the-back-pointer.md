permalink: anatomy-of-a-forwarded-record-ndash-the-back-pointer
title: Anatomy of a Forwarded Record & the Back Pointer
date: 2011-06-09
tags: [SQL Server - Internals]
---
Earlier this week I provided some details on the [forwarding stub that’s left behind](http://improve.dk/archive/2011/06/07/anatomy-of-a-forwarded-record-ndash-the-forwarding-stub.aspx" target="_blank) when a heap record is forwarded. In this post I’ll look at the second part of a forwarded record – the actual record to which the forwarding stub points.

## What’s in a forwarded record?

A forwarded record is just like a normal record, only with a couple of minor differences.

For one, the record type (read from bits 1-3 of the first record status byte, see [the earlier post](http://improve.dk/archive/2011/06/07/anatomy-of-a-forwarded-record-ndash-the-forwarding-stub.aspx" target="_blank)
for details) is changed to BlobFragment, decimal value 4. This is important to note when scanning data – as all blob fragment records should be ignored. Instead, blob fragments will automatically be read whenever we scan the forwarding stub which points to the blob fragment. Scanning both directly would result in the records being read twice.

The second part being that there’s an extra variable length column stored in the record. This last variable length column is not part of the table schema, it’s actually a special pointer called the back pointer. The back pointer points back to the <font color="#444444">forwarding stub that points to this record. This makes it easy to find the original record location, given the blob fragment location. When a blob fragment shrinks in size, we can easily check whether it might fit on the original page again. It’s also used if the blob fragment size increases and we therefore might need to allocate it on a new page. In that case, we’ll have to go back to the</font>forwarding stub and change it so it points to the new location.

The naïve implementation would be to just replace the blob fragment with another forwarding stub, thus creating a chain of forwarding stubs, eventually pointing to the forwarded record itself. However, this is not the case - SQL Server will never chain forwarding stubs.

## Parsing the forwarded record

To check out the back pointer storage format, I’ve reused the table sample from the [last post](http://improve.dk/archive/2011/06/07/anatomy-of-a-forwarded-record-ndash-the-forwarding-stub.aspx" target="_blank). Thus we’ve got a forwarding stub on page (1:114) pointing to the forwarded record on page (1:118). Let’s try and parse the forwarded record at (1:118:0):

**<font color="#ff0000">3200</font><font color="#0000ff">0800</font><font color="#9b00d3">02000000</font><font color="#008000">0200</font>**<font color="#a5a5a5">**0002 009913a3 93 &lt;snip 5.000 x 0x62&gt; 00047200 00000100 0100**</font>

<ul>
	<li><font face="Lucida Sans Unicode"><font color="#ff0000">**3200  **</font>The first two bytes are the two status bytes, indicating that this is a blob fragment record, it’s got a null bitmap and it’s got variable length columns.             </font></li>
	<li><font face="Lucida Sans Unicode"><font color="#0000ff">**0800  **</font><font color="#000000">The next two bytes indicates the total length of the fixed length portion of this record.    </font></font></li>
	<li><font face="Lucida Sans Unicode"><font color="#9b00d3">**02000000  **</font><font color="#000000">Next up is the first and only fixed length column, an integer field with a decimal value of 2.    </font></font></li>
	<li><font face="Lucida Sans Unicode"><font color="#804000"><font color="#008000">**0200  **</font></font><font color="#000000">Indicates the total number of columns in this record – decimal value 2.</font></font>  </li>
</ul>

<font color="#a5a5a5" />

**<font color="#a5a5a5">32000800 02000000 0200</font><font color="#ff0000">00</font><font color="#0000ff">02 00</font><font color="#9b00d3">9913a3</font><font color="#9b00d3">93</font><font color="#008000">&lt;snip 5.000 x 0x62&gt;</font>**<font color="#a5a5a5">**00047200 00000100 0100**</font>

<ul>
	<li><font face="Lucida Sans Unicode"><font color="#ff0000">**00  **</font><font color="#000000">The next byte is the null bitmap. As there are no nullable columns in this table, no columns have a null bit set, thus the value of 0.</font>    </font></li>
	<li><font face="Lucida Sans Unicode"><font color="#0000ff">**0200  **</font><font color="#000000">The next two bytes specify the number of variable length columns contained in the record. Hold up - this doesn’t add up! The total number of columns was two, and we’ve got a single fixed length column. So how can there be two variable length columns, adding up to a total of three columns? This extra variable length column is the special back pointer field, as we’ll look at in just a bit.</font>    </font></li>
	<li><font face="Lucida Sans Unicode"><font color="#9b00d3">**9913a393  **</font><font color="#000000">The next four bytes, in pairs of two, is the variable length column offset array. They hold the offsets (and thus the length) of each variable length field. The first offset is 0x1399 = 5.017. The second offset is a bit more tricky. 0x93a3 has a decimal value of 37.795, clearly above the valid threshold. If we convert that value to binary, we get a value of 0b1001001110100011. No variable column length offset will ever need the high order bit and it’s thus used as an indicator for a pointer column – just like it’s used to indicate a row-overflow pointer. If we flip the high order bit, we get a value of 0b0001001110100011 = 5.027. Subtracting 5.017 from 5.027 gives us a column size of 10 bytes – the size of the back pointer.</font>    </font></li>
	<li><font face="Lucida Sans Unicode"><font color="#008000">**5.000 x 0x62  **</font><font color="#000000">I’ve snipped the next 5.000 bytes as those are just 5.000 ‘b’s repeated – the data in the Data column.</font></font></li>
</ul>

## The back pointer format

<font color="#000000">The remaining 10 bytes make up the back pointer:</font>

<font color="#a5a5a5">**32000800 02000000 02000002 009913a3 93 &lt;snip 5.000 x 0x62&gt; ****<font color="#ff0000">0004</font><font color="#0000ff">7200 0000</font><font color="#9b00d3">0100</font> <font color="#008000">0100</font>**</font>

<ul>
	<li><font color="#000000"><font face="Lucida Sans Unicode"><font color="#ff0000">**0004  **</font>The first two bytes indicates the special column ID with a decimal value of 1.024, indicating that it’s a back pointer.    </font></font></li>
	<li><font color="#000000"><font face="Lucida Sans Unicode"><font color="#0000ff">**72000000  **</font>Is the beginning of the back pointer page location. 0x72 = 114 in decimal, which is the page ID of the referencing forwarding stub.    </font></font></li>
	<li><font color="#000000"><font face="Lucida Sans Unicode"><font color="#9b00d3">**0100  **</font>Indicates the file ID with a decimal value of 1.    </font></font></li>
	<li><font color="#000000"><font face="Lucida Sans Unicode"><font color="#008000">**0100  **</font>Finally the last two bytes indicates the slot number with a decimal value of 1.</font></font></li>
</ul>

<font color="#000000" />

<font color="#000000">And so, with the above, we’ve now parsed the complete forwarded record. We can conclude that the back pointer takes up a total of 10 bytes, in this case pointing to the slot (<font color="#9b00d3">**1**</font>:<font color="#0000ff">**114**</font>:<font color="#008000">**1**</font>)</font>
