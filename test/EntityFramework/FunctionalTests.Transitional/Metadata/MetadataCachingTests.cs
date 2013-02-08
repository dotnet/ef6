// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Reflection;
    using Xunit;

    public class MetadataCachingTests : FunctionalTestBase
    {
        private static readonly string connectionString = string.Format(
            @"metadata=res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.csdl|res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.ssdl|res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.msl;provider=System.Data.SqlClient;provider connection string=""{0}""",
            ModelHelpers.SimpleConnectionString("tempdb"));

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
        public void
            Verify_that_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_equivalent_connection_strings()
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
        public void
            Verify_that_conceptual_metadata_is_the_same_for_two_workspaces_created_from_two_entity_connections_with_reordered_metadata_in_connection_strings
            ()
        {
            var connectionString2 = string.Format(
                 @"metadata=res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.msl|res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.ssdl|res://EntityFramework.FunctionalTests.Transitional/System.Data.Entity.Metadata.MetadataCachingModel.csdl;provider=System.Data.SqlClient;provider connection string=""{0}""",
                ModelHelpers.SimpleConnectionString("tempdb"));

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

            MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
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

            MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
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

            MetadataCachingWithGarbageCollectionTemplate(garbageCollection);
        }

        private void MetadataCachingWithGarbageCollectionTemplate(Action garbageCollection)
        {
            MetadataWorkspace.ClearCache();
            var weakReferences = new WeakReference[3];

            // load metadata
            using (var connection1 = new EntityConnection(connectionString))
            {
                connection1.Open();

                weakReferences[0] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.CSpace));
                weakReferences[1] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace));
                weakReferences[2] = new WeakReference(connection1.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace));
            }

            // perform necessary garbage collection steps
            garbageCollection();

            // verify that metadata was cached
            using (var connection2 = new EntityConnection(connectionString))
            {
                connection2.Open();

                Assert.Same(weakReferences[0].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.CSpace));
                Assert.Same(weakReferences[1].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace));
                Assert.Same(weakReferences[2].Target, connection2.GetMetadataWorkspace().GetItemCollection(DataSpace.CSSpace));
            }
        }

        internal static void CallPeriodicCleanupMethod()
        {
            var method = typeof(MetadataCache).GetMethod(
                "PeriodicCleanupCallback", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(object) }, null);
            method.Invoke(null, new object[] { null });
        }
    }
}
