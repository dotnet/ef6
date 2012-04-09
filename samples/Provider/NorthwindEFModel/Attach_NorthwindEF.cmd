set DATABASE_NAME=NorthwindEF5
set MDF_FILE=%~dp0\Database\NorthwindEF5.mdf
set LDF_FILE=%~dp0\Database\NorthwindEF5_log.ldf
set SERVER_INSTANCE=%1
if (%1)==() set SERVER_INSTANCE=.\sqlexpress

osql -S %SERVER_INSTANCE% -E -Q "exec sp_attach_db '%DATABASE_NAME%','%MDF_FILE%','%LDF_FILE%'"