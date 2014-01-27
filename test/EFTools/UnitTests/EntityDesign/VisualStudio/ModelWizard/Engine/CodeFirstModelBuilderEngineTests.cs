
namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Xunit;

    public class CodeFirstModelBuilderEngineTests
    {
        [Fact]
        public void ProcessModel_validates_store_model()
        {
            var storeModel = EdmModel.CreateStoreModel(
                new DbProviderInfo("System.Data.SqlClient", "2012"), 
                SqlProviderServices.Instance.GetProviderManifest("2012"));

            storeModel.AddItem(EntityType.Create("E", "ns", DataSpace.SSpace, new string[0], new EdmMember[0], null));

            var model = CreateDbModel(null, storeModel);
            
            var errors = new List<EdmSchemaError>();
            new CodeFirstModelBuilderEngineInvoker()
                .InvokeProcessModel(model, null, null, null, errors);

            Assert.Equal(1, errors.Count);

            Assert.Contains(
                string.Format(Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType("E")), 
                errors.Single().Message);
        }

        [Fact]
        public void ProcessModel_validates_conceptual_model()
        {
            var conceptualModel = EdmModel.CreateConceptualModel();
            conceptualModel.AddItem(EntityType.Create("E", "ns", DataSpace.CSpace, new string[0], new EdmMember[0], null));

            var model = CreateDbModel(conceptualModel, null);

            var errors = new List<EdmSchemaError>();
            new CodeFirstModelBuilderEngineInvoker()
                .InvokeProcessModel(model, null, null, null, errors);

            Assert.Equal(1, errors.Count);

            Assert.Contains(
                string.Format(Strings.EdmModel_Validator_Semantic_KeyMissingOnEntityType("E")),
                errors.Single().Message);
        }

        private static DbModel CreateDbModel(EdmModel conceptualModel, EdmModel storeModel)
        {
            if (storeModel == null)
            {
                storeModel = EdmModel.CreateStoreModel(
                    new DbProviderInfo("System.Data.SqlClient", "2012"),
                    SqlProviderServices.Instance.GetProviderManifest("2012"));                
            }

            if (conceptualModel == null)
            {
                conceptualModel = EdmModel.CreateConceptualModel();
            }

            var databaseMapping = new DbDatabaseMapping
            {
                Database = storeModel,
                Model = conceptualModel
            };

            databaseMapping.AddEntityContainerMapping(new EntityContainerMapping(databaseMapping.Model.Container));

            return new DbModel(databaseMapping, new DbModelBuilder());
        }

        private class CodeFirstModelBuilderEngineInvoker : CodeFirstModelBuilderEngine
        {
            public void InvokeProcessModel(DbModel model, string storeModelNamespace, ModelBuilderSettings settings, 
            ModelBuilderEngineHostContext hostContext, List<EdmSchemaError> errors)
            {
                ProcessModel(model, storeModelNamespace, settings, hostContext, errors);
            }
        }
    }
}
