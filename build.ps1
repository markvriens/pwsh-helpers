$ErrorActionPreference = 'Stop'
$moduleName = 'Build-JsonValidator'
# Set up output directories and paths for the build artifacts
$outDir = Join-Path $PSScriptRoot "release/$moduleName"
$binReleaseDir = Join-Path $PSScriptRoot "src/Modules/$moduleName/bin/Release/net10.0"
$binDll = Join-Path $outDir "$moduleName.dll"

if (-Not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
}
else {
  Remove-Item -Recurse -Force -Path $outDir
}

$srcDir = Join-Path $PSScriptRoot "src/Modules/$moduleName"
$srcProject = Join-Path $srcDir "$moduleName.csproj"
$changelogPath = Join-Path $srcDir 'CHANGELOG.md'

if (-not (Get-Module -ListAvailable -Name ChangelogManagement)) {
    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
    Install-Module ChangelogManagement -Scope CurrentUser -Force -AllowClobber
}

Import-Module ChangelogManagement -Force

if (-not (Test-Path $changelogPath)) {
    New-Changelog -Path $changelogPath -NoSemVer
}

dotnet build $srcProject -c Release
$srcDir
Copy-Item `
  (Join-Path $srcDir "$moduleName.psd1") `
  (Join-Path $outDir "$moduleName.psd1") `
  -Force

Copy-Item `
  (Join-Path $srcDir 'CHANGELOG.md') `
  (Join-Path $outDir 'CHANGELOG.md') `
  -Force

Copy-Item `
  (Join-Path $binReleaseDir "$moduleName.dll") `
  $binDll `
  -Force
# Copy dependencies from the build output to the release directory
Copy-Item `
  (Join-Path $binReleaseDir 'Newtonsoft.Json.dll') `
  (Join-Path $outDir 'Newtonsoft.Json.dll') `
  -Force

Copy-Item `
  (Join-Path $binReleaseDir 'Newtonsoft.Json.Schema.dll') `
  (Join-Path $outDir 'Newtonsoft.Json.Schema.dll') `
  -Force

Import-Module $outDir/$moduleName.psd1 -Force
Get-JsonSchemaValidation -JsonPath '.\tests\mock\test.json' -SchemaSource '.\tests\mock\test.schema.json'

Get-Module Build-JsonValidator -All | Remove-Module -Force -ErrorAction SilentlyContinue

