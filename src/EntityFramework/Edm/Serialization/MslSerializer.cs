// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Serialization.Xml.Internal.Msl;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Xml;

    public class MslSerializer
    {
        /// <summary>
        ///     Serialize the <see cref="DbModel" /> to the XmlWriter
        /// </summary>
        /// <param name="databaseMapping"> The DbModel to serialize </param>
        /// <param name="xmlWriter"> The XmlWriter to serialize to </param>
        public virtual bool Serialize(DbDatabaseMapping databaseMapping, XmlWriter xmlWriter)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(xmlWriter != null);

            var schemaWriter = new DbModelMslSchemaWriter(xmlWriter, databaseMapping.Model.Version);

            schemaWriter.WriteSchema(databaseMapping);

            return true;
        }
    }
}
