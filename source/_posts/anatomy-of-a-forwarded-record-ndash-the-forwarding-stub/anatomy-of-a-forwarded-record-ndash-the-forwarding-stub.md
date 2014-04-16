permalink: anatomy-of-a-forwarded-record-ndash-the-forwarding-stub
title: Anatomy of a Forwarded Record & the Forwarding Stub
date: 2011-06-07
tags: [SQL Server - Internals]
---
A forwarded record occurs whenever a record in a [heap](http://msdn.microsoft.com/en-us/library/ms188270.aspx) increases in size and it no longer fits on the page. Instead of causing a page split, as would happen had the table not been a heap, the record is moved onto another with enough free space, or onto a newly allocated page. Forwarded records can wreak havoc to your performance due to fragmentation, but I'll leave not cover that here as many other more skilled people [have](http://sqlblog.com/blogs/kalen_delaney/archive/2009/11/11/fragmentation-and-forwarded-records-in-a-heap.aspx) [already](http://blogs.msdn.com/b/mssqlisv/archive/2006/12/01/knowing-about-forwarded-records-can-help-diagnose-hard-to-find-performance-issues.aspx) [done](http://www.sqlskills.com/BLOGS/PAUL/post/Forwarding-and-forwarded-records-and-the-back-pointer-size.aspx) [so](http://blogs.msdn.com/b/sqlserverstorageengine/archive/2006/09/19/761437.aspx).

<!-- more -->

## Test setup

As a test table we'll use a simple table with three wide records, taking up almost a full page of data.

```sql
-- Create test table
CREATE TABLE ForwardedRecordTest
(
	ID int identity,
	Data varchar(8000)
)

-- Insert dummy data
INSERT INTO
	ForwardedRecordTest (Data)
VALUES 
	(REPLICATE('a', 2000)),
	(REPLICATE('b', 2000)),
	(REPLICATE('c', 2000))
```

Firing up DBCC IND shows us a single IAM page tracking a single data page:

```sql
DBCC IND (Test, ForwardedRecordTest, -1)
```

image_21.png

Now, to force a forwarded record, we'll update one of the columns so it'll no longer fit on the page with the other records:

```sql
UPDATE
	ForwardedRecordTest
SET
	Data = REPLICATE('b', 5000)
WHERE
	Data = REPLICATE('b', 2000)
```

Invoking DBCC IND again confirms that a new page has been allocated to our table:

image_41.png

## The FORWARDING_STUB

By using DBCC PAGE we can take a look at the forwarded recorded, or at least what's left of it on the original page 114:

```sql
DBCC TRACEON (3604)
DBCC PAGE (Test, 1, 114, 3)
```

image_6.png

Identifying a forwarding stub is done by looking at the first status byte of the record. Specifically, bits 1-3 will tell us the record type:

```sql
Type = (RecordType)((Convert.ToByte(bits[1]) << 2) + (Convert.ToByte(bits[2]) << 1) + Convert.ToByte(bits[3]));
```

With Type being one of the valid SQL Server record types:

```sql
public enum RecordType : byte
{
	Primary = 0,
	Forwarded = 1,
	ForwardingStub = 2,
	Index = 3,
	BlobFragment = 4,
	GhostIndex = 5,
	GhostData = 6,
	GhostVersion = 7
}
```

While other record types will have two status bytes, a forwarding stub only has a single status byte. Thus, if we identify the record to be a forwarding stub, we know that the next 8 bytes will be a page pointer.

## Parsing the forwarding stub

Once we know the format, parsing a forwarding stub record is straight forward.

The first byte has a value of 0x04, or 0b100 in binary. Looking at bits 1-3 we get 0b10 or decimal 2 – which matches RecordType.ForwardingStub.

Looking at the next 8 bytes we have <span style="color: #ff0000;">76000000</span> <span style="color: #0000ff;">0001</span> <span style="color: #9b00d3;">0000</span>. I've divided them into three groups – in order they contain the page ID, the file ID and finally the slot number.

<span style="color: #ff0000;">76000000</span> byte swapped = 0x76 = 118 in decimal.

<span style="color: #0000ff;">0001</span> byte swapped = 0x01 = 1 in decimal.

<span style="color: #9b00d3;">0000</span> byte swapped = 0x00 = 0 in decimal.

Thus we have the position of the forwarded record: (<span style="color: #0000ff;">1</span>:<span style="color: #ff0000;">118</span>:<span style="color: #9b00d3;">0</span>).
