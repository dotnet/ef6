namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public class ObjectQueryExecutionPlanFactoryTests
    {
        [Fact]
        public void Prepare_returns_a_new_instance()
        {
            var objectQueryExecutionPlanFactory = new ObjectQueryExecutionPlanFactory(
                Common.Internal.Materialization.MockHelper.CreateTranslator<object>());

            var metadataWorkspace = new MetadataWorkspace();
            var edmItemCollection = new EdmItemCollection();
            metadataWorkspace.RegisterItemCollection(edmItemCollection);
            metadataWorkspace.RegisterItemCollection(new ObjectItemCollection());
            var fakeSqlProviderManifest = new FakeSqlProviderServices().GetProviderManifest("2008");
            var storeItemCollection = new StoreItemCollection(FakeSqlProviderFactory.Instance, fakeSqlProviderManifest, "2008");
            metadataWorkspace.RegisterItemCollection(storeItemCollection);
            metadataWorkspace.RegisterItemCollection(new StorageMappingItemCollection(edmItemCollection, storeItemCollection, Enumerable.Empty<XmlReader>()));

            var fakeSqlConnection = new FakeSqlConnection();
            fakeSqlConnection.ConnectionString = "foo";
            var entityConnection = new EntityConnection(metadataWorkspace, fakeSqlConnection, false);

            var objectContext = new ObjectContext(entityConnection);
            var dbExpression = new DbNullExpression(TypeUsage.Create(fakeSqlProviderManifest.GetStoreTypes().First()));
            var dbQueryCommandTree = new DbQueryCommandTree(metadataWorkspace, DataSpace.CSpace,
               dbExpression, validate: false);
            var parameters = new List<Tuple<ObjectParameter, QueryParameterExpression>>();

            var objectQueryExecutionPlan = objectQueryExecutionPlanFactory.Prepare(objectContext, dbQueryCommandTree, typeof(object),
                MergeOption.NoTracking, new Span(), parameters, aliasGenerator: null);

            Assert.NotNull(objectQueryExecutionPlan);
        }
    }
}