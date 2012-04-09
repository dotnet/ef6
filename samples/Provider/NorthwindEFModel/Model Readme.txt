This project contains the Entity Framework Model and Database for the Entity Framework Query Samples. In order to run the samples, you need to perform the following steps:

1. Attach the database to your SQL Server instance. The database MDF and LDF can be found under the Database folder. 

2. Modify the connection string in the App.config file to reflect the SQL Server instance you are using.


*** Attaching the database ***
To attach the database, please run the Attach_NorthwindEF.cmd script in this folder, passing in the instance name as an argument. For example, if you are using SQL Express, you would execute the following on the command line.

Attach_NorthwindEF.cmd .\sqlexpress 

When finished, you can use the Detact_NorthwindEF.cmd script, again passing in the instance name, to remove the database.

*** Modifying the App.config ***
In the App.config file, update the connection string to reflect the SQL Server instance and security parameters you are using. The current connection string is set up for SQL Express, so if you are using this, you should not need to modify the connection string.
