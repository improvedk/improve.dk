permalink: converting-page-pointers-into-a-human-readable-format
title: Converting Page Pointers Into a Humanly Readable Format
date: 2011-04-28
tags: [SQL Server - Internals]
---
I often like to spend my weekends perusing the sys.system_internals_allocation_units table, looking for the remnants of Frodo and his crew. In the sys.system_internals_allocation_units there are several references to relevant pages:

<pre lang="tsql" escaped="true">select
	first_page,
	root_page,
	first_iam_page
from
	sys.system_internals_allocation_units</pre>

image_2.png

Once you get used to reading these pointers, it becomes rather trivial – byte swap the last two pointers to get the file ID (0 or 1 in all of the above rows), and byte swap the first four bytes to get the page ID. To make it a bit more easier for myself and for those who do not read HEX natively, I’ve made a simple function to convert the pointers into a more easily read format.

<pre lang="tsql" escaped="true">create function getPageLocationFromPointer
(
	@Pointer binary(6)
)
returns varchar(17)
begin

	return 
		'(' + 
		cast(
			cast(
				substring(@Pointer, 6, 1) +
				substring(@Pointer, 5, 1)
				as smallint
			) as varchar
		) +
		':' +
		cast(
			cast(
				substring(@Pointer, 4, 1) +
				substring(@Pointer, 3, 1) +
				substring(@Pointer, 2, 1) +
				substring(@Pointer, 1, 1)
				as int
			) as varchar
		) +
		')'

end</pre>

While not beautiful, it is rather simple. The result:

<pre lang="tsql" escaped="true">select
	first_page,
	dbo.getPageLocationFromPointer(first_page) as first_page_location,
	root_page,
	dbo.getPageLocationFromPointer(root_page) as root_page_location,
	first_iam_page,
	dbo.getPageLocationFromPointer(first_iam_page) as first_iam_page_location
from
	sys.system_internals_allocation_units</pre>

image_4.png
