// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace MetadataCachingTests
{
    using System;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Reflection;
    using Xunit;

    public class MetadataCachingTests
    {
        private readonly static string connectionString = @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.msl;provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""";

        [Fact]
        public void Verify_that_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_same_connection_strings()
        {
            var connection1 = new EntityConnection(connectionString);
            var connection2 = new EntityConnection(connectionString);
            var workspace1 = connection1.GetMetadataWorkspace();
            var workspace2 = connection2.GetMetadataWorkspace();

            Assert.Same(workspace1.GetItemCollection(DataSpace.CSpace), workspace2.GetItemCollection(DataSpace.CSpace));
            Assert.Same(workspace1.GetItemCollection(DataSpace.SSpace), workspace2.GetItemCollection(DataSpace.SSpace));
            Assert.Same(workspace1.GetItemCollection(DataSpace.CSSpace), workspace2.GetItemCollection(DataSpace.CSSpace));
        }

        [Fact]
        public void Verify_that_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_equivalent_connection_strings()
        {
            var connection1 = new EntityConnection(connectionString);
            var connection2 = new EntityConnection(connectionString + ";;");

            var workspace1 = connection1.GetMetadataWorkspace();
            var workspace2 = connection2.GetMetadataWorkspace();

            Assert.Same(workspace1.GetItemCollection(DataSpace.CSpace), workspace2.GetItemCollection(DataSpace.CSpace));
            Assert.Same(workspace1.GetItemCollection(DataSpace.SSpace), workspace2.GetItemCollection(DataSpace.SSpace));
            Assert.Same(workspace1.GetItemCollection(DataSpace.CSSpace), workspace2.GetItemCollection(DataSpace.CSSpace));
        }

        [Fact]
        public void Verify_that_conceptual_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_reordered_metadata_in_connection_strings()
        {
            var connectionString2 = @"metadata=res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.msl|res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Metadata.MetadataCachingModel.csdl;provider=System.Data.SqlClient;provider connection string=""Data Source=.\sqlexpress;Initial Catalog=tempdb;Integrated Security=True""";

            var connection1 = new EntityConnection(connectionString);
            var connection2 = new EntityConnection(connectionString2);
            var workspace1 = connection1.GetMetadataWorkspace();
            var workspace2 = connection2.GetMetadataWorkspace();

            Assert.Same(workspace1.GetItemCollection(DataSpace.CSpace), workspace2.GetItemCollection(DataSpace.CSpace));
        }

        [Fact]
        public void Verify_that_opening_connection_does_not_create_new_MetadataWorkspace()
        {
            var connection = new EntityConnection(connectionString);
            var workspace = connection.GetMetadataWorkspace();

            connection.Open();
            Assert.Same(workspace, connection.GetMetadataWorkspace());
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

            this.MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
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

            this.MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
        }

        private void MetadataCachingWithGarbageCollectionTemplate(Action garbageCollection)
        {
            MetadataWorkspace.ClearCache();
            WeakReference[] weakReferences = new WeakReference[3];

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

        internal static void CallPeriodicCleanupMethod()
        {
            MethodInfo method = typeof(MetadataCache).GetMethod("PeriodicCleanupCallback", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null);
            method.Invoke(null, new object[] { null });
        }
    }
}
