namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Ssdl
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Internal;
    using System.Xml;

    internal class DbModelSsdlSerializationVisitor : DbDatabaseVisitor
    {
        private readonly double _dbVersion;
        private readonly DbModelSsdlSchemaWriter _schemaWriter;
        private readonly XmlWriter _xmlWriter;

        private readonly Dictionary<DbTableMetadata, IEnumerable<DbForeignKeyConstraintMetadata>>
            _foreignKeyConstraintList =
                new Dictionary<DbTableMetadata, IEnumerable<DbForeignKeyConstraintMetadata>>();

        internal DbModelSsdlSerializationVisitor(XmlWriter xmlWriter, double dbVersion)
        {
            _dbVersion = dbVersion;
            _xmlWriter = xmlWriter;
            _schemaWriter = new DbModelSsdlSchemaWriter(xmlWriter, dbVersion);
        }

        internal void Visit(DbDatabaseMetadata dbDatabase, string provider, string providerManifestToken)
        {
            var namespaceName = dbDatabase.Name;

            _schemaWriter.WriteSchemaElementHeader(namespaceName, provider, providerManifestToken);

            foreach (var schema in dbDatabase.Schemas)
            {
                VisitDbSchemaMetadata(schema);
            }

            // Serialize EntityContainer
            var containerVisitor = new DbModelEntityContainerSerializationVisitor(_xmlWriter, _dbVersion);

            containerVisitor.Visit(dbDatabase);

            _schemaWriter.WriteEndElement();
        }

        protected override void VisitDbSchemaMetadata(DbSchemaMetadata item)
        {
            // this will write out the types and functions
            base.VisitDbSchemaMetadata(item);

            // this will write out the associations
            VisitForeignKeyConstraints(_foreignKeyConstraintList);

            // clear the constraints for this schema so we can use the collection again
            _foreignKeyConstraintList.Clear();
        }

        protected virtual void VisitForeignKeyConstraints(
            Dictionary<DbTableMetadata, IEnumerable<DbForeignKeyConstraintMetadata>> foreignKeyConstraints)
        {
            foreach (var foreignKeyConstraint in foreignKeyConstraints)
            {
                foreach (var tableFKConstraint in foreignKeyConstraint.Value)
                {
                    _schemaWriter.WriteForeignKeyConstraintElement(foreignKeyConstraint.Key, tableFKConstraint);
                }
            }
        }

        protected override void VisitDbTableMetadata(DbTableMetadata item)
        {
            _schemaWriter.WriteEntityTypeElementHeader(item);
            base.VisitDbTableMetadata(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitKeyColumns(DbTableMetadata item, IEnumerable<DbTableColumnMetadata> keyColumns)
        {
            _schemaWriter.WriteDelaredKeyPropertiesElementHeader();
            foreach (var keyColumn in keyColumns)
            {
                _schemaWriter.WriteDelaredKeyPropertyRefElement(keyColumn);
            }
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitDbTableColumnMetadata(DbTableColumnMetadata item)
        {
            _schemaWriter.WritePropertyElementHeader(item);
            base.VisitDbTableColumnMetadata(item);
            _schemaWriter.WriteEndElement();
        }

        protected override void VisitForeignKeyConstraints(
            DbTableMetadata item, IEnumerable<DbForeignKeyConstraintMetadata> foreignKeyConstraints)
        {
            _foreignKeyConstraintList.Add(item, foreignKeyConstraints);
        }

        private class DbModelEntityContainerSerializationVisitor : DbModelSsdlSerializationVisitor
        {
            private readonly DbModelEntityContainerSsdlSchemaWriter _containerSchemaWriter;
            private DbSchemaMetadata currentSchema;

            internal DbModelEntityContainerSerializationVisitor(XmlWriter xmlWriter, double dbVersion)
                : base(xmlWriter, dbVersion)
            {
                _containerSchemaWriter = new DbModelEntityContainerSsdlSchemaWriter(xmlWriter);
            }

            internal void Visit(DbDatabaseMetadata dbDatabase)
            {
                _containerSchemaWriter.WriteEntityContainerElementHeader(dbDatabase.Name);

                foreach (var dbSchema in dbDatabase.Schemas)
                {
                    currentSchema = dbSchema;
                    base.VisitDbSchemaMetadata(dbSchema);
                    currentSchema = null;
                }

                _containerSchemaWriter.WriteEndElement();
            }

            protected override void VisitDbTableMetadata(DbTableMetadata item)
            {
                _containerSchemaWriter.WriteEntitySetElementHeader(currentSchema, item);
                VisitForeignKeyConstraints(item, item.ForeignKeyConstraints);
                _schemaWriter.WriteEndElement();
            }

            protected override void VisitForeignKeyConstraints(
                Dictionary<DbTableMetadata, IEnumerable<DbForeignKeyConstraintMetadata>> foreignKeyConstraints)
            {
                foreach (var foreignKeyConstraint in foreignKeyConstraints)
                {
                    foreach (var foreignKeyConstraintInTable in foreignKeyConstraint.Value)
                    {
                        _containerSchemaWriter.WriteAssociationSetElementHeader(foreignKeyConstraintInTable);

                        var roleNames = DbModelSsdlHelper.GetRoleNamePair(
                            foreignKeyConstraintInTable.PrincipalTable, foreignKeyConstraint.Key);
                        _containerSchemaWriter.WriteAssociationSetEndElement(
                            foreignKeyConstraintInTable.PrincipalTable, roleNames[0]);
                        _containerSchemaWriter.WriteAssociationSetEndElement(foreignKeyConstraint.Key, roleNames[1]);

                        _containerSchemaWriter.WriteEndElement();
                    }
                }
            }
        }
    }
}
