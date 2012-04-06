set DATABASE_NAME=NorthwindEF5
set SERVER_INSTANCE=%1
if (%1)==() set SERVER_INSTANCE=.\sqlexpress

osql -S %SERVER_INSTANCE% -E -Q "exec sp_detach_db '%DATABASE_NAME%'"