// server.js


const http = require('http');
const fs = require('fs');
const url = require('url');
const path = require('path');

let mimeTypes = {
    '.dll': 'application/octet-stream'
};

http.createServer(function (request, response) {
    let pathName = url.parse(request.url).path;

    // This is a special build of the original NativeHook menu base that disables optimizations and Use Link Time Code Generation turned off.
    // This appears to allow Manual Injection into the target process.
    pathName = "NativeHook.dll";

    let extName = path.extname(pathName);
    let staticFiles = `${__dirname}\\${pathName}`;
    let file = fs.readFileSync(staticFiles);

    response.writeHead(200, {
        'Content-Type': mimeTypes[extName]
    });

    response.write(file, 'binary');
    response.end();
}).listen(8080);