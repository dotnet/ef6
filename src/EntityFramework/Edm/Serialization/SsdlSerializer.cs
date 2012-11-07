// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Serialization.Xml.Internal.Csdl;
    using System.Diagnostics.Contracts;
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
        public virtual bool Serialize(
            EdmModel dbDatabase, string provider, string providerManifestToken, XmlWriter xmlWriter)
        {
            Contract.Requires(dbDatabase != null);
            Contract.Requires(xmlWriter != null);

            var visitor = new EdmSerializationVisitor(xmlWriter, dbDatabase.Version, serializeDefaultNullability: true);

            visitor.Visit(dbDatabase, provider, providerManifestToken);

            return true;
        }
    }
}
