// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VersioningFacade.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class CodeFirstModelBuilderEngine : ModelBuilderEngine
    {
        protected override void ProcessModel(DbModel model, string storeModelNamespace, ModelBuilderSettings settings, 
            ModelBuilderEngineHostContext hostContext, List<EdmSchemaError> errors)
        {
            ValidateModel(model, errors);
        }

        private static void ValidateModel(DbModel model, List<EdmSchemaError> errors)
        {
            var settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };
            using (var writer = XmlWriter.Create(new StringBuilder(), settings))
            {
                var ssdlSerializer = new SsdlSerializer();
                ssdlSerializer.OnError +=
                    CreateOnErrorEventHandler(errors, ErrorCodes.GenerateModelFromDbReverseEngineerStoreModelFailed);

                ssdlSerializer.Serialize(
                    model.StoreModel,
                    model.ProviderInfo.ProviderInvariantName,
                    model.ProviderInfo.ProviderManifestToken,
                    writer);

                var csdlSerializer = new CsdlSerializer();
                csdlSerializer.OnError +=
                    CreateOnErrorEventHandler(errors, ErrorCodes.GenerateModelFromDbInvalidConceptualModel);

                csdlSerializer.Serialize(model.ConceptualModel, writer);

                new MslSerializerWrapper().Serialize(model, writer);
            }
        }

        private static EventHandler<DataModelErrorEventArgs> CreateOnErrorEventHandler(List<EdmSchemaError> errors, int errorCode)
        {
            return (sender, errorEventArgs) =>
                    errors.Add(
                        new EdmSchemaError(
                            errorEventArgs.ErrorMessage,
                            errorCode,
                            EdmSchemaErrorSeverity.Error));
        }
    }
}
