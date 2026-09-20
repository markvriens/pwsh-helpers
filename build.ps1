$ErrorActionPreference = 'Stop'
$moduleName = 'Build-JsonValidator'
# Set up output directories and paths for the build artifacts
$outDir = Join-Path $PSScriptRoot "release/$moduleName"
$binReleaseDir = Join-Path $PSScriptRoot "src/modules/$moduleName/bin/Release/net8.0"
$binDll = Join-Path $outDir "$moduleName.dll"

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$srcDir = Join-Path $PSScriptRoot "src/modules/$moduleName"
$srcProject = Join-Path $srcDir "$moduleName.csproj"
dotnet build $srcProject -c Release

Copy-Item `
  (Join-Path $srcDir "$moduleName.psd1") `
  (Join-Path $outDir "$moduleName.psd1") `
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
Get-JsonSchemaValidation -Json '{"name": "Mark"}' -Schema '{"type": "object", "properties": {"name": {"type": "string"}}}'

Get-Module Build-JsonValidator | Remove-Module -Force

