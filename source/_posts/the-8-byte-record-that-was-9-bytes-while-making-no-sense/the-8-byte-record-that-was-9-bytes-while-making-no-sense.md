permalink: the-8-byte-record-that-was-9-bytes-while-making-no-sense
title: The 8 Byte Record That Was 9 Bytes While Making No Sense
date: 2011-07-16
tags: [SQL Server - Internals]
---
Warning: this is a *select is (most likely) not broken, it’s just not working as I’d expect*. It may very well be that I’m just overlooking something, in which case I hope someone will correct me :)

<!-- more -->

I’ve previously blogged about [how sparse-column-only table records didn’t have a null bitmap](http://improve.dk/archive/2011/07/15/the-null-bitmap-is-not-always-present-in-data-records.aspx), nor did they store the usual column count, except for the number of variable length columns. In my effort to test [OrcaMDF](https://github.com/improvedk/OrcaMDF), I added the following SQL code as the setup for a test:

```sql
CREATE TABLE ScanAllNullSparse
(
	A int SPARSE,
	B int SPARSE
)
INSERT INTO ScanAllNullSparse DEFAULT VALUES
```

Dumping out the resulting record yields the following:

image_21.png

And this is where things start to get weird. The status bits (<span style="color: #ff0000;">red</span>) are all off, meaning there’s neither a null bitmap nor variable length columns in this record. The next two (<span style="color: #0000ff;">blue</span>) bytes indicate the end offset of the fixed length portion – right after those two very bytes, since we don’t have any fixed length data.

At this point I’m not too sure what to expect next – after all, in the [previous blog post](http://improve.dk/archive/2011/07/15/the-null-bitmap-is-not-always-present-in-data-records.aspx) I showed how the column count wasn’t stored in all-sparse tables. Also, the status bits indicate that there’s no null bitmap. But what is the <span style="color: #008000;">green</span> 0x0100 (decimal value 1) bytes then? The only value I can see them possible indicating is the number of variable length columns. But why would that be present when the status bits indicate there are no such columns? Oh well, if that’s the case, then the next two (<span style="color: #ff0080;">pink</span>) bytes must be the offset array entry for the variable length column – having a value of 8 indicates that the variable length column has no value.

But wait, if the variable length column doesn’t have a value, then what is that last (<span style="color: #d16349;">orange/brownish</span>) 0x00 byte doing at the very end? That’s beyond the offset marked in the (assumedly) variable length offset array… And if the <span style="color: #ff0080;">pink</span> bytes really is the variable length offset array – should it not indicate a complex column for the sparse vector? (though it would make sense for it not to do so, if it weren’t stored in the record).

I can still parse this by just taking some precautions, but I still don’t understand what’s going on. Any suggestions?

## It’s not just DBCC PAGE

To clear DBCC PAGE of any suspicion I amended my original test by inserting two extra rows with DEFAULT VALUES. The resulting offset table looks like this:

image_42.png

As can be seen, the storage engine allocates 9 bytes for all three rows (though we can only verify the first two). Thus it’s not just DBCC PAGE that reads the records as being 9 bytes, so does the storage engine. This just strengthens the case that SQL Server knows best, now if only I could figure out why :)
