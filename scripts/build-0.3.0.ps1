[CmdletBinding()]
param(
    [switch]$SkipTests,
    [switch]$SkipVulnerabilityCheck
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/release/0.3.0'))
$expectedRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $artifactsRoot.StartsWith($expectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Refusing to write release output outside the repository artifacts directory.'
}

function Invoke-DotNet {
    param([Parameter(Mandatory)][string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($Arguments -join ' ')" }
}

if (Test-Path -LiteralPath $artifactsRoot) { Remove-Item -LiteralPath $artifactsRoot -Recurse -Force }
$publishRoot = Join-Path $artifactsRoot 'publish/self-contained'
$packageRoot = Join-Path $artifactsRoot 'package'
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

Push-Location $repositoryRoot
try {
    # Cleaning the solution in parallel can race while shared project outputs are
    # removed by multiple test-project dependency graphs.
    Invoke-DotNet @('clean', 'Sasd.Pims.slnx', '-c', 'Release', '--disable-build-servers', '-m:1')
    Invoke-DotNet @('restore', 'Sasd.Pims.slnx', '--disable-build-servers', '-m:1')
    Invoke-DotNet @('build', 'Sasd.Pims.slnx', '-c', 'Release', '--no-restore', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false')
    # Run the built xUnit v3 executables sequentially. The .NET 10 solution-level
    # MTP orchestrator can leave parallel Windows test hosts waiting after all
    # tests completed, while the in-process runners terminate deterministically.
    if (-not $SkipTests) {
        $testTempRoot = Join-Path $artifactsRoot 'test-temp'
        New-Item -ItemType Directory -Path $testTempRoot -Force | Out-Null
        $previousTemp = $env:TEMP
        $previousTmp = $env:TMP
        try {
            $env:TEMP = $testTempRoot
            $env:TMP = $testTempRoot
            foreach ($testExecutable in @(
                'tests/Sasd.Pims.Domain.Tests/bin/Release/net10.0/Sasd.Pims.Domain.Tests.exe',
                'tests/Sasd.Pims.Application.Tests/bin/Release/net10.0/Sasd.Pims.Application.Tests.exe',
                'tests/Sasd.Pims.IntegrationTests/bin/Release/net10.0/Sasd.Pims.IntegrationTests.exe',
                'tests/Sasd.Pims.Architecture.Tests/bin/Release/net10.0/Sasd.Pims.Architecture.Tests.exe',
                'tests/Sasd.Pims.WinForms.Tests/bin/Release/net10.0-windows/Sasd.Pims.WinForms.Tests.exe'
            )) {
                & $testExecutable
                if ($LASTEXITCODE -ne 0) { throw "Test executable failed with exit code ${LASTEXITCODE}: $testExecutable" }
            }
        }
        finally {
            $env:TEMP = $previousTemp
            $env:TMP = $previousTmp
        }
        Remove-Item -LiteralPath $testTempRoot -Recurse -Force
    }
    if (-not $SkipVulnerabilityCheck) {
        Invoke-DotNet @('package', 'list', '--vulnerable', '--include-transitive')
    }
    # The explicit gate above owns vulnerability auditing. Restore the RID graph
    # without a second network audit so packaging also works in restricted CI.
    Invoke-DotNet @('restore', 'src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj', '-r', 'win-x64', '--disable-build-servers', '-m:1', '-p:NuGetAudit=false')
    Invoke-DotNet @('publish', 'src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $publishRoot, '--no-restore', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false')

    Copy-Item -Path (Join-Path $publishRoot '*') -Destination $packageRoot -Recurse
    Copy-Item -LiteralPath 'LICENSE' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.3.0/QUICK-START.md' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.3.0/RELEASE-NOTES.md' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.3.0/THIRD-PARTY-NOTICES.md' -Destination $packageRoot

    $manifest = [ordered]@{
        packageFormatVersion = '1.0'
        applicationVersion = '0.3.0'
        sourceCommit = (& git rev-parse HEAD).Trim()
        dotnetSdkVersion = (& dotnet --version).Trim()
        runtimeIdentifier = 'win-x64'
        publishModel = 'self-contained'
        schemaVersion = '202608200003_RequirementsAndTypedReferences'
        generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    }
    [IO.File]::WriteAllText((Join-Path $packageRoot 'package-manifest.json'),
        ($manifest | ConvertTo-Json), [Text.UTF8Encoding]::new($false))

    $zipPath = Join-Path $artifactsRoot 'SASD-PIMS-0.3.0-win-x64.zip'
    Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
    $hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
    "$hash  $([IO.Path]::GetFileName($zipPath))" |
        Set-Content -LiteralPath (Join-Path $artifactsRoot 'SASD-PIMS-0.3.0-SHA256SUMS.txt') -Encoding ascii
    if ((Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash -ne $hash) { throw 'Release ZIP checksum verification failed.' }
    Get-ChildItem -LiteralPath $packageRoot -Recurse -File |
        Where-Object { $_.Extension -in '.db', '.log', '.zip', '.env' -or $_.Name -match 'token|secret|backup' } |
        ForEach-Object { throw "Forbidden runtime data found in package: $($_.FullName)" }

    Write-Output "Release ZIP: $zipPath"
    Write-Output "SHA-256: $hash"
    Write-Output "ZIP bytes: $((Get-Item -LiteralPath $zipPath).Length)"
}
finally { Pop-Location }
