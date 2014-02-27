var fs = require('fs');
var path = require('path');
var publicDir = hexo.public_dir;
var sourceDir = hexo.source_dir;
var postsDir = path.join(sourceDir, '_posts');
var htmlTag = hexo.util.html_tag;
var route = hexo.route;

// Gets an array of files in dir or at some descending location
function getFilesRecursively(dir) {
    var results = [];
    var list = fs.readdirSync(dir);

    list.forEach(function(file) {
        file = dir + '\\' + file;
        var stat = fs.statSync(file);

        if (stat && stat.isDirectory())
        	results = results.concat(getFilesRecursively(file));
        else
        	results.push(file);
    });

    return results;
}

// After Hexo's done generating, we'll copy post assets to their public folderse
hexo.on('generateAfter', function() {
	var files = getFilesRecursively(postsDir)
		.filter(function(filePath) { return filePath.match(/\\_posts\\.*\.([^md]+)$/ig); })
		.map(function(filePath) {
			return {
				path: filePath,
				name: path.basename(filePath),
				postPermalink: path.basename(path.dirname(filePath))
			};
		});

	files.forEach(function(file) {
		var outputPath = path.join(publicDir, file.postPermalink, file.name);
		fs.writeFileSync(outputPath, fs.readFileSync(file.path));
	});
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