@echo off
SqlLocalDB start
sqlcmd -S "(localdb)\mssqllocaldb" -i "%~dp0DropAllDatabases.sql" -l 32 -r0 >"DropAll.sql"
echo Will execute:
type DropAll.sql
sqlcmd -S "(localdb)\mssqllocaldb" -i "DropAll.sql"
del "DropAll.sql"

%~dp0ShrinkLocalDBModel.cmd
