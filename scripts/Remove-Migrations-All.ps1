# PowerShell script to remove all migrations from all database provider projects
# Usage: .\scripts\Remove-Migrations-All.ps1 [-Confirm]

param(
    [Parameter(Mandatory=$false)]
    [switch]$Confirm
)

$ErrorActionPreference = "Stop"

Write-Host "Removing migrations from all database providers..." -ForegroundColor Cyan
Write-Host ""

# Define migration projects
$migrators = @(
    @{
        Name = "PostgreSQL"
        MigrationsPath = "src\Migrators\Migrators.PostgreSQL\Migrations"
    },
    @{
        Name = "SQLite"
        MigrationsPath = "src\Migrators\Migrators.SQLite\Migrations"
    }
)

$removedCount = 0
$errorCount = 0

foreach ($migrator in $migrators) {
    Write-Host "Processing $($migrator.Name)..." -ForegroundColor Green
    
    $migrationsPath = $migrator.MigrationsPath
    
    if (-not (Test-Path $migrationsPath)) {
        Write-Host "  ⚠ Migrations folder not found: $migrationsPath" -ForegroundColor Yellow
        continue
    }
    
    $migrationFiles = Get-ChildItem -Path $migrationsPath -File -ErrorAction SilentlyContinue
    
    if ($migrationFiles.Count -eq 0) {
        Write-Host "  ℹ No migration files found in $migrationsPath" -ForegroundColor Gray
        continue
    }
    
    Write-Host "  Found $($migrationFiles.Count) file(s) to remove:" -ForegroundColor Yellow
    foreach ($file in $migrationFiles) {
        Write-Host "    - $($file.Name)" -ForegroundColor Gray
    }
    
    if (-not $Confirm) {
        $response = Read-Host "  Do you want to remove these files? (Y/N)"
        if ($response -ne "Y" -and $response -ne "y") {
            Write-Host "  ✗ Skipped $($migrator.Name)" -ForegroundColor Yellow
            continue
        }
    }
    
    try {
        foreach ($file in $migrationFiles) {
            Remove-Item -Path $file.FullName -Force -ErrorAction Stop
            Write-Host "  ✓ Removed: $($file.Name)" -ForegroundColor Green
            $removedCount++
        }
        
        # Try to remove the Migrations folder if it's empty
        $remainingFiles = Get-ChildItem -Path $migrationsPath -File -ErrorAction SilentlyContinue
        if ($remainingFiles.Count -eq 0) {
            try {
                Remove-Item -Path $migrationsPath -Force -ErrorAction SilentlyContinue
                Write-Host "  ✓ Removed empty Migrations folder" -ForegroundColor Green
            } catch {
                # Ignore if folder can't be removed (might be needed by project structure)
            }
        }
        
        Write-Host "  ✓ Successfully removed all migrations for $($migrator.Name)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ Error removing migrations for $($migrator.Name): $_" -ForegroundColor Red
        $errorCount++
    }
    
    Write-Host ""
}

Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Files removed: $removedCount" -ForegroundColor Green
Write-Host "  Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })

if ($errorCount -gt 0) {
    Write-Host ""
    Write-Host "⚠ Some errors occurred during migration removal." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host ""
    Write-Host "✓ All migrations removed successfully!" -ForegroundColor Green
    Write-Host "  You can now create new migrations using:" -ForegroundColor Cyan
    Write-Host "  .\scripts\Add-Migration-All.ps1 -MigrationName `"YourMigrationName`"" -ForegroundColor Yellow
}
