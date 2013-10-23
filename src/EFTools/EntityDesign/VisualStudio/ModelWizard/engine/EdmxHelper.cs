// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VersioningFacade.Serialization;

    internal class EdmxHelper
    {
        private readonly XDocument _edmx;

        public EdmxHelper(XDocument edmx)
        {
            Debug.Assert(edmx != null, "edmx != null");

            _edmx = edmx;
        }

        public void UpdateEdmxFromModel(
            DbModel model, string storeModelNamespace,
            string entityModelNamespace, List<EdmSchemaError> errors)
        {
            Debug.Assert(model != null, "model != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(storeModelNamespace),
                "storeModelNamespace must not be null or empty string");

            // update the EDMX - stop if any attempt to update fails   
            if (UpdateStorageModels(model.GetStoreModel(), storeModelNamespace, model.ProviderInfo, errors)
                && UpdateConceptualModels(model.GetConceptualModel(), entityModelNamespace))
            {
                UpdateMapping(model);
            }
        }

        // internal virtual for testing
        internal virtual bool UpdateStorageModels(
            EdmModel storeModel, string storeModelNamespace, DbProviderInfo providerInfo, List<EdmSchemaError> errors)
        {
            Debug.Assert(storeModel != null, "storeModel != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(storeModelNamespace),
                "storeModelNamespace must not be null or empty string");

            var serializer = new SsdlSerializer();
            serializer.OnError +=
                (sender, errorEventArgs) =>
                errors.Add(
                    new EdmSchemaError(
                    errorEventArgs.ErrorMessage, ErrorCodes.GenerateModelFromDbReverseEngineerStoreModelFailed,
                    EdmSchemaErrorSeverity.Error));

            return ReplaceEdmxSection(
                _edmx,
                "StorageModels",
                writer => serializer.Serialize(
                    storeModel,
                    storeModelNamespace,
                    providerInfo.ProviderInvariantName,
                    providerInfo.ProviderManifestToken,
                    writer,
                    serializeDefaultNullability: false));
        }

        // internal virtual for testing
        internal virtual bool UpdateConceptualModels(EdmModel entityModel, string modelNamespace)
        {
            Debug.Assert(entityModel != null, "entityModel != null");

            return ReplaceEdmxSection(
                _edmx,
                "ConceptualModels",
                writer => new CsdlSerializer().Serialize(entityModel, writer, modelNamespace));
        }

        // internal virtual for testing
        internal virtual bool UpdateMapping(DbModel model)
        {
            Debug.Assert(model != null, "model != null");

            return ReplaceEdmxSection(
                _edmx,
                "Mappings",
                writer => new MslSerializerWrapper().Serialize(model, writer));
        }

        internal virtual void UpdateDesignerOptionProperty<T>(string optionName, T value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(optionName), "optionName must not be null or empty string.");

            var edmxNs = _edmx.Root.Name.Namespace;

            var options =
                _edmx.Root.Element(edmxNs + "Designer")
                    .Element(edmxNs + "Options");

            var optionInfoPropertySet = options
                .Element(edmxNs + "DesignerInfoPropertySet");
            if (optionInfoPropertySet == null)
            {
                optionInfoPropertySet = new XElement(edmxNs + "DesignerInfoPropertySet");
                options.Add(optionInfoPropertySet);
            }

            var optionElement =
                optionInfoPropertySet
                    .Elements(edmxNs + "DesignerProperty")
                    .SingleOrDefault(e => (string)e.Attribute("Name") == optionName);

            if (optionElement == null)
            {
                optionElement = new XElement(edmxNs + "DesignerProperty");
                optionInfoPropertySet.Add(optionElement);
            }

            optionElement.SetAttributeValue("Name", optionName);
            optionElement.SetAttributeValue("Value", value);
        }

        private static bool ReplaceEdmxSection(XDocument edmx, string elementLocalName, Func<XmlWriter, bool> writeContent)
        {
            Debug.Assert(edmx != null, "edmx != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(elementLocalName),
                "elementLocalName must not be null or empty string");
            Debug.Assert(writeContent != null, "writeContent != null");

            var modelElement =
                edmx.Descendants(edmx.Root.Name.Namespace + elementLocalName).Single();
            var existingChildElements = modelElement.Elements();

            using (var writer = modelElement.CreateWriter())
            {
                if (writeContent(writer))
                {
                    existingChildElements.Remove();
                    return true;
                }

                return false;
            }
        }
    }
}
