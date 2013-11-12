// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public static class ModelHelpers
    {
        #region State entry helpers

        /// <summary>
        /// Gets all GetStateEntries for the given DbContext.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <returns> All state entries in the ObjectStateManager. </returns>
        public static IEnumerable<ObjectStateEntry> GetStateEntries(DbContext dbContext)
        {
            return GetStateEntries(TestBase.GetObjectContext(dbContext));
        }

        /// <summary>
        /// Gets all GetStateEntries for the given ObjectContext.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <returns> All state entries in the ObjectStateManager. </returns>
        public static IEnumerable<ObjectStateEntry> GetStateEntries(ObjectContext objectContext)
        {
            return objectContext.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        /// <returns> The ObjectStateEntry. </returns>
        public static ObjectStateEntry GetStateEntry(DbContext dbContext, object entity)
        {
            return GetStateEntry(TestBase.GetObjectContext(dbContext), entity);
        }

        /// <summary>
        /// Gets the ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        /// <returns> The ObjectStateEntry. </returns>
        public static ObjectStateEntry GetStateEntry(ObjectContext objectContext, object entity)
        {
            return objectContext.ObjectStateManager.GetObjectStateEntry(entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given DbContext.
        /// </summary>
        /// <param name="dbContext"> A DbContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        public static void AssertNoStateEntry(DbContext dbContext, object entity)
        {
            AssertNoStateEntry(TestBase.GetObjectContext(dbContext), entity);
        }

        /// <summary>
        /// Asserts that there's no ObjectStateEntry for the given entity in the given ObjectContext.
        /// </summary>
        /// <param name="objectContext"> A ObjectContext instance. </param>
        /// <param name="entity"> The entity to lookup. </param>
        public static void AssertNoStateEntry(ObjectContext objectContext, object entity)
        {
            ObjectStateEntry entry;
            Assert.False(
                objectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out entry),
                "The context contains an unexpected entry for the given entity");
        }

        #endregion

        #region Connection helpers

        private static readonly string _baseConnectionString = ConfigurationManager.AppSettings["BaseConnectionString"]
                                                               ?? @"Data Source=.\SQLEXPRESS; Integrated Security=True; MultipleActiveResultSets=True;";

        public static string BaseConnectionString
        {
            get { return _baseConnectionString; }
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine with the given database name.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <returns> The connection string. </returns>
        public static string SimpleConnectionString(string databaseName)
        {
            return new SqlConnectionStringBuilder(_baseConnectionString)
                {
                    InitialCatalog = databaseName
                }
                .ConnectionString;
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using an attachable database
        /// with the given database name.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="useInitialCatalog">
        /// Specifies whether the InitialCatalog should be created from the context name.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleAttachConnectionString(string databaseName, bool useInitialCatalog = true)
        {
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseName + ".mdf");

            return new SqlConnectionStringBuilder(_baseConnectionString)
                {
                    InitialCatalog = useInitialCatalog ? databaseName : string.Empty,
                    AttachDBFilename = databasePath,
                    UserInstance = !useInitialCatalog
                }.ConnectionString;
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using an attachable database
        /// with the given database name and the specified credentials.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleAttachConnectionStringWithCredentials(
            string databaseName,
            string userId,
            string password,
            bool persistSecurityInfo = false)
        {
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseName + ".mdf");

            var builder = new SqlConnectionStringBuilder(_baseConnectionString)
                {
                    InitialCatalog = databaseName,
                    AttachDBFilename = databasePath,
                    UserID = userId,
                    Password = password,
                    PersistSecurityInfo = persistSecurityInfo
                };
            builder.Remove("Integrated Security");

            return builder.ConnectionString;
        }

        /// <summary>
        /// Returns a simple SQL Server connection string with the specified credentials.
        /// </summary>
        /// <param name="databaseName"> The database name. </param>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleConnectionStringWithCredentials(
            string databaseName,
            string userId,
            string password,
            bool persistSecurityInfo = false)
        {
            var builder = new SqlConnectionStringBuilder(_baseConnectionString)
                {
                    InitialCatalog = databaseName,
                    UserID = userId,
                    Password = password,
                    PersistSecurityInfo = persistSecurityInfo
                };
            builder.Remove("Integrated Security");

            return builder.ConnectionString;
        }

        /// <summary>
        /// Returns a simple SQL CE connection string to the local machine with the given database name.
        /// </summary>
        /// <param name="databaseName"> Name of the database. </param>
        /// <returns> The connection string. </returns>
        public static string SimpleCeConnectionString(string databaseName)
        {
            return String.Format(
                CultureInfo.InvariantCulture, "Data Source={0}.sdf;Persist Security Info=False;",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseName));
        }

        /// <summary>
        /// Returns the default name that will be created for the context of the given type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a name for. </typeparam>
        /// <returns> The name. </returns>
        public static string DefaultDbName<TContext>() where TContext : DbContext
        {
            return typeof(TContext).FullName;
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection string for. </typeparam>
        /// <returns> The connection string. </returns>
        public static string SimpleConnectionString<TContext>() where TContext : DbContext
        {
            return SimpleConnectionString(DefaultDbName<TContext>());
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine for the given context type
        /// with the specified credentials.
        /// </summary>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleConnectionStringWithCredentials<TContext>(
            string userId,
            string password,
            bool persistSecurityInfo = false)
            where TContext : DbContext
        {
            return SimpleConnectionStringWithCredentials(
                DefaultDbName<TContext>(),
                userId,
                password,
                persistSecurityInfo);
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using an attachable database
        /// for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection string for. </typeparam>
        /// <param name="useInitialCatalog">
        /// Specifies whether the InitialCatalog should be created from the context name.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleAttachConnectionString<TContext>(bool useInitialCatalog = true) where TContext : DbContext
        {
            return SimpleAttachConnectionString(DefaultDbName<TContext>(), useInitialCatalog);
        }

        /// <summary>
        /// Returns a simple SQL Server connection string to the local machine using an attachable database
        /// for the given context type with the specified credentials.
        /// </summary>
        /// <param name="userId"> User ID to be use when connecting to SQL Server. </param>
        /// <param name="password"> Password for the SQL Server account. </param>
        /// <param name="persistSecurityInfo">
        /// Indicates if security-sensitive information is not returned as part of the 
        /// connection if the connection has ever been opened.
        /// </param>
        /// <returns> The connection string. </returns>
        public static string SimpleAttachConnectionStringWithCredentials<TContext>(
            string userId,
            string password,
            bool persistSecurityInfo = false) where TContext : DbContext
        {
            return SimpleAttachConnectionStringWithCredentials(
                DefaultDbName<TContext>(),
                userId,
                password,
                persistSecurityInfo);
        }

        /// <summary>
        /// Returns a simple SQLCE connection string to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection string for. </typeparam>
        /// <returns> The connection string. </returns>
        public static string SimpleCeConnectionString<TContext>() where TContext : DbContext
        {
            return SimpleCeConnectionString(DefaultDbName<TContext>());
        }

        /// <summary>
        /// Returns a simple SQL Server connection to the local machine for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection for. </typeparam>
        /// <returns> The connection. </returns>
        public static SqlConnection SimpleConnection<TContext>() where TContext : DbContext
        {
            return new SqlConnection(SimpleConnectionString<TContext>());
        }

        /// <summary>
        /// Returns a simple SQL CE connection for the given context type.
        /// </summary>
        /// <typeparam name="TContext"> The type of the context to create a connection for. </typeparam>
        /// <returns> The connection. </returns>
        public static DbConnection SimpleCeConnection<TContext>() where TContext : DbContext
        {
            return
                new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", AppDomain.CurrentDomain.BaseDirectory, "").
                    CreateConnection(DefaultDbName<TContext>());
        }

        #endregion

        #region Entity set name helpers

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="dbContext"> The context to look in. </param>
        /// <param name="clrType"> The type to lookup. </param>
        /// <returns> The entity set name. </returns>
        public static string GetEntitySetName(DbContext dbContext, Type clrType)
        {
            return GetEntitySetName(TestBase.GetObjectContext(dbContext), clrType);
        }

        /// <summary>
        /// Gets the entity set name for the given CLR type, assuming no MEST.
        /// </summary>
        /// <param name="objectContext"> The context to look in. </param>
        /// <param name="clrType"> The type to lookup. </param>
        /// <returns> The entity set name. </returns>
        public static string GetEntitySetName(ObjectContext objectContext, Type clrType)
        {
            var cspaceType = GetStructuralType<EntityType>(objectContext, clrType);
            if (cspaceType == null)
            {
                return null;
            }

            var inverseHierarchy = new Stack<EntityType>();
            do
            {
                inverseHierarchy.Push(cspaceType);
                cspaceType = (EntityType)cspaceType.BaseType;
            }
            while (cspaceType != null);

            while (inverseHierarchy.Count > 0)
            {
                cspaceType = inverseHierarchy.Pop();
                foreach (var container in objectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace))
                {
                    var entitySet = container.BaseEntitySets.Where(s => s.ElementType == cspaceType).FirstOrDefault();
                    if (entitySet != null)
                    {
                        return entitySet.Name;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Entity type helpers

        /// <summary>
        /// Gets the Entity Type of the entity, given the CLR type
        /// </summary>
        /// <param name="dbContext"> The context to look in. </param>
        /// <param name="clrType"> Type of the CLR. </param>
        /// <returns> </returns>
        public static EntityType GetEntityType(DbContext dbContext, Type clrType)
        {
            return GetStructuralType<EntityType>(TestBase.GetObjectContext(dbContext), clrType);
        }

        /// <summary>
        /// Gets the structural type of the entity type or complex type given the CLR type
        /// </summary>
        /// <param name="objectContext"> The context to look in. </param>
        /// <param name="clrType"> The CLR type. </param>
        /// <returns> The EntityType or ComplexType </returns>
        public static TStructural GetStructuralType<TStructural>(ObjectContext objectContext, Type clrType)
            where TStructural : StructuralType
        {
            var objectItemCollection =
                (ObjectItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
            var ospaceTypes = objectContext.MetadataWorkspace.GetItems<TStructural>(DataSpace.OSpace);
            var ospaceType = ospaceTypes.Where(t => objectItemCollection.GetClrType(t) == clrType).FirstOrDefault();
            if (ospaceType == null)
            {
                objectContext.MetadataWorkspace.LoadFromAssembly(clrType.Assembly);
                ospaceType = ospaceTypes.Where(t => objectItemCollection.GetClrType(t) == clrType).FirstOrDefault();
                if (ospaceType == null)
                {
                    return null;
                }
            }

            return (TStructural)objectContext.MetadataWorkspace.GetEdmSpaceType(ospaceType);
        }

        #endregion

        #region Helpers for creating metadata (csdl/ssdl/msl) files

        /// <summary>
        /// Writes an edmx file into the current directory for the model generated from the given model builder.
        /// </summary>
        /// <param name="builder"> The builder. </param>
        /// <param name="filename"> The filename to use for the edmx file. </param>
        public static void WriteEdmx(DbModelBuilder builder, string filename)
        {
            EdmxWriter.WriteEdmx(
                builder.Build(new DbProviderInfo("System.Data.SqlClient", "2008")),
                XmlWriter.Create(filename));
        }

        /// <summary>
        /// Writes csdl, msdl, and ssdl files into the current directory for the model generated from
        /// the given model builder.
        /// </summary>
        /// <param name="builder"> The builder. </param>
        /// <param name="filename"> The base filename to use for csdl, msdl, and sssl files. </param>
        public static void WriteMetadataFiles(DbModelBuilder builder, string filename)
        {
            var xml = new StringBuilder();
            EdmxWriter.WriteEdmx(
                builder.Build(new DbProviderInfo("System.Data.SqlClient", "2008")),
                XmlWriter.Create(xml));

            WriteMetadataFiles(xml.ToString(), filename);
        }

        /// <summary>
        /// Takes the edmx given as input and splits it into csdl, msl, and ssdl files that are written to the
        /// current directory.
        /// </summary>
        /// <param name="edmx"> The edmx. (Note that this is NOT the filename of an edmx file; it is the actual edmx.) </param>
        /// <param name="filename"> The base filename to use for csdl, msdl, and sssl files. </param>
        public static void WriteMetadataFiles(string edmx, string filename)
        {
            var csdlNameV2 = (XNamespace)"http://schemas.microsoft.com/ado/2008/09/edm" + "Schema";
            var ssdlNameV2 = (XNamespace)"http://schemas.microsoft.com/ado/2009/02/edm/ssdl" + "Schema";
            var mslNameV2 = (XNamespace)"http://schemas.microsoft.com/ado/2008/09/mapping/cs" + "Mapping";

            var csdlNameV3 = (XNamespace)"http://schemas.microsoft.com/ado/2009/11/edm" + "Schema";
            var ssdlNameV3 = (XNamespace)"http://schemas.microsoft.com/ado/2009/11/edm/ssdl" + "Schema";
            var mslNameV3 = (XNamespace)"http://schemas.microsoft.com/ado/2009/11/mapping/cs" + "Mapping";

            var edmxDoc = XDocument.Load(new StringReader(edmx));

            WriteMetadataFile(
                filename + ".csdl",
                ExtractMetadataContent(edmxDoc, "ConceptualModels", csdlNameV2, csdlNameV3));
            WriteMetadataFile(
                filename + ".ssdl",
                ExtractMetadataContent(edmxDoc, "StorageModels", ssdlNameV2, ssdlNameV3));
            WriteMetadataFile(filename + ".msl", ExtractMetadataContent(edmxDoc, "Mappings", mslNameV2, mslNameV3));
        }

        private static void WriteMetadataFile(string filename, XElement element)
        {
            Debug.Assert(element != null, "Expected to find element");

            using (var writer = XmlWriter.Create(filename))
            {
                element.Save(writer);
            }
        }

        private static XElement ExtractMetadataContent(XDocument edmxDoc, string part, params XName[] elements)
        {
            XNamespace edmxnsV2 = "http://schemas.microsoft.com/ado/2008/10/edmx";
            XNamespace edmxnsV3 = "http://schemas.microsoft.com/ado/2009/11/edmx";

            var edmxNode = edmxDoc.Element(edmxnsV2 + "Edmx") ?? edmxDoc.Element(edmxnsV3 + "Edmx");
            Debug.Assert(edmxNode != null, "Expected to find edmx node.");

            var runtimeNode = edmxNode.Element(edmxnsV2 + "Runtime") ?? edmxNode.Element(edmxnsV3 + "Runtime");
            Debug.Assert(runtimeNode != null, "Expected to find runtime node.");

            var partNode = runtimeNode.Element(edmxnsV2 + part) ?? runtimeNode.Element(edmxnsV3 + part);
            Debug.Assert(partNode != null, "Expected to find " + part);

            return partNode.Element(elements[0]) ?? partNode.Element(elements[1]);
        }

        #endregion
    }
}
