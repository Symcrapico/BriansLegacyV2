# ============================================================================
# check-active-task.ps1
# Purpose: BLOCK editing code without an active task (Windows)
# Dependencies: BACKLOG.md
# ============================================================================

# Skip check if no BACKLOG.md (new project setup)
if (-not (Test-Path "BACKLOG.md")) {
    exit 0
}

# Read BACKLOG.md and check for active task
$content = Get-Content "BACKLOG.md" -Raw
$activeMatch = [regex]::Match($content, '^> Active: (.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
$active = if ($activeMatch.Success) { $activeMatch.Groups[1].Value.Trim() } else { "None" }

if ($active -eq "None" -or [string]::IsNullOrEmpty($active)) {
    Write-Host ""
    Write-Host "X BLOCKED: No active task" -ForegroundColor Red
    Write-Host ""
    Write-Host "You must have an active task before editing code." -ForegroundColor Yellow
    Write-Host "Run: /task start TASK-XXX" -ForegroundColor Cyan
    Write-Host ""
    exit 2  # Block the operation
}

# Task is active, allow edit
exit 0
