// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class MetadataCachingTests
    {
        private readonly static string connectionStringDirectoryBasedTemplate = @"metadata={0};provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""";
        private readonly static string connectionStringFileBasedTemplate = @"metadata={0}/Model.csdl|{0}/Model.ssdl|{0}/Model.msl;provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""";


        [Fact]
        public void Verify_that_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_same_connection_strings()
        {
            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringDirectoryBasedTemplate, modelDirectory);

            try
            {
                this.DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection1 = new EntityConnection(connectionString);
                var connection2 = new EntityConnection(connectionString);
                MetadataWorkspace workspace1 = connection1.GetMetadataWorkspace();
                MetadataWorkspace workspace2 = connection2.GetMetadataWorkspace();

                var edmCollection1 = workspace1.GetItemCollection(DataSpace.CSpace);
                var edmCollection2 = workspace2.GetItemCollection(DataSpace.CSpace);

                Assert.Same(edmCollection1, edmCollection2);

                connection1.Open();
                connection2.Open();

                var storeCollection1 = workspace1.GetItemCollection(DataSpace.SSpace);
                var storeCollection2 = workspace2.GetItemCollection(DataSpace.SSpace);
                var mappingCollection1 = workspace1.GetItemCollection(DataSpace.CSSpace);
                var mappingCollection2 = workspace2.GetItemCollection(DataSpace.CSSpace);

                Assert.Same(storeCollection1, storeCollection2);
                Assert.Same(mappingCollection1, mappingCollection2);
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void Verify_that_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_equivalent_connection_strings() 
        {
            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString1 = string.Format(CultureInfo.InvariantCulture, connectionStringDirectoryBasedTemplate, modelDirectory);
            var connectionString2 = connectionString1 + ";;";

            try
            {
                this.DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection1 = new EntityConnection(connectionString1);
                var connection2 = new EntityConnection(connectionString2);

            var workspace1 = connection1.GetMetadataWorkspace();
            var workspace2 = connection2.GetMetadataWorkspace();

            var edmCollection1 = workspace1.GetItemCollection(DataSpace.CSpace);
            var edmCollection2 = workspace2.GetItemCollection(DataSpace.CSpace);

            Assert.Same(edmCollection1, edmCollection2);

                connection1.Open();
                connection2.Open();

                var storeCollection1 = workspace1.GetItemCollection(DataSpace.SSpace);
                var storeCollection2 = workspace2.GetItemCollection(DataSpace.SSpace);
                var mappingCollection1 = workspace1.GetItemCollection(DataSpace.CSSpace);
                var mappingCollection2 = workspace2.GetItemCollection(DataSpace.CSSpace);

                Assert.Same(storeCollection1, storeCollection2);
                Assert.Same(mappingCollection1, mappingCollection2);
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void Verify_that_metadata_is_different_for_two_workspaces_created_from_entity_connections_with_diffrent_connection_strings_although_model_is_equivalent()
        {
            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString1 = string.Format(CultureInfo.InvariantCulture, connectionStringDirectoryBasedTemplate, modelDirectory);
            var connectionString2 = string.Format(CultureInfo.InvariantCulture, connectionStringFileBasedTemplate, modelDirectory);

            try
            {
                this.DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection1 = new EntityConnection(connectionString1);
                var connection2 = new EntityConnection(connectionString2);
                MetadataWorkspace workspace1 = connection1.GetMetadataWorkspace();
                MetadataWorkspace workspace2 = connection2.GetMetadataWorkspace();

                var edmCollection1 = workspace1.GetItemCollection(DataSpace.CSpace);
                var edmCollection2 = workspace2.GetItemCollection(DataSpace.CSpace);

                Assert.NotSame(edmCollection1, edmCollection2);

                connection1.Open();
                connection2.Open();

                var storeCollection1 = workspace1.GetItemCollection(DataSpace.SSpace);
                var storeCollection2 = workspace2.GetItemCollection(DataSpace.SSpace);
                var mappingCollection1 = workspace1.GetItemCollection(DataSpace.CSSpace);
                var mappingCollection2 = workspace2.GetItemCollection(DataSpace.CSSpace);

                Assert.NotSame(storeCollection1, storeCollection2);
                Assert.NotSame(mappingCollection1, mappingCollection2);
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void Verify_that_conceptual_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_reordered_metadata_in_connection_strings()
        {
            var modelDirectory = "Model" + Guid.NewGuid();

            var connectionString1 = string.Format(CultureInfo.InvariantCulture, connectionStringFileBasedTemplate, modelDirectory);
            var connectionString2 = string.Format(CultureInfo.InvariantCulture, @"metadata={0}/Model.msl|{0}/Model.ssdl|{0}/Model.csdl;provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""", modelDirectory);

            try
            {
                this.DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection1 = new EntityConnection(connectionString1);
                var connection2 = new EntityConnection(connectionString2);
                var workspace1 = connection1.GetMetadataWorkspace();
                var workspace2 = connection2.GetMetadataWorkspace();

                var edmCollection1 = workspace1.GetItemCollection(DataSpace.CSpace);
                var edmCollection2 = workspace2.GetItemCollection(DataSpace.CSpace);

                Assert.Same(edmCollection1, edmCollection2);
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void Verify_that_opening_connection_does_not_create_new_MetadataWorkspace()
        {
            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringFileBasedTemplate, modelDirectory);

            try
            {
                this.DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection = new EntityConnection(connectionString);
                var workspace = connection.GetMetadataWorkspace();

                connection.Open();
                Assert.Same(workspace, connection.GetMetadataWorkspace());
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void Metadata_does_not_get_garbage_collected_if_references_are_alive()
        {
            Action garbageCollection = () => 
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            };

            this.MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
        }

        [Fact]
        public void Metadata_does_not_get_garbage_collected_after_cleanup_is_performed_once_if_references_are_alive()
        {
            Action garbageCollection = () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                CallPeriodicCleanupMethod();
            };
        }

        [Fact]
        public void Metadata_does_not_get_garbage_collected_after_cleanup_is_performed_twice_if_references_are_alive()
        {
            Action garbageCollection = () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                CallPeriodicCleanupMethod();
                CallPeriodicCleanupMethod();
            };
        }

        [Fact]
        public void Metadata_does_not_get_garbage_collected_after_cleanup_is_performed_thrice_if_references_are_alive()
        {
            Action garbageCollection = () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                CallPeriodicCleanupMethod();
                CallPeriodicCleanupMethod();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            };
        }

        private void MetadataCachingWithGarbageCollectionTemplate(Action garbageCollection)
        {
            MetadataWorkspace.ClearCache();
            WeakReference[] weakReferences = new WeakReference[3];

            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringDirectoryBasedTemplate, modelDirectory);

            try
            {
                DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                // load metadata
                using (EntityConnection connection1 = new EntityConnection(connectionString))
                {
                    connection1.Open();

                    weakReferences[0] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.CSpace));
                    weakReferences[1] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace));
                    weakReferences[2] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace));
                }

                // perform necessary garbage collection steps
                garbageCollection();

                // verify that metadata was cached
                using (EntityConnection connection2 = new EntityConnection(connectionString))
                {
                    connection2.Open();

                    Assert.Same((ItemCollection)weakReferences[0].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.CSpace));
                    Assert.Same((ItemCollection)weakReferences[1].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace));
                    Assert.Same((ItemCollection)weakReferences[2].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace));
                }
            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        [Fact]
        public void New_files_added_to_model_directory_are_properly_detected()
        {
            var csdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""MetadataCachingModel"" Alias=""Self"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2008/09/edm"">
  <EntityContainer Name=""Entities"" annotation:LazyLoadingEnabled=""true"">
    <EntitySet Name=""Customers"" EntityType=""MetadataCachingModel.Customer"" />
    <EntitySet Name=""Orders"" EntityType=""MetadataCachingModel.Order"" />
  </EntityContainer>
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
    <Property Name=""Name"" Type=""String"" Nullable=""false"" />
  </EntityType>
  <EntityType Name=""Order"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
  </EntityType>
</Schema>";

            MetadataWorkspace.ClearCache();

            var modelDirectory = "Model" + Guid.NewGuid();
            var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringDirectoryBasedTemplate, modelDirectory);

            try
            {
                DeployModelIntoDirectoryOnHardDrive(modelDirectory);

                var connection1 = new EntityConnection(connectionString);
                var edmCollection1 = connection1.GetMetadataWorkspace(false).GetItemCollection(DataSpace.CSpace);
                var itemCount1 = edmCollection1.Count();


                //modify csdl namespace
                File.Delete(modelDirectory + "//Model.csdl");
                File.WriteAllText(modelDirectory + "//Model2.csdl", csdl);

                var connection2 = new EntityConnection(connectionString);
                var edmCollection2 = connection1.GetMetadataWorkspace(false).GetItemCollection(DataSpace.CSpace);
                var itemCount2 = edmCollection2.Count();

                Assert.Equal(itemCount1, itemCount2);



                            // Forcing GC to kick in and garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            // Mark the entry for cleanup
            CallPeriodicCleanupMethod();
            // Remove the strong reference to item collection
            CallPeriodicCleanupMethod();
            GC.Collect(); // to make sure that the GC has collected the weak reference to item collection
            GC.WaitForPendingFinalizers();

        ////    // Now the cache entry state should be similar to a new entry

                var connection3 = new EntityConnection(connectionString);
                var edmCollection3 = connection1.GetMetadataWorkspace(false).GetItemCollection(DataSpace.CSpace);
                var itemCount3 = edmCollection3.Count();

                Assert.NotEqual(itemCount1, itemCount3);




        ////    Console.WriteLine("Open a new connection with some connection string");
        ////    EntityConnection connection = new EntityConnection(connectionString);
        ////    ItemCollection edmItemCollection = GetCurrentEdmItemCollection(connection);
        ////    Console.WriteLine("Number of items in the itemCollection = {0}", edmItemCollection.Count);

        ////    if (itemCount == edmItemCollection.Count)
        ////    {
        ////        throw new Exception("Since a new file was added, we should have detected this now");
        ////    }


            }
            finally
            {
                Directory.Delete(modelDirectory, true);
            }
        }

        internal static void CallPeriodicCleanupMethod()
        {
            MethodInfo method = typeof(MetadataCache).GetMethod("PeriodicCleanupCallback", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null);
            method.Invoke(null, new object[] { null });
        }

        ////[Test]
        ////public void DetectNewFilesAddition()
        ////{
        ////    Console.WriteLine("Detecting new files on the fly");

        ////    // Create the connection string
        ////    string connectionString = string.Format("metadata={0};provider={1};provider connection string=\"{2}\"",
        ////                                           ".\\",
        ////                                           provider,
        ////                                           providerConnStr);

        ////    int itemCount = LoadItemCollectionBeforeAndAfterAddingFilesToTheDirectory(connectionString, true, metadataSamplesPath);

        ////    // Forcing GC to kick in and garbage collection
        ////    GC.Collect();
        ////    GC.WaitForPendingFinalizers();
        ////    // Mark the entry for cleanup
        ////    CallPeriodicCleanupMethod();
        ////    // Remove the strong reference to item collection
        ////    CallPeriodicCleanupMethod();
        ////    GC.Collect(); // to make sure that the GC has collected the weak reference to item collection
        ////    GC.WaitForPendingFinalizers();

        ////    // Now the cache entry state should be similar to a new entry

        ////    Console.WriteLine("Open a new connection with some connection string");
        ////    EntityConnection connection = new EntityConnection(connectionString);
        ////    ItemCollection edmItemCollection = GetCurrentEdmItemCollection(connection);
        ////    Console.WriteLine("Number of items in the itemCollection = {0}", edmItemCollection.Count);

        ////    if (itemCount == edmItemCollection.Count)
        ////    {
        ////        throw new Exception("Since a new file was added, we should have detected this now");
        ////    }
        ////}

        ////[Test]
        ////public void DetectFileDeletes()
        ////{
        ////    Console.WriteLine("Making sure that if the file is deleted, the change is reflected at some point");

        ////    // Create the connection string
        ////    string connectionString = string.Format("metadata={0};provider={1};provider connection string=\"{2}\"",
        ////                                           ".\\",
        ////                                           provider,
        ////                                           providerConnStr);

        ////    int itemCount = LoadItemCollectionBeforeAndAfterAddingFilesToTheDirectory(connectionString, false, DataTestClass.metadataSamplesPath);

        ////    // Forcing GC to kick in and garbage collection
        ////    GC.Collect();
        ////    GC.WaitForPendingFinalizers();
        ////    // Mark the entry for cleanup
        ////    CallPeriodicCleanupMethod();
        ////    // Remove the strong reference to item collection
        ////    CallPeriodicCleanupMethod();
        ////    GC.Collect(); // to make sure that the GC has collected the weak reference to item collection
        ////    GC.WaitForPendingFinalizers();

        ////    // Now the cache entry state should be similar to a new entry

        ////    Console.WriteLine("Open a new connection with some connection string");
        ////    EntityConnection connection = new EntityConnection(connectionString);
        ////    ItemCollection edmItemCollection = GetCurrentEdmItemCollection(connection);
        ////    Console.WriteLine("Number of items in the itemCollection = {0}", edmItemCollection.Count);

        ////    if (itemCount == edmItemCollection.Count)
        ////    {
        ////        throw new Exception("Since a new file was added, we should have detected this now");
        ////    }
        ////}




            ////        Console.WriteLine("Open a new connection with some connection string");
            ////EntityConnection connection = new EntityConnection(connectionString);
            ////ItemCollection edmItemCollection = GetCurrentEdmItemCollection(connection);
            ////Console.WriteLine("Number of items in the itemCollection = {0}", edmItemCollection.Count);

            ////Console.WriteLine("Copying a new file into the current directory : TwoThree.Model.csdl");

            ////if (doFileAddition)
            ////{
            ////    // Make sure that the file is not already present. Add it, after loading the connection once
            ////    AddFileIfNotPresent(fileName, directory);
            ////}
            ////else
            ////{
            ////    // Make sure the file is present. Delete it after loading the connection for the first time
            ////    DeleteFileIfPresent(fileName);
            ////}

            ////Console.WriteLine("Trying to create another connection with the same connection string");
            ////EntityConnection connection1 = new EntityConnection(connectionString);
            
            ////ItemCollection edmItemCollection1 = GetCurrentEdmItemCollection(connection);
            ////Console.WriteLine("Number of items in the itemCollection = {0}", edmItemCollection1.Count);

            ////if (!object.ReferenceEquals(edmItemCollection, edmItemCollection1))
            ////{
            ////    throw new Exception("Both edm item collections should be the same");
            ////}

            ////return edmItemCollection1.Count;

        private void DeployModelIntoDirectoryOnHardDrive(string directoryPath)
        {
            var csdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""MetadataCachingModel"" Alias=""Self"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2008/09/edm"">
  <EntityContainer Name=""Entities"" annotation:LazyLoadingEnabled=""true"">
    <EntitySet Name=""Customers"" EntityType=""MetadataCachingModel.Customer"" />
  </EntityContainer>
  <EntityType Name=""Customer"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
    <Property Name=""Name"" Type=""String"" Nullable=""false"" />
  </EntityType>
</Schema>";

            var ssdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Namespace=""MetadataCachingModel.Store"" Alias=""Self"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" xmlns=""http://schemas.microsoft.com/ado/2009/02/edm/ssdl"">
  <EntityContainer Name=""MetadataCachingModelStoreContainer"">
    <EntitySet Name=""##Customers"" EntityType=""MetadataCachingModel.Store.##Customer"" store:Type=""Tables"" Schema=""dbo"" />
  </EntityContainer>
  <EntityType Name=""##Customer"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""int"" Nullable=""false"" StoreGeneratedPattern=""Identity"" />
    <Property Name=""Name"" Type=""nvarchar"" MaxLength=""20"" Nullable=""false"" />
  </EntityType>
</Schema>";

            var msl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2008/09/mapping/cs"">
  <EntityContainerMapping StorageEntityContainer=""MetadataCachingModelStoreContainer"" CdmEntityContainer=""Entities"">
    <EntitySetMapping Name=""Customers"">
      <EntityTypeMapping TypeName=""MetadataCachingModel.Customer"">
        <MappingFragment StoreEntitySet=""##Customers"">
          <ScalarProperty Name=""Id"" ColumnName=""Id"" />
          <ScalarProperty Name=""Name"" ColumnName=""Name"" />
        </MappingFragment>
      </EntityTypeMapping>
    </EntitySetMapping>
  </EntityContainerMapping>
</Mapping>";

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(directoryPath + "//Model.csdl", csdl);
            File.WriteAllText(directoryPath + "//Model.ssdl", ssdl);
            File.WriteAllText(directoryPath + "//Model.msl", msl);
        }
    }
}
