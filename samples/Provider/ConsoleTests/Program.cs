
namespace ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleWhidbeyTests.RunTests();

            EntityFrameworkPrerequisiteTests.RunTests();

            EntityClientTests.RunTests();
            ObjectQueryTests.RunTests();
            LinqToEntitiesTests.RunTests();

            DmlTests.RunTests();

            // Note: you need to rerun after this is executed to execute using the updated files
            FunctionStubGeneration.GenerateNewFiles();

            CreateDatabaseTests.RunTests();            
        }
    }
}
