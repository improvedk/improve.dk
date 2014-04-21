---
permalink: migrating-from-wordpress-to-hexo
title: Migrating from Wordpress to Hexo
date: 2014-04-19
tags: [Miscellaneous]
---
It's this time of the year again - the time to migrate from one blog engine to another.

<!-- more -->

About a year ago, I migrated from [Subtext](http://subtextproject.com/) to [Wordpress](http://wordpress.org/). While I was initially happy, I still wasn't completely satisfied with the workflow. My primary peeves were:

* Complexity - I had to pay a host to run a stack consisting of PHP and MySQL and keep it updated.
* Security - I needed to constantly keep watch over Wordpress and keep it updated, seeing as it's a popular target for mass defacements, etc.
* Backups - While I did run an automated backup plugin, it was cumbersome as I needed an offsite location (i used FTP).
* Writing - While the WYSIWYG editor works for some, it didn't for me. As such I ended up writing all my posts in pure HTML.
* Openness - I'm a big proponent of open source and while I did publish the source code for my [custom Wordpress theme](https://github.com/improvedk/improve.dk_Wordpress), I wanted to also open up my blog posts themselves.
* Speed - I've spent more time than I'd like to, just keeping Wordpress running smoothly. A lot of things were outside of my control though, seeing as performance optimization was typically relegated to third party plugins.

While considering the above list, I ended up settling on [Hexo](http://hexo.io) - a static site generator powered by [Node.js](http://nodejs.org).

## Migration
The migration process was simple enough, though it required some manual labor. All my Wordpress posts are written in HTML and since Hexo posts are based on Markdown, they needed to be converted. After dumping my old Wordpress site into a backup XML file, I was able to [write a script](https://github.com/improvedk/improve.dk/blob/master/WP%20Conversion.linq) that parsed the backup XML file and converted each post into the Hexo Markdown format. There were some misses that required manual intervention, seeing as I had invalid HTML, special cases, etc. But overall, 95% of the posts were converted automatically.

Since Hexo is a static site generator, I needed to host my comments offsite. Thankfully [Disqus](http://disqus.com/) has native support for the Wordpress comment backup format so importing the comments was a breeze.

Hexo does not support storing assets and posts in folders but prefers to store posts and assets seperately. As I like to keep them together (seeing as I've got close to 300 posts), I had to write a small script that copied the assets into the right output locations:

```js
var fs = require('fs');
var path = require('path');
var publicDir = hexo.public_dir;
var sourceDir = hexo.source_dir;
var postsDir = path.join(sourceDir, '_posts');
var htmlTag = hexo.util.html_tag;
var route = hexo.route;

// Stores assets that'll need to be copied to the post output folders
var filesToCopy = [];

// After Hexo's done generating, we'll copy post assets to their public folderse
hexo.on('generateAfter', function() {
	filesToCopy.forEach(function(obj) {
		fs.writeFileSync(obj.destination, fs.readFileSync(obj.source));
	});
});

// Each time a post is rendered, note that we need to copy its assets
hexo.extend.filter.register('post', function(data, cb) {
	if (data.slug) {
		var postDir = path.join(postsDir, data.slug);
		var files = fs.readdirSync(postDir);

		files.forEach(function(file) {
			// Skip the markdown files themselves
			if (path.extname(file) == '.md')
				return;

			var outputDir = path.join(publicDir, data.slug);
			var outputPath = path.join(publicDir, data.slug, file);
			var inputPath = path.join(postDir, file);

			if (!fs.existsSync(outputDir))
				fs.mkdirSync(path.join(outputDir));
			
			filesToCopy.push({ source: inputPath, destination: outputPath });
		});
	}

	cb();
});
```

Though Hexo has a number of helpers to easily insert image links, I prefer to be able to just write an image name on a line by itself and then have the asset link inserted. Enabling that was easy enough too:

```js
// Replaces lines with image names with the actual image markup
hexo.extend.filter.register('pre', function(data, cb) {
	// Find all matching image tags
	var regex = new RegExp(/^([a-z_0-9\-\.]+(?:.jpg|png|gif))(?: ([a-z]+)( \d+)?)?$/gim);
	
	data.content = data.content.replace(regex, function(match, file, type, maxHeight) {
		// Create image link
		var imgLink;
		if (data.slug) // Posts need to reference image absolutely
			imgLink = '/' + data.slug + '/' + file;
		else
			imgLink = file;

		// Max height of image
		var imgMaxHeight = '250px';
		if (maxHeight)
			imgMaxHeight = maxHeight + 'px';

		// Set style depending on type
		var style = '';
		if (type) {
			switch (type) {
				case 'right':
					style = 'float: right; margin: 20px';
					break;

				case 'left':
					style = 'float: left';
					break;
			}
		}

		return '<div class="imgwrapper" style="' + style + '"><div><a href="' + imgLink + '" class="fancy"><img src="' + imgLink + '" style="max-height: ' + imgMaxHeight + '"/></a></div></div>';
	});
	
	// Let hexo continue
	cb();
});
```

##Hosting, Security, Backup & Speed
Due to its static nature, there are no logins to protect, per se - seeing as there's no backend. The blog itself is hosted on Github, both the [source](https://github.com/improvedk/improve.dk) as well as the statically generated [output files](https://github.com/improvedk/improvedk.github.io). This means I've got full backup in the form of distributed git repositories, as well as very easy rollback in case of mistakes.

As for speed, it doesn't get much faster than serving static files. Comments are lazily loaded after the post itself is loaded. While I can't utilize the Github CDN (seeing as I'm hosting the blog at an apex domain, making it impossible for me to setup a CNAME - which is required to use the Github CDN), the speed is way faster than it used to be on Wordpress. I could move my DNS to a registrar that supports apex aliasing, but I'm happy with the speed for now.

##Openness
Finally, since the source for the blog itself is hosted on Github, including the posts themselves, each post is actually editable directly on Github. You'll notice that I've added an Octocat link at the bottom of each post, next to the social sharing icons. Clicking the Octocat will lead you directly to the source of the post you're looking at. If you find an error or have a suggestion for an edit, feel free to fork the post and submit a pull request.