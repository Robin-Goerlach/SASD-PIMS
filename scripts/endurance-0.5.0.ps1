[CmdletBinding()]
param([int]$DurationMinutes = 240, [int]$IntervalSeconds = 30)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$test = Join-Path $root 'tests/Sasd.Pims.IntegrationTests/bin/Release/net10.0/Sasd.Pims.IntegrationTests.exe'
if (-not (Test-Path -LiteralPath $test)) { throw 'Build Release tests before running endurance qualification.' }
$deadline = [DateTimeOffset]::UtcNow.AddMinutes($DurationMinutes)
$runs = 0
while ([DateTimeOffset]::UtcNow -lt $deadline) {
    & $test --filter-class '*SqliteRecoveryTests' --filter-class '*ReferenceTargetValidatorTests'
    if ($LASTEXITCODE -ne 0) { throw "Endurance negative-path run failed after $runs completed run(s)." }
    $runs++
    Start-Sleep -Seconds $IntervalSeconds
}
Write-Output "Completed $runs endurance negative-path runs without a test failure."
