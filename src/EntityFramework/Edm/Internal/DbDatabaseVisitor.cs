namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Db;

    internal class DbDatabaseVisitor : DataModelItemVisitor
    {
        protected DbDatabaseVisitor()
        {
        }

        protected virtual void VisitDbDatabaseMetadata(DbDatabaseMetadata item)
        {
            VisitDbAliasedMetadataItem(item);
            if (item != null)
            {
                if (item.HasSchemas)
                {
                    VisitCollection(item.Schemas, VisitDbSchemaMetadata);
                }
            }
        }

        protected virtual void VisitDbSchemaMetadata(DbSchemaMetadata item)
        {
            VisitDbAliasedMetadataItem(item);
            if (item != null)
            {
                if (item.HasTables)
                {
                    VisitCollection(item.Tables, VisitDbTableMetadata);
                }

#if IncludeUnusedEdmCode
                if (item.HasFunctions)
                {
                    VisitCollection(item.Functions, this.VisitDbFunctionMetadata);
                }
#endif
            }
        }

        protected virtual void VisitDbAliasedMetadataItem(DbAliasedMetadataItem item)
        {
            VisitDbNamedMetadataItem(item);
        }

        protected virtual void VisitDbNamedMetadataItem(DbNamedMetadataItem item)
        {
            VisitDbMetadataItem(item);
        }

        protected virtual void VisitDbMetadataItem(DbMetadataItem item)
        {
            VisitDbDataModelItem(item);
            if (item != null)
            {
                if (item.HasAnnotations)
                {
                    VisitAnnotations(item, item.Annotations);
                }
            }
        }

        protected virtual void VisitDbDataModelItem(DbDataModelItem item)
        {
            VisitDataModelItem(item);
        }

        protected virtual void VisitDbTableMetadata(DbTableMetadata item)
        {
            VisitDbSchemaMetadataItem(item);
            if (item != null)
            {
                if (item.HasColumns)
                {
                    VisitKeyColumns(item, item.KeyColumns);
                    VisitColumns(item, item.Columns);
                }
                if (item.HasForeignKeyConstraints)
                {
                    VisitForeignKeyConstraints(item, item.ForeignKeyConstraints);
                }
            }
        }

        protected virtual void VisitKeyColumns(DbTableMetadata item, IEnumerable<DbTableColumnMetadata> keyColumns)
        {
            VisitCollection(keyColumns, VisitDbTableColumnMetadata);
        }

        protected virtual void VisitColumns(DbTableMetadata item, IEnumerable<DbTableColumnMetadata> columns)
        {
            VisitCollection(columns, VisitDbTableColumnMetadata);
        }

        protected virtual void VisitForeignKeyConstraints(
            DbTableMetadata item, IEnumerable<DbForeignKeyConstraintMetadata> foreignKeyConstraints)
        {
            VisitCollection(foreignKeyConstraints, VisitDbForeignKeyConstraintMetadata);
        }

        private void VisitDbSchemaMetadataItem(DbSchemaMetadataItem item)
        {
            VisitDbAliasedMetadataItem(item);
        }

#if IncludeUnusedEdmCode
        protected virtual void VisitDbFunctionMetadata(DbFunctionMetadata item)
        {
            this.VisitDbSchemaMetadataItem(item);
            if (item != null)
            {
                this.VisitDbFunctionTypeMetadata(item.ReturnType);
                if (item.HasParameters)
                {
                    VisitCollection(item.Parameters, this.VisitDbFunctionParameterMetadata);
                }
            }
        }
#endif

        protected virtual void VisitDbTypeMetadata(DbTypeMetadata item)
        {
            VisitDbMetadataItem(item);
            if (item != null)
            {
                if (item.Facets != null)
                {
                    VisitDbPrimitiveTypeFacets(item.Facets);
                }
#if IncludeUnusedEdmCode
                else if (item.HasRowColumns)
                {
                    VisitCollection(item.RowColumns, this.VisitDbRowColumnMetadata);
                }
#endif
            }
        }

#if IncludeUnusedEdmCode
        protected void VisitDbRowColumnMetadata(DbRowColumnMetadata item)
        {
            this.VisitDbColumnMetadata(item);
        }
#endif

        protected void VisitDbPrimitiveTypeFacets(DbPrimitiveTypeFacets item)
        {
            VisitDbDataModelItem(item);
        }

#if IncludeUnusedEdmCode
        protected virtual void VisitDbFunctionParameterMetadata(DbFunctionParameterMetadata item)
        {
            this.VisitDbNamedMetadataItem(item);
            if (item != null)
            {
                this.VisitDbFunctionTypeMetadata(item.ParameterType);
            }
        }

        protected virtual void VisitDbFunctionTypeMetadata(DbFunctionTypeMetadata item)
        {
            this.VisitDbTypeMetadata(item);
        }
#endif

        protected virtual void VisitDbTableColumnMetadata(DbTableColumnMetadata item)
        {
            VisitDbColumnMetadata(item);
        }

        private void VisitDbColumnMetadata(DbColumnMetadata item)
        {
            VisitDbNamedMetadataItem(item);
        }

        protected virtual void VisitDbForeignKeyConstraintMetadata(DbForeignKeyConstraintMetadata item)
        {
            VisitDbConstraintMetadata(item);
            if (item != null)
            {
                if (item.HasDependentColumns)
                {
                    VisitCollection(item.DependentColumns, VisitDbTableColumnMetadata);
                }
                VisitDbTableMetadata(item.PrincipalTable);
            }
        }

        protected virtual void VisitDbConstraintMetadata(DbConstraintMetadata item)
        {
            VisitDbNamedMetadataItem(item);
        }
    }
}