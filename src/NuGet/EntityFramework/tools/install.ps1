param($installPath, $toolsPath, $package, $project)

if (Get-Service | ?{ $_.Name -eq 'MSSQL$SQLEXPRESS' -and $_.Status -eq 'Running' })
{
    Add-EFDefaultConnectionFactory $project 'System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework'
}
else
{
    $localDbVersion = Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions' -ErrorAction SilentlyContinue |
        %{ $_.PSChildName } |
        sort -Descending |
        select -First 1
    if (!$localDbVersion -or $localDbVersion -ge '12.0')
    {
        $localDbVersion = 'mssqllocaldb'
    }
    else
    {
        $localDbVersion = "v$localDbVersion"
    }

    Add-EFDefaultConnectionFactory $project 'System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework' -ConstructorArguments $localDbVersion
}

$project.Object.References |
    ?{ $_.Identity -eq 'System.Data.Entity' } |
    %{ $_.Remove() }

Write-Host
Write-Host "Type 'get-help EntityFramework6' to see all available Entity Framework commands."
