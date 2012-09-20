// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using Moq;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;
    using Xunit;

    public static class QueryTestHelpers
    {
        public static MetadataWorkspace CreateMetadataWorkspace(string csdl, string ssdl, string msl)
        {
            var edmItemCollection = new EdmItemCollection(new XmlReader[] { XmlReader.Create(new StringReader(csdl)) });
            var storeItemCollection = new StoreItemCollection(new XmlReader[] { XmlReader.Create(new StringReader(ssdl)) });
            var storageMappingItemCollection = new StorageMappingItemCollection(edmItemCollection, storeItemCollection, new XmlReader[] { XmlReader.Create(new StringReader(msl)) });

            var metadataWorkspaceMock = new Mock<MetadataWorkspace> { CallBase = true };
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSpace, It.IsAny<bool>())).Returns(edmItemCollection);
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace, It.IsAny<bool>())).Returns(storeItemCollection);
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSSpace, It.IsAny<bool>())).Returns(storageMappingItemCollection);

            return metadataWorkspaceMock.Object;
        }

        public static void VerifyQuery(DbExpression query, MetadataWorkspace workspace, string expectedSql)
        {
            var providerServices = (DbProviderServices)((IServiceProvider)EntityProviderFactory.Instance).GetService(typeof(DbProviderServices));
            var connection = new EntityConnection(workspace, new EntityConnection());
            var commandTree = workspace.CreateQueryCommandTree(query);

            var entityCommand = (EntityCommand)providerServices.CreateCommandDefinition(commandTree).CreateCommand();
            entityCommand.Connection = connection;

            Assert.Equal(StripFormatting(expectedSql), StripFormatting(entityCommand.ToTraceString()));
        }

        public static void VerifyQuery(string query, MetadataWorkspace workspace, string expectedSql)
        {
            var providerServices = (DbProviderServices)((IServiceProvider)EntityProviderFactory.Instance).GetService(typeof(DbProviderServices));
            var connection = new EntityConnection(workspace, new EntityConnection());
            var commandTree = workspace.CreateEntitySqlParser().Parse(query).CommandTree;

            var entityCommand = (EntityCommand)providerServices.CreateCommandDefinition(commandTree).CreateCommand();
            entityCommand.Connection = connection;

            Assert.Equal(StripFormatting(expectedSql), StripFormatting(entityCommand.ToTraceString()));
        }

        public static void VerifyThrows<TException>(string query, MetadataWorkspace workspace, string expectedExeptionMessage)
        {
            bool exceptionThrown = false;
            try
            {
                var providerServices = (DbProviderServices)((IServiceProvider)EntityProviderFactory.Instance).GetService(typeof(DbProviderServices));
                var connection = new EntityConnection(workspace, new EntityConnection());
                var commandTree = workspace.CreateEntitySqlParser().Parse(query).CommandTree;
                var entityCommand = (EntityCommand)providerServices.CreateCommandDefinition(commandTree).CreateCommand();
                entityCommand.Connection = connection;
                entityCommand.ToTraceString();
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                var innermostException = GetInnerMostException(e);
                Assert.IsType<TException>(innermostException);
                Assert.Equal(expectedExeptionMessage, innermostException.Message);
            }

            Assert.True(exceptionThrown, "No excepion has been thrown.");
        }

        private static Exception GetInnerMostException(Exception exception)
        {
            if (exception == null) 
            {
                return exception;
            }

            var currectException = exception;
            while (currectException.InnerException != null)
            {
                currectException = currectException.InnerException;
            }

            return currectException;
        }

        private static string StripFormatting(string argument)
        {
            return argument.Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace(" ", "");
        }
    }
}
