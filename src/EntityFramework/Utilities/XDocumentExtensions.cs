// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XDocumentExtensions
    {
        public static StorageMappingItemCollection GetStorageMappingItemCollection(
            this XDocument model, out DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(model);

            var edmItemCollection
                = new EdmItemCollection(
                    new[]
                        {
                            model.Descendants(EdmXNames.Csdl.SchemaNames).Single().CreateReader()
                        });

            var ssdlSchemaElement = model.Descendants(EdmXNames.Ssdl.SchemaNames).Single();

            providerInfo = new DbProviderInfo(
                ssdlSchemaElement.ProviderAttribute(),
                ssdlSchemaElement.ProviderManifestTokenAttribute());

            var storeItemCollection
                = new StoreItemCollection(
                    new[]
                        {
                            ssdlSchemaElement.CreateReader()
                        });

            var msl = new XElement(model.Descendants(EdmXNames.Msl.MappingNames).Single());

            // normalize EDM namespaces
            // TODO: This can be removed when we stop appending the history model.
            var schemaNamespace
                = model
                    .Descendants(EdmXNames.Csdl.SchemaNames)
                    .Single()
                    .NamespaceAttribute();

            msl.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
               .Where(e => e.IsSystem())
               .Each(
                   e =>
                       {
                           var typeNameTemplate = "{0}";
                           var typeNameAttribute = e.TypeNameAttribute();

                           if (typeNameAttribute.StartsWith(StorageMslConstructs.IsTypeOf, StringComparison.Ordinal))
                           {
                               typeNameAttribute
                                   = typeNameAttribute.Substring(
                                       StorageMslConstructs.IsTypeOf.Length,
                                       typeNameAttribute.Length
                                       - StorageMslConstructs.IsTypeOf.Length
                                       - StorageMslConstructs.IsTypeOfTerminal.Length);

                               typeNameTemplate
                                   = StorageMslConstructs.IsTypeOf
                                     + typeNameTemplate
                                     + StorageMslConstructs.IsTypeOfTerminal;
                           }

                           e.SetAttributeValue(
                               StorageMslConstructs.EntityTypeMappingTypeNameAttribute,
                               string.Format(
                                   CultureInfo.InvariantCulture,
                                   typeNameTemplate,
                                   schemaNamespace + '.' + typeNameAttribute.Split('.').Last()));
                       });

            // MSL does not allow arbitrary content
            // TODO: This can be removed when we stop appending the history model.
            msl.DescendantsAndSelf().Attributes(EdmXNames.IsSystemName).Remove();

            return new StorageMappingItemCollection(
                edmItemCollection,
                storeItemCollection,
                new[] { msl.CreateReader() });
        }

        public static bool HasSystemOperations(this XDocument model)
        {
            DebugCheck.NotNull(model);

            return model.Descendants().Attributes(EdmXNames.IsSystemName).Any();
        }
    }
}
