// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();

// Get the path to the published ClientApp wwwroot
var basePath = app.Environment.ContentRootPath;
var clientAppPath = "";

// Try different possible paths
var possiblePaths = new[]
{
    Path.Combine(
        basePath,
        "..",
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Debug",
        "net10.0",
        "wwwroot"
    ),
    Path.Combine(
        basePath,
        "..",
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Release",
        "net10.0",
        "browser-wasm",
        "publish",
        "wwwroot"
    ),
    Path.Combine(
        basePath,
        "..",
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Release",
        "net10.0",
        "publish",
        "wwwroot"
    ),
    Path.Combine(basePath, "..", "CleanAspire.ClientApp", "bin", "Debug", "net10.0", "wwwroot"),
    Path.Combine(
        basePath,
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Release",
        "net10.0",
        "browser-wasm",
        "publish",
        "wwwroot"
    ),
    Path.Combine(
        basePath,
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Release",
        "net10.0",
        "publish",
        "wwwroot"
    ),
};

// Try relative to executable location
var assemblyLocation =
    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
if (!string.IsNullOrEmpty(assemblyLocation))
{
    possiblePaths = possiblePaths
        .Concat(
            new[]
            {
                Path.Combine(
                    assemblyLocation,
                    "..",
                    "..",
                    "CleanAspire.ClientApp",
                    "bin",
                    "Release",
                    "net10.0",
                    "browser-wasm",
                    "publish",
                    "wwwroot"
                ),
                Path.Combine(
                    assemblyLocation,
                    "..",
                    "CleanAspire.ClientApp",
                    "bin",
                    "Release",
                    "net10.0",
                    "browser-wasm",
                    "publish",
                    "wwwroot"
                ),
            }
        )
        .ToArray();
}

foreach (var path in possiblePaths)
{
    var fullPath = Path.GetFullPath(path);
    if (Directory.Exists(fullPath))
    {
        clientAppPath = fullPath;
        break;
    }
}

if (Directory.Exists(clientAppPath))
{
    // Configure static file serving with proper MIME types
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".wasm"] = "application/wasm";
    provider.Mappings[".blat"] = "application/octet-stream";
    provider.Mappings[".dat"] = "application/octet-stream";
    provider.Mappings[".dll"] = "application/octet-stream";
    provider.Mappings[".pdb"] = "application/octet-stream";
    provider.Mappings[".json"] = "application/json";
    provider.Mappings[".js"] = "application/javascript";
    provider.Mappings[".mjs"] = "application/javascript";

    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                clientAppPath
            ),
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                // Cache static assets
                if (
                    ctx.File.Name.EndsWith(".wasm")
                    || ctx.File.Name.EndsWith(".blat")
                    || ctx.File.Name.EndsWith(".dll")
                )
                {
                    ctx.Context.Response.Headers.Append(
                        "Cache-Control",
                        "public, max-age=31536000"
                    );
                }
            },
        }
    );

    // Fallback to index.html for SPA routing
    app.MapFallbackToFile(
        "index.html",
        new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                clientAppPath
            ),
        }
    );
}
else
{
    app.MapGet(
        "/",
        () =>
            Results.Problem(
                $"Blazor WASM client files not found. Expected path: {clientAppPath}",
                statusCode: 500
            )
    );
}

app.Run();
