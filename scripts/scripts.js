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

// Replaces lines with image names with the actual image markup
hexo.extend.filter.register('pre', function(data, cb) {
	// Find all matching image tags
	var regex = new RegExp(/^([a-z_0-9\.]+(?:.jpg|png))(?: ([a-z]+)( \d+)?)?$/gim);
	
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