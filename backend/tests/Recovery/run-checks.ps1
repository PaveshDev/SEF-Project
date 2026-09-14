$ErrorActionPreference = 'Stop'
$checksRoot = $PSScriptRoot
$repoRoot = [IO.Path]::GetFullPath((Join-Path $checksRoot '../../..'))
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $checksRoot '.artifacts'))
$temporaryProject = [IO.Path]::GetFullPath((Join-Path $artifactsRoot ([Guid]::NewGuid().ToString('N'))))
if (-not $temporaryProject.StartsWith($artifactsRoot + [IO.Path]::DirectorySeparatorChar)) {
    throw 'Temporary project must stay inside the Recovery checks artifact directory.'
}
$apiProject = Join-Path $repoRoot 'backend/src/WasteToValue.Api/WasteToValue.Api.csproj'
Push-Location $repoRoot
try {
    & dotnet new console --framework net8.0 --name RecoveryChecks --output $temporaryProject --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not generate the temporary SDK checks project.' }
    $projectFile = Join-Path $temporaryProject 'RecoveryChecks.csproj'
    & dotnet add $projectFile reference $apiProject | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not reference the API project.' }
    # EF Design is private to the API, so its Relational dependency does not flow
    # to project references. Match the API's actual locked runtime version here.
    $apiLock = Get-Content -LiteralPath (Join-Path (Split-Path $apiProject) 'packages.lock.json') -Raw | ConvertFrom-Json
    $relationalVersion = $apiLock.dependencies.'net8.0'.'Microsoft.EntityFrameworkCore.Relational'.resolved
    & dotnet add $projectFile package Microsoft.EntityFrameworkCore.Relational --version $relationalVersion --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not align the temporary checks with the API runtime.' }
    Remove-Item -LiteralPath (Join-Path $temporaryProject 'Program.cs')
    Get-ChildItem -LiteralPath $checksRoot -Filter '*.cs' | Copy-Item -Destination $temporaryProject
    & dotnet run --project $projectFile --configuration Release --property:RestoreLockedMode=true
    if ($LASTEXITCODE -ne 0) { throw 'Recovery checks failed.' }
}
finally {
    Pop-Location
}
# The generated project stays under ignored .artifacts for diagnostics.
# No tracked solution, manifest, lockfile, migration, or model snapshot is edited.
