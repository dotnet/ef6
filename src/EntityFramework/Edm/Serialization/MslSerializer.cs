namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Serialization.Xml.Internal.Msl;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Xml;

    internal class MslSerializer
    {
        /// <summary>
        ///     Serialize the <see cref = "DbModel" /> to the XmlWriter
        /// </summary>
        /// <param name = "databaseMapping"> The DbModel to serialize </param>
        /// <param name = "xmlWriter"> The XmlWriter to serialize to </param>
        public bool Serialize(DbDatabaseMapping databaseMapping, XmlWriter xmlWriter)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(xmlWriter != null);

            // TODO: add the validation for MSL

            var schemaWriter = new DbModelMslSchemaWriter(xmlWriter, databaseMapping.Model.Version);

            schemaWriter.WriteSchema(databaseMapping);

            return true;
        }
    }
}
