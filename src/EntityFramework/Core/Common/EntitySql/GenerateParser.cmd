@ECHO OFF
SETLOCAL
SET TOOLSPATH=%SDXROOT%\ndp\fx\src\DataEntity\tools\bin\x86
del y
ECHO.
ECHO Generating Scanner...
ECHO ~~~~~~~~~~~~~~~~~~~~~
%TOOLSPATH%\lex.exe CqlLexer.l CqlLexer.cs
ECHO.
ECHO.
ECHO Generating Grammar...
ECHO ~~~~~~~~~~~~~~~~~~~~~
%TOOLSPATH%\yacc.exe -v -fCqlParser -m"internal partial" -nSystem.Data.Common.EntitySql -c# CqlGrammar.y 
POPD
ENDLOCAL
ECHO.
ECHO DONE!
ECHO.