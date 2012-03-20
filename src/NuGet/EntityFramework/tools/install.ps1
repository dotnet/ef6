param($installPath, $toolsPath, $package, $project)

function Invoke-ConnectionFactoryConfigurator($assemblyPath, $project)
{
    $appDomain = [AppDomain]::CreateDomain(
        'EntityFramework.PowerShell',
        $null,
        (New-Object System.AppDomainSetup -Property @{ ShadowCopyFiles = 'true' }))

    $appDomain.CreateInstanceFrom(
        $assemblyPath,
        'System.Data.Entity.ConnectionFactoryConfig.ConnectionFactoryConfigurator',
        $false,
        0,
        $null,
        $project,
        $null,
        $null) | Out-Null

    [AppDomain]::Unload($appDomain)
}

$version = (New-Object System.Runtime.Versioning.FrameworkName ($project.Properties.Item('TargetFrameworkMoniker').Value)).Version

if ($version -lt (New-Object System.Version @( 4, 5 )))
{
    $dte.ItemOperations.OpenFile((Join-Path $toolsPath 'EF5.0on.NET4.0Readme.txt'))
}

Invoke-ConnectionFactoryConfigurator (Join-Path $toolsPath EntityFramework.PowerShell.dll) $project

Write-Host
Write-Host "Type 'get-help EntityFramework' to see all available Entity Framework commands."
