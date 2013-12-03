param($installPath, $toolsPath, $package, $project)

Add-EFDefaultConnectionFactory $project 'System.Data.Entity.Infrastructure.SqlCeConnectionFactory, EntityFramework' -ConstructorArguments 'System.Data.SqlServerCe.3.5'
Add-EFProvider $project 'System.Data.SqlServerCe.3.5' 'System.Data.Entity.SqlServerCompact.Legacy.SqlCeProviderServices, EntityFramework.SqlServerCompact.Legacy'
