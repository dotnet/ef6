$ProgramFiles86 = (${env:ProgramFiles(x86)}, ${env:ProgramFiles} -ne $null)[0]
$vswhere = "${ProgramFiles86}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-NOT (Test-Path $vswhere)) {
  Write-Host "Could not find vswhere.exe in $Env:ProgramFiles(x86)\Microsoft Visual Studio\Installer"
  exit
}

$installationPath = Invoke-Expression "& '$vswhere' -prerelease -latest -property installationPath"
Write-Host "installationPath = ${installationPath}"

if ($installationPath -and (Test-Path "$installationPath\Common7\Tools\VSDevCmd.bat")) {
  & "${env:COMSPEC}" /s /c "`"$installationPath\Common7\Tools\VSDevCmd.bat`" -no_logo && set" | foreach-object {
    $name, $value = $_ -split '=', 2
    Write-Host "Setting $name to $value"
    Set-Content env:\"$name" $value
  }
}