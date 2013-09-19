@ECHO OFF

SETLOCAL

ECHO SampleEntityDDEXProvider Installation

SET MYDIR=%~dp0
SET RANU=No
SET REGROOT=SOFTWARE\Microsoft\VisualStudio\11.0Exp_Config
SET CODEBASE=

:ParseCmdLine

IF "%1"=="" GOTO Main
IF "%1"=="/ranu" SET RANU=Yes& GOTO NextCmdLine
IF "%1"=="/regroot" IF NOT "%~2"=="" SET REGROOT=%~2& SHIFT & GOTO NextCmdLine
IF "%1"=="/codebase" IF NOT "%~2"=="" SET CODEBASE=%~f2& SHIFT & GOTO NextCmdLine
IF "%1"=="/?" GOTO Help
GOTO Help

:NextCmdLine

SHIFT
GOTO ParseCmdLine

:Main

IF "%CODEBASE%"=="" GOTO Help

ECHO   Register as Normal User: %RANU%
ECHO   VS Registry Root: %REGROOT%
ECHO   Code base: %CODEBASE%

IF NOT EXIST "%CODEBASE%" (
  ECHO The code base was not found.
  GOTO End
)

IF NOT EXIST "%SystemRoot%\SysWOW64" (
  CScript "%MYDIR%\Install.vbs" //NoLogo %RANU% "%REGROOT%" "%CODEBASE%" "regedit"
) ELSE (
  CScript "%MYDIR%\Install.vbs" //NoLogo %RANU% "%REGROOT%" "%CODEBASE%" "%SystemRoot%\SysWOW64\regedit"
)

ECHO Done!

GOTO End

:Help

ECHO   Usage: install [/ranu] [/regroot ^<regroot^>] /codebase ^<codebase^> [/?]

:End

ENDLOCAL
