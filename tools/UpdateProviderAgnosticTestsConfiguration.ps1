# Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

# this script configures ProviderAgnostic tests to run againts a particular provider and targetting a particular database
# replaces the default App.config file with a provider specific template (e.g. App.config.mysql) 
# and sets a base connection string for that file
# first parameter is a provider name and must match the suffix on one of the config files (e.g mysql for App.config.mysql)
# second parameter is a base connection string (without a database name, e.g. "server=localhost;User Id=root;password=******;")

Param(
  [string]$providerName,
  [string]$newConnectionString
)

$appConfigDirectoryPath = "$pwd\test\FunctionalTests.ProviderAgnostic"
write-host $appConfigDirectoryPath

$sourceConfigPath = "$appConfigDirectoryPath\App.config.$providerName"

if (Test-Path ($sourceConfigPath)) {
	$xml = [xml] (get-content $sourceConfigPath)
	$xpath = "/configuration/appSettings/add[@key='BaseConnectionString']"
	Select-Xml $xml -XPath $xpath | Foreach {$_.Node.SetAttribute('value', $newConnectionString)}
	$xml.Save("$appConfigDirectoryPath\App.config")
}

