param($installPath, $toolsPath, $package, $project)

$importedModule = Get-Module | ?{ $_.Name -eq 'EntityFramework' }
$thisModule = Test-ModuleManifest (Join-Path $toolsPath EntityFramework.psd1)
$shouldImport = $true

if ($importedModule)
{
    if ($importedModule.Version -le $thisModule.Version)
    {
        Remove-Module EntityFramework
    }
    else
    {
        $shouldImport = $false
    }    
}

if ($shouldImport)
{
    Import-Module $thisModule
}
