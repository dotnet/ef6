param($installPath, $toolsPath, $package, $project)

Initialize-EFConfiguration $project
Add-EFProvider $project 'System.Data.SqlClient' 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'

Write-Host
Write-Host "Type 'get-help EntityFramework' to see all available Entity Framework commands."
