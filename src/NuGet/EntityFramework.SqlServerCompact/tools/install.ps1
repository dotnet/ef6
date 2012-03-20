param($installPath, $toolsPath, $package, $project)

function Invoke-SqlCompactConnectionFactoryConfigurator($assemblyPath, $project)
{
    $appDomain = [AppDomain]::CreateDomain(
        'EntityFramework.PowerShell',
        $null,
        (New-Object System.AppDomainSetup -Property @{ ShadowCopyFiles = 'true' }))

    $appDomain.CreateInstanceFrom(
        $assemblyPath,
        'System.Data.Entity.ConnectionFactoryConfig.SqlCompactConnectionFactoryConfigurator',
        $false,
        0,
        $null,
        $project,
        $null,
        $null) | Out-Null

    [AppDomain]::Unload($appDomain)
}

Invoke-SqlCompactConnectionFactoryConfigurator (Join-Path $toolsPath EntityFramework.PowerShell.dll) $project
