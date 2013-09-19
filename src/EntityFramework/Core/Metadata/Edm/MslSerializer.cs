// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Xml;

    internal class MslSerializer
    {
        /// <summary>
        /// Serialize the <see cref="DbModel" /> to the XmlWriter
        /// </summary>
        /// <param name="databaseMapping"> The DbModel to serialize </param>
        /// <param name="xmlWriter"> The XmlWriter to serialize to </param>
        public virtual bool Serialize(DbDatabaseMapping databaseMapping, XmlWriter xmlWriter)
        {
            Check.NotNull(databaseMapping, "databaseMapping");
            Check.NotNull(xmlWriter, "xmlWriter");

            var schemaWriter = new MslXmlSchemaWriter(xmlWriter, databaseMapping.Model.SchemaVersion);

            schemaWriter.WriteSchema(databaseMapping);

            return true;
        }
    }
}
