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
	data.content = data.content.replace(/^([a-z_0-9\.]+\.(jpg|png))$/gim,
		'<div class="imgwrapper"><div><a href="/' + data.slug + '/$1" class="fancy"><img src="/' + data.slug + '/$1" style="max-height: 300px"/></a></div></div>'
	);
	
	// Let hexo continue
	cb();
});