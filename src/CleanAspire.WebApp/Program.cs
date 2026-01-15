// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.ClientApp;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients();
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);

// Configure static file MIME types for Blazor WASM
builder.Services.Configure<StaticFileOptions>(options =>
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".wasm"] = "application/wasm";
    provider.Mappings[".blat"] = "application/octet-stream";
    provider.Mappings[".dat"] = "application/octet-stream";
    provider.Mappings[".dll"] = "application/octet-stream";
    provider.Mappings[".pdb"] = "application/octet-stream";
    provider.Mappings[".json"] = "application/json";
    provider.Mappings[".js"] = "application/javascript";
    provider.Mappings[".mjs"] = "application/javascript";
    options.ContentTypeProvider = provider;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Configure static files with proper MIME types for Blazor WASM
var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".wasm"] = "application/wasm",
            [".blat"] = "application/octet-stream",
            [".dat"] = "application/octet-stream",
            [".dll"] = "application/octet-stream",
            [".pdb"] = "application/octet-stream",
            [".json"] = "application/json",
            [".js"] = "application/javascript",
            [".mjs"] = "application/javascript",
        },
    },
};
app.UseStaticFiles(staticFileOptions);

app.MapStaticAssets();
app.MapRazorComponents<CleanAspire.WebApp.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CleanAspire.ClientApp._Imports).Assembly);

app.Run();
