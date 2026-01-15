// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Enhance console logging with formatted output
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = false;
    options.IncludeScopes = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.UseUtcTimestamp = false;
});

var app = builder.Build();

// Log startup information
var logger = app.Logger;
logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("🚀 CleanAspire ClientAppHost - Starting application...");
logger.LogInformation("═══════════════════════════════════════════════════════════");

// Get the path to the published ClientApp wwwroot
var basePath = app.Environment.ContentRootPath;
var clientAppPath = "";
var frameworkPath = "";
var contentPath = "";

// Try different possible paths - prioritize source wwwroot for development
var possiblePaths = new[]
{
    // Source wwwroot (contains index.html and static assets)
    Path.Combine(basePath, "..", "..", "CleanAspire.ClientApp", "wwwroot"),
    Path.Combine(basePath, "..", "CleanAspire.ClientApp", "wwwroot"),
    // Bin wwwroot (contains _framework after build)
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
                Path.Combine(assemblyLocation, "..", "..", "CleanAspire.ClientApp", "wwwroot"),
                Path.Combine(assemblyLocation, "..", "CleanAspire.ClientApp", "wwwroot"),
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

// Find source wwwroot (with index.html)
foreach (var path in possiblePaths)
{
    var fullPath = Path.GetFullPath(path);
    var indexHtmlPath = Path.Combine(fullPath, "index.html");
    if (Directory.Exists(fullPath) && File.Exists(indexHtmlPath))
    {
        clientAppPath = fullPath;
        break;
    }
}

// Find framework path (with _framework folder)
foreach (var path in possiblePaths)
{
    var fullPath = Path.GetFullPath(path);
    var frameworkDir = Path.Combine(fullPath, "_framework");
    if (Directory.Exists(frameworkDir))
    {
        frameworkPath = fullPath;
        break;
    }
}

// Find _content path (with _content folder for library static assets like MudBlazor)
// This is typically only in the publish folder
var publishPaths = new[]
{
    Path.Combine(
        basePath,
        "..",
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Debug",
        "net10.0",
        "publish",
        "wwwroot"
    ),
    Path.Combine(
        basePath,
        "..",
        "CleanAspire.ClientApp",
        "bin",
        "Debug",
        "net10.0",
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

foreach (var path in publishPaths)
{
    var fullPath = Path.GetFullPath(path);
    var contentDir = Path.Combine(fullPath, "_content");
    if (Directory.Exists(contentDir))
    {
        contentPath = fullPath;
        break;
    }
}

// Log paths for debugging
logger.LogInformation("📁 Searching for ClientApp files...");
logger.LogInformation("   ClientApp wwwroot path: {Path}", clientAppPath);
logger.LogInformation("   Framework path: {Path}", frameworkPath);
logger.LogInformation("   Content path: {Path}", contentPath);

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

    // Create file provider - combine multiple paths for source, framework, and library content
    // IMPORTANT: Framework and content providers must come FIRST to ensure _framework and _content files are found
    // before falling back to source wwwroot files. CompositeFileProvider checks providers in order.
    IFileProvider fileProvider;
    var providers = new List<IFileProvider>();

    // Add framework provider FIRST if it's in a different location (contains _framework folder)
    if (
        !string.IsNullOrEmpty(frameworkPath)
        && frameworkPath != clientAppPath
        && Directory.Exists(frameworkPath)
    )
    {
        providers.Add(new PhysicalFileProvider(frameworkPath));
    }

    // Add content provider SECOND if it's in a different location (contains _content folder for library assets like MudBlazor)
    if (
        !string.IsNullOrEmpty(contentPath)
        && contentPath != clientAppPath
        && contentPath != frameworkPath
        && Directory.Exists(contentPath)
    )
    {
        providers.Add(new PhysicalFileProvider(contentPath));
    }

    // Add source wwwroot provider LAST (contains index.html and static assets)
    providers.Add(new PhysicalFileProvider(clientAppPath));

    if (providers.Count > 1)
    {
        fileProvider = new CompositeFileProvider(providers.ToArray());
        logger.LogInformation(
            "✅ Using composite file provider with {Count} paths: source={Source}, framework={Framework}, content={Content}",
            providers.Count,
            clientAppPath,
            frameworkPath,
            contentPath
        );
    }
    else
    {
        fileProvider = new PhysicalFileProvider(clientAppPath);
        logger.LogInformation("✅ Using single file provider: {Path}", clientAppPath);
    }

    // Enable default files (index.html) before static files
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });

    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider = fileProvider,
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                // Log requests to _framework files for debugging
                if (ctx.Context.Request.Path.Value?.Contains("_framework") == true)
                {
                    logger.LogInformation(
                        "📦 Serving _framework file: {Path} (Physical: {PhysicalPath})",
                        ctx.Context.Request.Path.Value,
                        ctx.File.PhysicalPath ?? "N/A"
                    );
                }

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
    // Use the same file provider as UseStaticFiles to ensure _framework files are accessible
    app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });
    logger.LogInformation("✅ Static file serving configured");
    logger.LogInformation("✅ SPA fallback routing configured");
    logger.LogInformation("═══════════════════════════════════════════════════════════");
    logger.LogInformation("✅ CleanAspire ClientAppHost - Application ready!");
    logger.LogInformation("═══════════════════════════════════════════════════════════");
}
else
{
    logger.LogError(
        "❌ Blazor WASM client files not found. Checked paths: {Paths}",
        string.Join(", ", possiblePaths.Select(p => Path.GetFullPath(p)))
    );
    app.MapGet(
        "/",
        () =>
            Results.Problem(
                $"Blazor WASM client files not found. Checked {possiblePaths.Length} possible paths.",
                statusCode: 500
            )
    );
}

app.Run();
