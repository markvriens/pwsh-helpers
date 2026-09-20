$ErrorActionPreference = 'Stop'

# Set up output directories and paths for the build artifacts

$binReleaseDir = Join-Path $PSScriptRoot 'src/modules/Build-JsonValidator/bin/Release/net8.0'
$binDll = Join-Path $outDir 'Build-JsonValidator.dll'

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$srcDir = Join-Path $PSScriptRoot 'src/modules/Build-JsonValidator'
$srcProject = Join-Path $srcDir 'Build-JsonValidator.csproj'
dotnet build $srcProject -c Release

Copy-Item `
  (Join-Path $srcDir "Build-JsonValidator.psd1") `
  (Join-Path $outDir "Build-JsonValidator.psd1") `
  -Force

Copy-Item `
  (Join-Path $binReleaseDir 'Build-JsonValidator.dll') `
  $binDll `
  -Force

Copy-Item `
  (Join-Path $binReleaseDir 'Newtonsoft.Json.dll') `
  (Join-Path $outDir 'Newtonsoft.Json.dll') `
  -Force

Copy-Item `
  (Join-Path $binReleaseDir 'Newtonsoft.Json.Schema.dll') `
  (Join-Path $outDir 'Newtonsoft.Json.Schema.dll') `
  -Force

Import-Module ./out/Build-JsonValidator.psd1 -Force
Get-JsonSchemaValidation -Json '{"name": "Mark"}' -Schema '{"type": "object", "properties": {"name": {"type": "string"}}}'

Get-Module Build-JsonValidator | Remove-Module -Force

