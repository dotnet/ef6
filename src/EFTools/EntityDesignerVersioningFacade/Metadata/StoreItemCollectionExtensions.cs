// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;

    internal static class StoreItemCollectionExtensions
    {
        public static void WriteSsdl(this StoreItemCollection storeItemCollection, XmlWriter writer, string schemaNamespaceName = null)
        {
            Debug.Assert(storeItemCollection != null, "storeItemCollection != null");
            Debug.Assert(writer != null, "writer != null");

            // In the StoreItemCollection all non-primitive types (either EntityType or AssociationType) are in the same namespace. 
            // If there is any EntityType we should use the namespace of this type and it should win over the namespace defined by the user
            // otherwise we could "move" all the types to the namespace defined by the user since in the SsdlWriter we use the namespace
            // alias when writing the type (note that if we used the namespace instead of alias we could create an invalid Ssdl since 
            // the types would be in a different (and undefined) namespace then the one written on the Schema element).
            // To infer the namespace we use EntityType because you any other type with a namespace (e.g. EntitySet, AssociationType etc.)
            // refer to an entity type. If there are no entity types we will use the namespace name provided by the user or - if it is null
            // or empty - we will an arbitrary one (at this point the schema name does not really matter since the collection 
            // does not have any type that would use it)
            var entityType = storeItemCollection.GetItems<EntityType>().FirstOrDefault();
            if (entityType != null)
            {
                schemaNamespaceName = entityType.NamespaceName;
            }
            else if (string.IsNullOrWhiteSpace(schemaNamespaceName))
            {
                schemaNamespaceName = "Model.Store";
            }

            new SsdlSerializer()
                .Serialize(
                    storeItemCollection.ToEdmModel(),
                    schemaNamespaceName,
                    storeItemCollection.ProviderInvariantName,
                    storeItemCollection.ProviderManifestToken,
                    writer,
                    serializeDefaultNullability: false);
        }

        public static EdmModel ToEdmModel(this StoreItemCollection storeItemCollection)
        {
            Debug.Assert(storeItemCollection != null, "storeItemCollection != null");

            var container = storeItemCollection.GetItems<EntityContainer>().SingleOrDefault();
            var edmModel =
                container != null
                    ? EdmModel.CreateStoreModel(container, null, null, storeItemCollection.StoreSchemaVersion)
                    : new EdmModel(DataSpace.SSpace, storeItemCollection.StoreSchemaVersion);

            foreach (var entityType in storeItemCollection.GetItems<EntityType>())
            {
                edmModel.AddItem(entityType);
            }

            foreach (var associationType in storeItemCollection.GetItems<AssociationType>())
            {
                edmModel.AddItem(associationType);
            }

            return edmModel;
        }
    }
}
