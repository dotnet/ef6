// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    public class SsdlSerializer
    {
        /// <summary>
        ///     Serialize the <see cref="EdmModel" /> to the <see cref="XmlWriter" />
        /// </summary>
        /// <param name="dbDatabase"> The EdmModel to serialize </param>
        /// <param name="provider"> Provider information on the Schema element </param>
        /// <param name="providerManifestToken"> ProviderManifestToken information on the Schema element </param>
        /// <param name="xmlWriter"> The XmlWriter to serialize to </param>
        /// <returns> </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nullability")]
        public virtual bool Serialize(
            EdmModel dbDatabase, string provider, string providerManifestToken, XmlWriter xmlWriter, bool serializeDefaultNullability = true)
        {
            Check.NotNull(dbDatabase, "dbDatabase");
            Check.NotEmpty(provider, "provider");
            Check.NotEmpty(providerManifestToken, "providerManifestToken");
            Check.NotNull(xmlWriter, "xmlWriter");

            CreateVisitor(xmlWriter, dbDatabase, serializeDefaultNullability)
                .Visit(dbDatabase, provider, providerManifestToken);

            return true;
        }

        /// <summary>
        ///     Serialize the <see cref="EdmModel" /> to the <see cref="XmlWriter" />
        /// </summary>
        /// <param name="dbDatabase"> The EdmModel to serialize </param>
        /// <param name="namespaceName"> Namespace name on the Schema element </param>
        /// <param name="provider"> Provider information on the Schema element </param>
        /// <param name="providerManifestToken"> ProviderManifestToken information on the Schema element </param>
        /// <param name="xmlWriter"> The XmlWriter to serialize to </param>
        /// <returns> </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nullability")]
        public virtual bool Serialize(
            EdmModel dbDatabase, string namespaceName, string provider, string providerManifestToken, XmlWriter xmlWriter, bool serializeDefaultNullability = true)
        {
            Check.NotNull(dbDatabase, "dbDatabase");
            Check.NotEmpty(namespaceName, "namespaceName");
            Check.NotEmpty(provider, "provider");
            Check.NotEmpty(providerManifestToken, "providerManifestToken");
            Check.NotNull(xmlWriter, "xmlWriter");
            
            CreateVisitor(xmlWriter, dbDatabase, serializeDefaultNullability)
                .Visit(dbDatabase, namespaceName, provider, providerManifestToken);

            return true;
        }

        private static EdmSerializationVisitor CreateVisitor(XmlWriter xmlWriter, EdmModel dbDatabase, bool serializeDefaultNullability)
        {
            return new EdmSerializationVisitor(xmlWriter, dbDatabase.Version, serializeDefaultNullability);
        }
    }
}
