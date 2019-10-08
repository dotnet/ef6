param($installPath, $toolsPath, $package, $project)

# NB: Not set for scripts in PowerShell 2.0
if (!$PSScriptRoot)
{
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

if ($PSVersionTable.PSVersion -lt '3.0')
{
    Import-Module (Join-Path $PSScriptRoot 'EntityFramework6.PS2.psd1')

    return
}

$importedModule = Get-Module 'EntityFramework6'
$moduleToImport = Test-ModuleManifest (Join-Path $PSScriptRoot 'EntityFramework6.psd1')
$import = $true
if ($importedModule)
{
    if ($importedModule.Version -le $moduleToImport.Version)
    {
        Remove-Module 'EntityFramework6'
    }
    else
    {
        $import = $false
    }
}

if ($import)
{
    Import-Module $moduleToImport
}
