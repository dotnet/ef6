@ECHO OFF

IF DEFINED VisualStudioVersion (
  GOTO BuildStep
)
IF DEFINED VS110COMNTOOLS (
  CALL "%VS110COMNTOOLS%\..\..\VC\vcvarsall.bat"
  GOTO BuildStep
)
IF DEFINED VS120COMNTOOLS (
  CALL "%VS120COMNTOOLS%\..\..\VC\vcvarsall.bat"
  GOTO BuildStep
)
ECHO "Warning: Cannot find installed VS path."

:BuildStep

msbuild "%~dp0\EFTools.msbuild" /p:RunCodeAnalysisForEFTools=true /v:minimal /maxcpucount /nodeReuse:false %*
