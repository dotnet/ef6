using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Xml;

namespace ConsoleTests
{
    class EntityFrameworkPrerequisiteTests
    {
        public static void RunTests()
        {
            GetFactoryViaConnection();
            VerifyCommandImplementsICloneable();
            VerifyProviderSupportsDbProviderServices();
            VerifyProviderManifest();
            VerifyProviderManifestToken();
        }

        static void GetFactoryViaConnection()
        {
            Console.WriteLine("GetFactoryViaConnection");

            Type type = typeof(DbConnection);
            BindingFlags flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic;
            PropertyInfo property = type.GetProperty("ProviderFactory", flags);

            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            if (factory == null)
                Console.WriteLine("  Failure! - Null DbProviderFactory returned via DbProviderFactories.GetFactory({0})!", HelperFunctions.ProviderName);
            else
            {
                DbConnection connection = factory.CreateConnection();
                DbProviderFactory factoryFromConnection = (DbProviderFactory)property.GetValue(connection, null);
                if (factoryFromConnection == null)
                    Console.WriteLine("  Failure! - Connection.ProviderFactory returned null");
                else
                {
                    if (!factoryFromConnection.GetType().Equals(factory.GetType()))
                        Console.WriteLine("  Failure! - Connection.ProviderFactory returned {0}, expected {1}", factoryFromConnection.GetType().Name, factory.GetType().Name);
                    else
                        Console.WriteLine("  Success!");
                }
            }

            Console.WriteLine();
        }

        static void VerifyCommandImplementsICloneable()
        {
            Console.WriteLine("VerifyCommandImplementsICloneable");

            DbProviderFactory factory = DbProviderFactories.GetFactory(HelperFunctions.ProviderName);
            if (factory == null)
                Console.WriteLine("  Failure! - Null DbProviderFactory returned via DbProviderFactories.GetFactory({0})!", HelperFunctions.ProviderName);
            else
            {
                DbCommand command = factory.CreateCommand();
                ICloneable cloneable = command as ICloneable;
                if (cloneable == null)
                    Console.WriteLine("  Failure! - {0} does not implement ICloneable", command.GetType().Name);
                else
                {
                    DbCommand clonedCommand = (DbCommand)cloneable.Clone();
                    if (cloneable == null)
                        Console.WriteLine("  Failure! - Cloning command returned null");
                    else
                        Console.WriteLine("  Success!");
                }
            }

            Console.WriteLine();
        }

        static void VerifyProviderSupportsDbProviderServices()
        {
            Console.WriteLine("VerifyProviderSupportsDbProviderServices");

            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            IServiceProvider iserviceprovider = factory as IServiceProvider;
            if (iserviceprovider == null)
                Console.WriteLine("  Failure! - {0} does not implement IServiceProvider", factory.GetType().Name);
            else
            {
                DbProviderServices dbproviderservices = (DbProviderServices) iserviceprovider.GetService(typeof(DbProviderServices));
                if (dbproviderservices == null)
                    Console.WriteLine("  Failure! - {0} does not support IServiceProvider.GetService(typeof(DbProviderServices))", factory.GetType().Name);
                else
                {
                    Console.WriteLine("  Success!");
                }
            }

            Console.WriteLine();
        }

        static void VerifyProviderManifest()
        {
            Console.WriteLine("VerifyProviderManifest");

            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            DbProviderServices providerservices = HelperFunctions.GetProviderServicesViaDbProviderFactories();
            DbProviderManifest manifest = providerservices.GetProviderManifest("2005");
            if (manifest == null)
                Console.WriteLine("  Failure! - DbProviderServices.GetProviderManifest() returned null!");
            else
                Console.WriteLine("  Success!");

            Console.WriteLine();
        }

        static void VerifyProviderManifestToken()
        {
            Console.WriteLine("VerifyProviderManifestToken");

            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            DbProviderServices providerservices = HelperFunctions.GetProviderServicesViaDbProviderFactories();
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = HelperFunctions.NorthwindDirectConnectionString;

                string token = providerservices.GetProviderManifestToken(connection);
                if (token != "2005" && token != "2008")
                    Console.WriteLine("  Failure! - DbProviderServices.GetProviderManifestToken() returned invalid token!");
                else
                    Console.WriteLine("  Success!");
            }

            Console.WriteLine();
        }

    }
}
