@{
    RootModule = 'Build-JsonValidator.dll'
    ModuleVersion = '0.1.0'
    GUID = '7d2d5d8a-3c5d-41f0-a0de-c8f7eabf9f12'
    Author = 'Mark'
    CompanyName = 'MV'
    Copyright = '(c) 2026'
    Description = 'Sample PowerShell 7 binary module in C#'
    PowerShellVersion = '7.0'
    CompatiblePSEditions = @('Core')

    RequiredAssemblies = @(
        'Newtonsoft.Json.dll',
        'Newtonsoft.Json.Schema.dll'
    )

    CmdletsToExport = @('Get-JsonSchemaValidation')
    FunctionsToExport = @()
    AliasesToExport = @()

    PrivateData = @{
        PSData = @{
            Tags = @('PowerShell', 'CSharp', 'BinaryModule')
            ProjectUri = 'https://github.com/markvriens/pwsh-helpers'
        }
    }
}