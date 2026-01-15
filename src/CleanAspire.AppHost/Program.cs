var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL database for the application
var postgresServer = builder
    .AddPostgres("postgresserver")
    .WithPgAdmin()
    .WithDataVolume()
    .WithImage("postgres", "16"); // Use PostgreSQL 16

var postgresDatabase = postgresServer.AddDatabase("postgresdb");

var apiService = builder
    .AddProject<Projects.CleanAspire_Api>("apiservice")
    .WithReference(postgresDatabase)
    // Configure database settings to use PostgreSQL
    // WithReference automatically creates ConnectionStrings:postgresdb variable
    // Map it to DatabaseSettings__ConnectionString for the application
    .WithEnvironment("DatabaseSettings__DBProvider", "postgresql")
    .WithEnvironment("DatabaseSettings__ConnectionString", postgresDatabase.GetConnectionString())
    .WaitFor(postgresDatabase);

builder
    .AddProject<Projects.CleanAspire_WebApp>("blazorweb")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

// Blazor WebAssembly Standalone project.
// This project runs as a standalone WASM application that connects to the API server.
var clientWebWasmProject = builder
    .AddProject<Projects.CleanAspire_ClientAppHost>("blazorwasm")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    // Configure ServiceBaseUrl environment variable to point to the API server
    // This will override the default ServiceBaseUrl from appsettings.json
    .WithEnvironment("ClientAppSettings__ServiceBaseUrl", apiService.GetEndpoint("http"))
    .WithExplicitStart() // Requires manual start from Aspire Dashboard
    .WaitFor(apiService);

// Adding health checks endpoints to applications in non-development environments has security implications.
// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
if (builder.Environment.EnvironmentName == "Development")
{
    clientWebWasmProject.WithHttpHealthCheck("/");
}

builder.Build().Run();
