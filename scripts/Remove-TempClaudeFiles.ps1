# Скрипт для удаления всех временных файлов tmpclaude-*-cwd из проекта
# Использование: .\scripts\Remove-TempClaudeFiles.ps1

$ErrorActionPreference = "Stop"

# Получаем корневую директорию проекта (родительская папка scripts)
$projectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Поиск временных файлов tmpclaude-*-cwd в проекте..." -ForegroundColor Cyan
Write-Host "Корневая директория: $projectRoot" -ForegroundColor Gray

# Рекурсивно ищем все файлы, соответствующие паттерну
$files = Get-ChildItem -Path $projectRoot -Filter "tmpclaude-*-cwd" -File -Recurse -ErrorAction SilentlyContinue

if ($files.Count -eq 0) {
    Write-Host "Временные файлы не найдены." -ForegroundColor Green
    exit 0
}

Write-Host "`nНайдено файлов: $($files.Count)" -ForegroundColor Yellow
foreach ($file in $files) {
    Write-Host "  - $($file.FullName)" -ForegroundColor Gray
}

Write-Host "`nУдаление файлов..." -ForegroundColor Cyan

$deletedCount = 0
foreach ($file in $files) {
    try {
        Remove-Item -Path $file.FullName -Force
        Write-Host "  Удален: $($file.FullName)" -ForegroundColor Green
        $deletedCount++
    }
    catch {
        Write-Host "  Ошибка при удалении $($file.FullName): $_" -ForegroundColor Red
    }
}

Write-Host "`nГотово! Удалено файлов: $deletedCount из $($files.Count)" -ForegroundColor Green
