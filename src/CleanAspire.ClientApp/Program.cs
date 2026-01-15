// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.ClientApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#if STANDALONE
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
#endif

// register the cookie handler
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHttpClients();
builder.Services.AddAuthenticationAndLocalization(builder.Configuration);
var app = builder.Build();

await app.InitializeCultureAsync();

await app.RunAsync();
