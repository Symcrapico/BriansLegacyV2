# ============================================================================
# session-start.ps1
# Purpose: Display backlog status at session start (Windows)
# Dependencies: BACKLOG.md
# ============================================================================

# Check if BACKLOG.md exists
if (-not (Test-Path "BACKLOG.md")) {
    Write-Host "+-------------------------------------------------------------+"
    Write-Host "| [!] BACKLOG.md not found                                    |"
    Write-Host "|     Run: /task init  to initialize task management          |"
    Write-Host "+-------------------------------------------------------------+"
    exit 0
}

# Read BACKLOG.md content
$content = Get-Content "BACKLOG.md" -Raw

# Extract active task
$activeMatch = [regex]::Match($content, '^> Active: (.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
$active = if ($activeMatch.Success) { $activeMatch.Groups[1].Value.Trim() } else { "None" }

# Get active task description if there is one
$activeDesc = ""
if ($active -ne "None" -and $active) {
    $descMatch = [regex]::Match($content, "\| $([regex]::Escape($active)) \| ([^|]+) \|")
    if ($descMatch.Success) {
        $activeDesc = $descMatch.Groups[1].Value.Trim()
    }
}

# Count blocked tasks
$blockedMatches = [regex]::Matches($content, '\| [^|]+ \|')
$blockedCount = ([regex]::Matches($content, '\| [^|]+ \|')).Count
$blockedCount = 0
foreach ($line in ($content -split "`n")) {
    if ($line -match '\| [^|]+ \|') {
        # This is a rough check - we need to look for the blocked emoji
    }
}
$blockedCount = ([regex]::Matches($content, [regex]::Escape('| blocked |'))).Count

# Actually, let's count by the emoji pattern
$blockedLines = $content -split "`n" | Where-Object { $_ -match [regex]::Escape('| blocked |') -or $_ -match 'blocked' }
$blockedCount = ($content -split "`n" | Where-Object { $_ -match '\|.*blocked.*\|' }).Count

# Count by status - use simpler approach
$lines = $content -split "`n"
$blockedCount = ($lines | Where-Object { $_ -match 'blocked' -and $_ -match '^\|' }).Count
$readyCount = ($lines | Where-Object { $_ -match 'ready' -and $_ -match '^\|' }).Count
$inProgressCount = ($lines | Where-Object { $_ -match 'in_progress|in-progress' -and $_ -match '^\|' }).Count
$doneCount = ($lines | Where-Object { $_ -match 'completed|done' -and $_ -match '^\|' }).Count

# Get next 3 ready tasks
$readyTasks = $lines | Where-Object { $_ -match 'ready' -and $_ -match '^\| TASK-' } | Select-Object -First 3
$nextTasks = @()
foreach ($task in $readyTasks) {
    if ($task -match '\| (TASK-[^ |]+) \| ([^|]+) \|') {
        $nextTasks += "   $($matches[1]): $($matches[2].Trim())"
    }
}

# Display status
Write-Host "+-------------------------------------------------------------+"
Write-Host "| BACKLOG STATUS                                              |"
Write-Host "+-------------------------------------------------------------+"

if ($active -ne "None" -and $active) {
    Write-Host "| " -NoNewline
    Write-Host "ACTIVE: $active" -ForegroundColor Yellow
    if ($activeDesc) {
        Write-Host "|    $activeDesc"
    }
} else {
    Write-Host "| No active task                                              |"
}

Write-Host "+-------------------------------------------------------------+"

if ($blockedCount -gt 0) {
    Write-Host "| " -NoNewline
    Write-Host "BLOCKED: $blockedCount task(s)" -ForegroundColor Red -NoNewline
    Write-Host "                                          |"
    Write-Host "+-------------------------------------------------------------+"
}

Write-Host "| NEXT READY:                                                 |"
if ($nextTasks.Count -gt 0) {
    foreach ($task in $nextTasks) {
        Write-Host "| $task"
    }
} else {
    Write-Host "|    (no ready tasks)                                         |"
}

Write-Host "+-------------------------------------------------------------+"
Write-Host "| Ready: $readyCount | In Progress: $inProgressCount | Done: $doneCount"
Write-Host "+-------------------------------------------------------------+"
Write-Host ""
Write-Host "Commands: /task start TASK-XXX | /task done TASK-XXX | /task"
