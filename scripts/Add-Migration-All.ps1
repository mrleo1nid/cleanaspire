# PowerShell script to create migrations for all database providers
# Usage: .\scripts\Add-Migration-All.ps1 -MigrationName "YourMigrationName"

param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProject = "src\CleanAspire.Api\CleanAspire.Api.csproj",
    
    [Parameter(Mandatory=$false)]
    [string]$Context = "ApplicationDbContext"
)

$ErrorActionPreference = "Stop"

# Check if dotnet ef tool is installed
Write-Host "Checking for dotnet ef tool..." -ForegroundColor Cyan
$efToolCheck = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ dotnet ef tool is not installed. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to install dotnet ef tool. Please install it manually:" -ForegroundColor Red
        Write-Host "  dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "✓ dotnet ef tool installed successfully" -ForegroundColor Green
} else {
    Write-Host "✓ dotnet ef tool is available" -ForegroundColor Green
}
Write-Host ""

Write-Host "Creating migrations for all database providers..." -ForegroundColor Cyan
Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow
Write-Host "Startup Project: $StartupProject" -ForegroundColor Yellow
Write-Host ""

# Define migration projects with their provider settings
$migrators = @(
    @{
        Name = "PostgreSQL"
        Project = "src\Migrators\Migrators.PostgreSQL\Migrators.PostgreSQL.csproj"
        DBProvider = "postgresql"
        ConnectionString = "Server=127.0.0.1;Database=CleanAspireDb;User Id=root;Password=root;Port=5432"
    },
    @{
        Name = "SQLite"
        Project = "src\Migrators\Migrators.SQLite\Migrators.SQLite.csproj"
        DBProvider = "sqlite"
        ConnectionString = "Data Source=CleanAspireDb.db"
    }
)

$successCount = 0
$failCount = 0

foreach ($migrator in $migrators) {
    Write-Host "Creating migration for $($migrator.Name)..." -ForegroundColor Green
    
    try {
        # Set environment variables to override appsettings.json
        $env:DatabaseSettings__DBProvider = $migrator.DBProvider
        $env:DatabaseSettings__ConnectionString = $migrator.ConnectionString
        
        # Build the migration project first to ensure it compiles
        Write-Host "  Building migration project..." -ForegroundColor Gray
        $buildCommand = "dotnet build `"$($migrator.Project)`" --no-incremental"
        $buildOutput = Invoke-Expression $buildCommand 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ✗ Build failed for migration project" -ForegroundColor Red
            Write-Host $buildOutput -ForegroundColor Red
            $failCount++
            continue
        }
        Write-Host "  ✓ Migration project build succeeded" -ForegroundColor Green
        
        # Build the startup project with environment variables set
        # Note: Build may fail due to database connection during OpenAPI generation, but that's OK
        Write-Host "  Building startup project..." -ForegroundColor Gray
        $startupBuildCommand = "dotnet build `"$StartupProject`" --no-incremental 2>&1"
        $startupBuildOutput = Invoke-Expression $startupBuildCommand
        $startupBuildExitCode = $LASTEXITCODE
        
        # Check if build failed due to database connection (expected) or other errors
        if ($startupBuildExitCode -ne 0) {
            # Check if it's just a database connection error (which is expected when DB is not running)
            if ($startupBuildOutput -match "Failed to connect|connection.*refused|NpgsqlException|SocketException.*5432") {
                Write-Host "  ⚠ Build failed due to database connection (expected if DB is not running)" -ForegroundColor Yellow
                Write-Host "  ℹ Continuing with migration creation..." -ForegroundColor Gray
            } else {
                # Show actual build errors
                Write-Host "  ✗ Build failed for startup project" -ForegroundColor Red
                Write-Host $startupBuildOutput -ForegroundColor Red
                Write-Host "  Tip: Check if all required database packages are installed" -ForegroundColor Yellow
                $failCount++
                continue
            }
        } else {
            Write-Host "  ✓ Startup project build succeeded" -ForegroundColor Green
        }
        
        # Create migration using --no-build to avoid re-running the build process
        # Connection string is read from environment variables
        $command = "dotnet ef migrations add $MigrationName " +
                   "--project `"$($migrator.Project)`" " +
                   "--startup-project `"$StartupProject`" " +
                   "--context $Context " +
                   "--output-dir Migrations " +
                   "--no-build"
        
        Write-Host "  Executing migration command..." -ForegroundColor Gray
        
        $migrationOutput = Invoke-Expression $command 2>&1
        $migrationExitCode = $LASTEXITCODE
        
        # Clear environment variables
        Remove-Item Env:\DatabaseSettings__DBProvider -ErrorAction SilentlyContinue
        Remove-Item Env:\DatabaseSettings__ConnectionString -ErrorAction SilentlyContinue
        
        if ($migrationExitCode -eq 0) {
            Write-Host "  ✓ Migration created successfully for $($migrator.Name)" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "  ✗ Failed to create migration for $($migrator.Name)" -ForegroundColor Red
            # Show output if there were errors
            if ($migrationOutput -match "error|failed|exception" -or $migrationOutput -match "Build failed") {
                Write-Host "  Error details:" -ForegroundColor Yellow
                Write-Host $migrationOutput -ForegroundColor Red
            }
            $failCount++
        }
    }
    catch {
        # Clear environment variables on error
        Remove-Item Env:\DatabaseSettings__DBProvider -ErrorAction SilentlyContinue
        Remove-Item Env:\DatabaseSettings__ConnectionString -ErrorAction SilentlyContinue
        
        Write-Host "  ✗ Error creating migration for $($migrator.Name): $_" -ForegroundColor Red
        $failCount++
    }
    
    Write-Host ""
}

Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })

if ($failCount -gt 0) {
    exit 1
}
