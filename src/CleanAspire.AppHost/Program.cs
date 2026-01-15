// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database for the application
var postgresDatabase = builder
    .AddPostgres("postgresserver")
    .WithPgAdmin(config => config.WithVolume("/var/lib/pgadmin/BitDashboardTemplate/data"))
    .WithImage("pgvector/pgvector", "pg18") // pgvector supports embedded vector search.
    .AddDatabase("postgresdb");

// Redis cache for distributed caching
var redisCache = builder.AddRedis("rediscache");

// Blazor WebAssembly Standalone project - declare first to get endpoint reference
var clientWebWasmProject = builder
    .AddProject<Projects.CleanAspire_ClientAppHost>("blazorwasm")
    .WithExternalHttpEndpoints();

var apiService = builder
    .AddProject<Projects.CleanAspire_Api>("apiservice")
    .WithReference(postgresDatabase)
    .WithReference(redisCache)
    // Configure database settings to use PostgreSQL
    // WithReference automatically creates ConnectionStrings:postgresdb variable
    // Map it to DatabaseSettings__ConnectionString using the reference expression
    .WithEnvironment("DatabaseSettings__DBProvider", "postgresql")
    .WithEnvironment(
        "DatabaseSettings__ConnectionString",
        postgresDatabase.Resource.ConnectionStringExpression
    )
    // Configure Redis settings
    .WithEnvironment("Redis__Enabled", "true")
    .WithEnvironment("Redis__ConnectionString", redisCache.Resource.ConnectionStringExpression)
    // Add WASM client origin to allowed CORS origins
    .WithEnvironment(
        "AllowedCorsOrigins",
        ReferenceExpression.Create(
            $"https://localhost:7341,https://localhost:7123,{clientWebWasmProject.GetEndpoint("https")}"
        )
    )
    .WaitFor(postgresDatabase)
    .WaitFor(redisCache);

builder
    .AddProject<Projects.CleanAspire_WebApp>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithExplicitStart() // Requires manual start from Aspire Dashboard
    .WaitFor(apiService);

// Configure WASM client to point to API and wait for it to be ready
clientWebWasmProject
    .WithReference(apiService)
    .WithEnvironment("ClientAppSettings__ServiceBaseUrl", apiService.GetEndpoint("http"))
    .WaitFor(apiService);

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.EnvironmentName == "Development")
{
    clientWebWasmProject.WithHttpHealthCheck("/");
}

builder.Build().Run();
