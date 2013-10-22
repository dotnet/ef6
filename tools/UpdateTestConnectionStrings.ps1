# Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Param(
  [string]$newConnectionString
)

foreach($appConfigPath in "FunctionalTests", "FunctionalTests.Transitional", "FunctionalTests.ProviderAgnostic", "UnitTests", "VBTests")
{
	$appConfigPath = "$pwd\test\EntityFramework\$appConfigPath\App.config"
	$xml = [xml] (get-content $appConfigPath)
	$connectionStrings = $xml.configuration.connectionStrings.add
	foreach ($conStr in $connectionStrings)
	{
		$str = $conStr.connectionString
		if ($str.Contains($sqlexpress))
		{
			$str = $str.replace("Integrated Security=True;", "")
			$str = $str.replace("Trusted_Connection=True;", "")
			$str = $str.replace("Server=.\SQLEXPRESS;", "$newConnectionString")
			$str = $str.replace("DataSource=.\SQLEXPRESS;", "$newConnectionString")
		}
		$conStr.connectionString = $str
	}
	foreach ($setting in $xml.configuration.appSettings.add)
	{
		if ($setting.key -eq "BaseConnectionString")
		{
			$setting.value="$newConnectionString"
		}
	}
	$xml.Save("$appConfigPath")
}

