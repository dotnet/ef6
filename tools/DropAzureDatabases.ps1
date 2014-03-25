# Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Param(
  [string]$AzureServer,
  [string]$AzureUsername,
  [string]$AzurePassword
)

$query = "SELECT Name FROM sysdatabases WHERE Name NOT IN ('master', 'tempdb', 'model', 'msdb', 'AdventureWorks2012', 'Chinook', 'Northwind', 'pubs')"
$results = Invoke-Sqlcmd -Query $query -ServerInstance $AzureServer -Username $AzureUsername -Password $AzurePassword -Database "master" -verbose -ConnectionTimeout 30 -EncryptConnection

$dropQueries = New-Object "System.Collections.Generic.List``1[System.String]"
$results | ForEach-Object({$dropQueries.add("DROP DATABASE [" + $_.Name.Replace("]", "]]") + "]")}) #String.Replace is used to generate drop queries that correctly escape closing square brackets in table names

$dropQueries | ForEach-Object({Invoke-Sqlcmd -Query $_ -ServerInstance $AzureServer -Username $AzureUsername -Password $AzurePassword -Database "master" -verbose -ConnectionTimeout 30 -EncryptConnection})