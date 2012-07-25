// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public partial class ConventionsConfiguration
    {
        private class DatabaseConventionDispatcher : DbDatabaseVisitor
        {
            private readonly IConvention _convention;
            private readonly DbDatabaseMetadata _database;

            public DatabaseConventionDispatcher(IConvention convention, DbDatabaseMetadata database)
            {
                Contract.Requires(convention != null);
                Contract.Requires(database != null);

                _convention = convention;
                _database = database;
            }

            public void Dispatch()
            {
                VisitDbDatabaseMetadata(_database);
            }

            private void Dispatch<TDbDataModelItem>(TDbDataModelItem item)
                where TDbDataModelItem : DbDataModelItem
            {
                var convention
                    = _convention as IDbConvention<TDbDataModelItem>;

                if (convention != null)
                {
                    convention.Apply(item, _database);
                }
            }

            protected override void VisitDbDatabaseMetadata(DbDatabaseMetadata item)
            {
                var convention = _convention as IDbConvention;

                if (convention != null)
                {
                    convention.Apply(item);
                }

                base.VisitDbDatabaseMetadata(item);
            }

            protected override void VisitDbSchemaMetadata(DbSchemaMetadata item)
            {
                Dispatch(item);

                base.VisitDbSchemaMetadata(item);
            }

            protected override void VisitDbAliasedMetadataItem(DbAliasedMetadataItem item)
            {
                Dispatch(item);

                base.VisitDbAliasedMetadataItem(item);
            }

            protected override void VisitDbNamedMetadataItem(DbNamedMetadataItem item)
            {
                Dispatch(item);

                base.VisitDbNamedMetadataItem(item);
            }

            protected override void VisitDbMetadataItem(DbMetadataItem item)
            {
                Dispatch(item);

                base.VisitDbMetadataItem(item);
            }

            protected override void VisitDbDataModelItem(DbDataModelItem item)
            {
                Dispatch(item);

                base.VisitDbDataModelItem(item);
            }

            protected override void VisitDbTableMetadata(DbTableMetadata item)
            {
                Dispatch(item);

                base.VisitDbTableMetadata(item);
            }

            protected override void VisitDbTypeMetadata(DbTypeMetadata item)
            {
                Dispatch(item);

                base.VisitDbTypeMetadata(item);
            }

            protected override void VisitDbTableColumnMetadata(DbTableColumnMetadata item)
            {
                Dispatch(item);

                base.VisitDbTableColumnMetadata(item);
            }

            protected override void VisitDbForeignKeyConstraintMetadata(DbForeignKeyConstraintMetadata item)
            {
                Dispatch(item);
            }

            protected override void VisitDbConstraintMetadata(DbConstraintMetadata item)
            {
                Dispatch(item);

                base.VisitDbConstraintMetadata(item);
            }
        }
    }
}
