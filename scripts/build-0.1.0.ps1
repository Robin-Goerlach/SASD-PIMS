[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/release/0.1.0'))
$expectedRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $artifactsRoot.StartsWith($expectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Refusing to write release output outside the repository artifacts directory.'
}

function Invoke-DotNet {
    param([Parameter(Mandatory)][string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($Arguments -join ' ')"
    }
}

if (Test-Path -LiteralPath $artifactsRoot) {
    Remove-Item -LiteralPath $artifactsRoot -Recurse -Force
}
$publishRoot = Join-Path $artifactsRoot 'publish/self-contained'
$packageRoot = Join-Path $artifactsRoot 'package'
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

Push-Location $repositoryRoot
try {
    Invoke-DotNet @('clean', 'Sasd.Pims.slnx', '-c', 'Release', '--disable-build-servers')
    Invoke-DotNet @('restore', 'Sasd.Pims.slnx', '--disable-build-servers')
    Invoke-DotNet @('build', 'Sasd.Pims.slnx', '-c', 'Release', '--no-restore', '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false')
    Invoke-DotNet @('test', 'Sasd.Pims.slnx', '-c', 'Release', '--no-build')
    Invoke-DotNet @('package', 'list', '--vulnerable', '--include-transitive')
    Invoke-DotNet @('publish', 'src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $publishRoot, '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false')

    Copy-Item -Path (Join-Path $publishRoot '*') -Destination $packageRoot -Recurse
    Copy-Item -LiteralPath 'LICENSE' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.1.0/QUICK-START.md' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.1.0/RELEASE-NOTES.md' -Destination $packageRoot
    Copy-Item -LiteralPath 'docs/releases/0.1.0/THIRD-PARTY-NOTICES.md' -Destination $packageRoot

    $manifest = [ordered]@{
        packageFormatVersion = '1.0'
        applicationVersion = '0.1.0'
        sourceCommit = (& git rev-parse HEAD).Trim()
        dotnetSdkVersion = (& dotnet --version).Trim()
        runtimeIdentifier = 'win-x64'
        publishModel = 'self-contained'
        schemaVersion = '202608200001_ProjectCatalog'
        generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    }
    [IO.File]::WriteAllText((Join-Path $packageRoot 'package-manifest.json'),
        ($manifest | ConvertTo-Json), [Text.UTF8Encoding]::new($false))

    $zipPath = Join-Path $artifactsRoot 'SASD-PIMS-0.1.0-win-x64.zip'
    Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal
    $hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
    "$hash  $([IO.Path]::GetFileName($zipPath))" |
        Set-Content -LiteralPath (Join-Path $artifactsRoot 'SASD-PIMS-0.1.0-SHA256SUMS.txt') -Encoding ascii
    if ((Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash -ne $hash) {
        throw 'Release ZIP checksum verification failed.'
    }
    Get-ChildItem -LiteralPath $packageRoot -Recurse -File |
        Where-Object { $_.Extension -in '.db', '.log', '.zip', '.env' -or $_.Name -match 'token|secret|backup' } |
        ForEach-Object { throw "Forbidden runtime data found in package: $($_.FullName)" }

    Write-Output "Release ZIP: $zipPath"
    Write-Output "SHA-256: $hash"
    Write-Output "ZIP bytes: $((Get-Item -LiteralPath $zipPath).Length)"
}
finally {
    Pop-Location
}
