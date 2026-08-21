[CmdletBinding()]
param([switch]$SkipTests, [switch]$SkipVulnerabilityCheck)

$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out = [IO.Path]::GetFullPath((Join-Path $repo 'artifacts/release/0.5.0'))
if (-not $out.StartsWith(([IO.Path]::GetFullPath((Join-Path $repo 'artifacts')) + [IO.Path]::DirectorySeparatorChar), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe output path.' }
function DotNet([string[]]$Arguments) { & dotnet @Arguments; if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $($Arguments -join ' ')" } }
if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
$publish = Join-Path $out 'publish/self-contained'; $package = Join-Path $out 'package'
New-Item -ItemType Directory -Path $package -Force | Out-Null
Push-Location $repo
try {
    DotNet -Arguments @('clean','Sasd.Pims.slnx','-c','Release','--disable-build-servers','-m:1')
    DotNet -Arguments @('restore','Sasd.Pims.slnx','--disable-build-servers','-m:1')
    DotNet -Arguments @('build','Sasd.Pims.slnx','-c','Release','--no-restore','--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
    if (-not $SkipTests) {
        foreach ($test in @('Domain','Application','Integration','Architecture','WinForms')) {
            $tfm = if ($test -eq 'WinForms') { 'net10.0-windows' } else { 'net10.0' }
            & "tests/Sasd.Pims.$test.Tests/bin/Release/$tfm/Sasd.Pims.$test.Tests.exe"
            if ($LASTEXITCODE -ne 0) { throw "$test tests failed." }
        }
    }
    if (-not $SkipVulnerabilityCheck) { DotNet -Arguments @('package','list','--project','Sasd.Pims.slnx','--vulnerable','--include-transitive') }
    $inventoryText = (& dotnet package list --project Sasd.Pims.slnx --include-transitive --format json --no-restore | Out-String)
    if ($LASTEXITCODE -ne 0) { throw 'Dependency inventory failed.' }
    [IO.File]::WriteAllText((Join-Path $out 'dependency-inventory.json'), $inventoryText, [Text.UTF8Encoding]::new($false))
    $inventory = $inventoryText | ConvertFrom-Json
    $packages = @($inventory.projects.frameworks.topLevelPackages + $inventory.projects.frameworks.transitivePackages) |
        Where-Object { $_ } | ForEach-Object { [pscustomobject]@{ id=$_.id; version=$_.resolvedVersion } } |
        Sort-Object id,version -Unique
    $spdxPackages = @($packages | ForEach-Object { [ordered]@{ SPDXID="SPDXRef-Package-$($_.id -replace '[^A-Za-z0-9.-]','-')-$($_.version)"; name=$_.id; versionInfo=$_.version; downloadLocation="https://www.nuget.org/packages/$($_.id)/$($_.version)"; filesAnalyzed=$false; licenseConcluded='NOASSERTION'; licenseDeclared='NOASSERTION'; copyrightText='NOASSERTION' } })
    $sbom = [ordered]@{ spdxVersion='SPDX-2.3'; dataLicense='CC0-1.0'; SPDXID='SPDXRef-DOCUMENT'; name='SASD-PIMS-0.5.0-win-x64'; documentNamespace="https://sasd.example/sbom/0.5.0/$([Guid]::NewGuid())"; creationInfo=[ordered]@{ created=[DateTimeOffset]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ'); creators=@('Tool: scripts/build-0.5.0.ps1') }; packages=$spdxPackages }
    [IO.File]::WriteAllText((Join-Path $out 'SASD-PIMS-0.5.0.spdx.json'), ($sbom | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
    DotNet -Arguments @('restore','src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj','-r','win-x64','--disable-build-servers','-m:1','-p:NuGetAudit=false')
    DotNet -Arguments @('publish','src/Sasd.Pims.WinForms/Sasd.Pims.WinForms.csproj','-c','Release','-r','win-x64','--self-contained','true','-o',$publish,'--no-restore','--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
    Copy-Item (Join-Path $publish '*') $package -Recurse
    foreach ($file in @('LICENSE','docs/exchange/sasd-pims-exchange-1.0.schema.json','docs/releases/0.5.0/QUICK-START.md','docs/releases/0.5.0/RELEASE-NOTES.md','docs/releases/0.5.0/THIRD-PARTY-NOTICES.md','docs/releases/0.5.0/DEPENDENCIES.md')) { Copy-Item -LiteralPath $file -Destination $package }
    Copy-Item -LiteralPath (Join-Path $out 'dependency-inventory.json') -Destination $package
    Copy-Item -LiteralPath (Join-Path $out 'SASD-PIMS-0.5.0.spdx.json') -Destination $package
    $manifest = [ordered]@{ packageFormatVersion='1.0'; applicationVersion='0.5.0'; sourceCommit=(& git rev-parse HEAD).Trim(); dotnetSdkVersion=(& dotnet --version).Trim(); runtimeIdentifier='win-x64'; publishModel='self-contained'; schemaVersion='202608210005_FullMustMvp'; exchangeFormat='sasd-pims-exchange/1.0'; generatedAtUtc=[DateTimeOffset]::UtcNow.ToString('O') }
    [IO.File]::WriteAllText((Join-Path $package 'package-manifest.json'), ($manifest | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
    $zip = Join-Path $out 'SASD-PIMS-0.5.0-win-x64.zip'; Compress-Archive (Join-Path $package '*') $zip -CompressionLevel Optimal
    $hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash; "$hash  $([IO.Path]::GetFileName($zip))" | Set-Content (Join-Path $out 'SASD-PIMS-0.5.0-SHA256SUMS.txt') -Encoding ascii
    Get-ChildItem $package -Recurse -File | Where-Object { $_.Extension -in '.db','.log','.zip','.env' -or $_.Name -match 'token|secret|backup' } | ForEach-Object { throw "Forbidden runtime data: $($_.FullName)" }
    Write-Output "Release ZIP: $zip"; Write-Output "SHA-256: $hash"; Write-Output "ZIP bytes: $((Get-Item $zip).Length)"
}
finally { Pop-Location }
