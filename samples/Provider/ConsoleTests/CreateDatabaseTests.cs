using System;
using System.Data.Objects;

namespace ConsoleTests
{
    class CreateDatabaseTests
    {
        public static void RunTests()
        {
            GenerateDatabaseScript();
            CreateDatabase();
        }

        private static void GenerateDatabaseScript()
        {
            ObjectContext nwEntities = new ObjectContext("name=NorthwindAttach");
            string script = nwEntities.CreateDatabaseScript();
            Console.WriteLine("GenerateDatabaseScript");
            Console.WriteLine(script);
        }        

        private static void CreateDatabase()
        {
            Console.WriteLine("CreateDatabase");

            ObjectContext nwEntities = new ObjectContext("name=NorthwindAttach");

            if (nwEntities.DatabaseExists())
            {
                Console.WriteLine("  Database already exists.....Deleting database first");
                nwEntities.DeleteDatabase();
                Console.WriteLine("  Database deleted");
            }

            Console.WriteLine("  Creating database");
            nwEntities.CreateDatabase();

            if(nwEntities.DatabaseExists())
                Console.WriteLine("  Success!");
        }        
    }
}
