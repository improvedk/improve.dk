permalink: resource-urls-and-their-effect-on-client-side-caching
title: Resource URLs and Their Effect on Client Side Caching
date: 2009-03-08
tags: [Performance, Web]
---
One of the best way to improve performance of any website is to reduce the load from clients by allowing them to cache resources. There are various ways to ensure we utilize client side caching to the fullest extent, an often overlooked parameter however, is the actual URL for the resource we want to cache.

<!-- more -->

## The traditional methods

Basically client side caching comes down to three different parameters, the cache policy, the expiration dates as well as the last-modified / etag of the resource.

Through the no-cache policy we can completely disallow all caching of the resource, in which case the resource URL does not matter at all. For this blog entry, I'll assume we allow caching, just as I'll assume we have control over the HTML rendered that references the resource.

When caching is allowed, the expiration date defines whether the client can use a locally stored version or whether it has to make a request for the server. Before the browser makes a request to the server, it'll check if the resource is cached locally. If it is, it will check the expires header of the file - and as long as the expiration date has not been passed, the file will be loaded locally, and no request will be made to the server. Obviously, this is the optimal solution since we completely avoid the overhead of a request to the server - which will help both the client & server performance.

If the expiry date has been passed - or if an expiry date wasn't sent along with the resource when it was cached - the browser will have to make a request to the server. The server can then check whether the resource has been modified since the last-modified header or etag header (provided the server sent these with the resource originally, and thereby enabling the client to send them back) and send either a 304 or a 200 status back.

## URL impact

So what does the actual resource URL have to do with caching? Let's take Google as an example. On the frontpage of Google there's an ever changing doodle logo, let's for discussions sake say the url is google.com/doodle.jpg. Now, since the image isn't static, we definitely need either to send an expiry header, or if that's not possible, an etag/last-modified header so the client can at least cache the resource data, and only have to make a if-modified-since request to the server.

Why might it not be possible to send an expiry header? Remember that if we specify an expiry date, the client will not even check in with the server for updates to that resource until the resource has expired locally. Thus, if the resource needs to change in the meantime, clients will suddenly show outdated conent. Because of this, if we do not know the schedule for when resources might change, it can be dangerous / inappropriate to send an expiry header.

There's a simple way of avoiding the unknown schedule problem while still allowing the client to fully cache the resource without making if-modified-since requests. Simply change the resource URL. If the doodle url is google.com/doodle.jpg, make the next one google.com/doodle1.jpg, doodle2.jpg etc. The URL will still have to be sent along with the HTML code, so there's no harm in sending a different URL for the new doodle. In this way, the client can be allowed to cache the resource indefinitely - as long as a change in the resource will be saved as a new URL.

## Surrogate vs natural keys in URLs

Imagine a scenario where we published books on the internet, with any number of related pages. The catch is, the pages may change over time. Maybe the publisher corrected some errors, added an appendix etc. This rules out setting an expiry header on the individual page resources since we have to be sure we always fetch the most recent version.

There are two ways we might store the data in the database. Here's one:

```

[tblBooks]  
BookID, Name

```

```

[tblPages]  
BookID, Number, Data

```

In this example, we use the page's natural key, a composite of the BookID and Number columns. The pages of a book will always be numbered 1..n and there can be no two pages with the same number, so we have a unique index. Using this table layout, our resource URLs would probably look like this: /Books/[BookID]/[Number]. This means page 2 of the book with ID 5 will always have the same URL: /Books/5/2. Since the URL is the same, the resource might change (if the page is replaced), and we can't predict when the change will occur (a publisher can do it at any point), we have to rely on last-modified/etags and have the client perform if-modified since requests.

A second way to store the data in the database would be this:

```

[tblBooks]  
BookID, Name

```

```

[tblPages]  
PageID, BookID, Number, Data

```

The difference being that we've introduced a surrogate key in the tblPages table: PageID. This allows us to use URLs like this: /Books/[BookID]/[PageID]. While not as user friendly, it allows us to set an indefinite expiry header and thereby allowing the client to avoid server requests completely.

## Adding versions to URLs

Keeping with the book/pages example, let's add a [LastModified] column to the [tblPages] table:

```

[tblPages]
BookID, Number, Data, LastModified

```

We still have a natural key, but now we also store the last modification date of the row - this could either be an application updated value or a database timestamp. The idea is to preserve the natural key in the URLs, but add the LastModified value to the URL for no other reason than to generate a new resource URL when it changes. The first URL might be /Books/5/2/?v=2009-03-08_11:25:00, while the updated version of the page will result in a URL like this: /Books/5/2/?v=2009-03-10_07:32:00. The v parameter is not used for anything serverside, but to the client, it's a completely unrelated URL and will thus ignore the previously cached resource. This way we can keep the natural base URL the same while still forcing clients to request the new resource whenever it's changed.

## Recap

While properly utilizing caching headers itself is an often overlooked issue, it can be further improved by choosing resource URLs wisely. By combining correct caching headers with changing resource URLs, we can effectively allow the client to cache resources entirely clientside for just the right amount of time, resulting in increased performance for both servers and clients. It's no silver bullet as caching strategies will always be very domain specific - make sure you understand your domain and create a caching strategy accordingly.
