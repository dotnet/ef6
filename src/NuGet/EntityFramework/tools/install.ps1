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

Invoke-ConnectionFactoryConfigurator (Join-Path $toolsPath EntityFramework.PowerShell.dll) $project

Write-Host
Write-Host "Type 'get-help EntityFramework' to see all available Entity Framework commands."
