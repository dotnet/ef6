// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    

    internal static class ModelAssertions
    {
        internal static PropertyAssertions Assert<TStructuralType>(
            this DbDatabaseMapping databaseMapping, Expression<Func<TStructuralType, object>> propertyExpression)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems
                    .OfType<StructuralType>()
                    .Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && (Type)a.Value == typeof(TStructuralType)));

            var property
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>()
                    .Where(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && ((Type)a.Value).IsAssignableFrom(typeof(TStructuralType))))
                    .SelectMany(th => th.Members.OfType<EdmProperty>()).Distinct().Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrPropertyInfo"
                                 && (PropertyInfo)a.Value == GetPropertyInfo(propertyExpression)));

            var columns
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => !(structuralType is EntityType) || etm.EntityType == structuralType)
                    .SelectMany(etm => etm.TypeMappingFragments)
                    .SelectMany(tmf => tmf.PropertyMappings)
                    .Where(pm => pm.PropertyPath.Contains(property))
                    .Select(pm => pm.Column);

            return new PropertyAssertions(property, columns.First());
        }

        internal static TypeAssertions Assert<TStructuralType>(this DbDatabaseMapping databaseMapping)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var table
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.TypeMappingFragments)
                    .Select(tmf => tmf.Table)
                    .Distinct()
                    .Single();

            return new TypeAssertions(table);
        }

        internal static TypeAssertions Assert<TStructuralType>(this DbDatabaseMapping databaseMapping, string tableName)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>()
                    .Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && (Type)a.Value == typeof(TStructuralType)));

            var table
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.TypeMappingFragments).First(mf => mf.Table.DatabaseIdentifier == tableName)
                    .Table;

            return new TypeAssertions(table);
        }

        internal static TypeAssertions Assert(
            this DbDatabaseMapping databaseMapping, string tableName,
            string schemaName = null)
        {
            var schema
                = schemaName == null
                      ? databaseMapping.Database.Schemas.Single()
                      : databaseMapping.Database.Schemas.Single(s => s.Name == schemaName);

            var table = schema.Tables.Single(t => t.DatabaseIdentifier == tableName);

            return new TypeAssertions(table);
        }

        internal static MappingFragmentAssertions AssertMapping<TStructuralType>(
            this DbDatabaseMapping databaseMapping,
            string tableName)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragments
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.TypeMappingFragments)
                    .Where(mf => mf.Table.DatabaseIdentifier == tableName);

            var fragment = fragments.First();

            Xunit.Assert.True(fragments.All(f => f.Table == fragment.Table));

            return new MappingFragmentAssertions(fragment);
        }

        internal static MappingFragmentAssertions AssertMapping<TStructuralType>(
            this DbDatabaseMapping databaseMapping,
            string tableName, bool isTypeOfMapping)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragment
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType && isTypeOfMapping == etm.IsHierarchyMapping)
                    .SelectMany(etm => etm.TypeMappingFragments).Single(mf => mf.Table.DatabaseIdentifier == tableName);

            return new MappingFragmentAssertions(fragment);
        }

        internal static void AssertNoMapping<TStructuralType>(this DbDatabaseMapping databaseMapping)
        {
            var structuralType
                = databaseMapping.Model.Namespaces.Single().NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragments
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.TypeMappingFragments);

            Xunit.Assert.Equal(0, fragments.Count());
        }

        private static PropertyInfo GetPropertyInfo(LambdaExpression propertyExpression)
        {
            return (PropertyInfo)((MemberExpression)propertyExpression.Body.RemoveConvert()).Member;
        }

        internal class ColumnAssertions
        {
            private readonly DbTableColumnMetadata _column;

            public ColumnAssertions(DbTableColumnMetadata column)
            {
                _column = column;
            }

            public ColumnAssertions DbEqual(object expected, Func<DbTableColumnMetadata, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_column));

                return this;
            }

            public ColumnAssertions DbIsFalse(Func<DbTableColumnMetadata, bool?> column)
            {
                Xunit.Assert.Equal(false, column(_column));

                return this;
            }

            public ColumnAssertions DbFacetEqual(object expected, Func<DbPrimitiveTypeFacets, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_column.Facets));

                return this;
            }
        }

        internal class PropertyAssertions
        {
            private readonly EdmProperty _property;
            private readonly DbTableColumnMetadata _column;

            public PropertyAssertions(EdmProperty property, DbTableColumnMetadata column)
            {
                _property = property;
                _column = column;
            }

            public PropertyAssertions IsTrue(Func<TypeUsage, bool?> facet)
            {
                Xunit.Assert.Equal(true, facet(_property.TypeUsage));

                return this;
            }

            public PropertyAssertions IsTrue(Func<EdmProperty, bool?> facet)
            {
                Xunit.Assert.Equal(true, facet(_property));

                return this;
            }

            public PropertyAssertions IsFalse(Func<TypeUsage, bool?> facet)
            {
                Xunit.Assert.Equal(false, facet(_property.TypeUsage));

                return this;
            }

            public PropertyAssertions IsFalse(Func<EdmProperty, bool?> facet)
            {
                Xunit.Assert.Equal(false, facet(_property));

                return this;
            }

            public PropertyAssertions DbEqual(object expected, Func<DbTableColumnMetadata, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_column));

                return this;
            }

            public PropertyAssertions DbIsFalse(Func<DbTableColumnMetadata, bool?> column)
            {
                Xunit.Assert.Equal(false, column(_column));

                return this;
            }

            public PropertyAssertions FacetEqual(object expected, Func<TypeUsage, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_property.TypeUsage));

                return this;
            }

            public PropertyAssertions FacetEqual(object expected, Func<EdmProperty, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_property));

                return this;
            }

            public PropertyAssertions DbFacetEqual(object expected, Func<DbPrimitiveTypeFacets, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_column.Facets));

                return this;
            }

            public PropertyAssertions AnnotationEqual(object expected, string annotation)
            {
                Xunit.Assert.Equal(
                    expected, _property.Annotations
                        .Single(a => a.Name.Equals(annotation, StringComparison.Ordinal))
                        .Value);

                return this;
            }

            public PropertyAssertions AnnotationNull(string annotation)
            {
                Xunit.Assert.Null(
                    _property.Annotations.SingleOrDefault(a => a.Name.Equals(annotation, StringComparison.Ordinal)));

                return this;
            }
        }

        internal class TypeAssertions
        {
            private readonly DbTableMetadata _table;

            public TypeAssertions(DbTableMetadata table)
            {
                _table = table;
            }

            public TypeAssertions DbEqual<T>(T expected, Func<DbTableMetadata, T> attribute)
            {
                Xunit.Assert.Equal(expected, attribute(_table));

                return this;
            }

            public TypeAssertions HasColumns(params string[] columns)
            {
                Xunit.Assert.True(_table.Columns.Select(c => c.Name).SequenceEqual(columns));

                return this;
            }

            public TypeAssertions HasColumn(string column)
            {
                Xunit.Assert.True(_table.Columns.Any(c => c.Name == column));

                return this;
            }

            public ColumnAssertions Column(string column)
            {
                return new ColumnAssertions(_table.Columns.Single(c => c.Name == column));
            }

            public TypeAssertions HasForeignKeyColumn(string column)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyConstraints.Count(f => f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasForeignKey(IEnumerable<string> columns, string toTable)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyConstraints.Count(
                        f => f.PrincipalTable.DatabaseIdentifier == toTable &&
                             f.DependentColumns.Select(c => c.Name).SequenceEqual(columns)));

                return this;
            }

            public TypeAssertions HasForeignKeyColumn(string column, string toTable)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyConstraints.Count(
                        f => f.PrincipalTable.DatabaseIdentifier == toTable &&
                             f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasNoForeignKeyColumn(string column)
            {
                Xunit.Assert.Equal(
                    0,
                    _table.ForeignKeyConstraints.Count(f => f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasNoForeignKeyColumns()
            {
                Xunit.Assert.Equal(0, _table.ForeignKeyConstraints.Count());

                return this;
            }

            public ColumnAssertions ForeignKeyColumn(string column)
            {
                return
                    new ColumnAssertions(
                        _table.ForeignKeyConstraints.SelectMany(f => f.DependentColumns).Single(c => c.Name == column));
            }
        }

        internal class MappingFragmentAssertions
        {
            private readonly DbEntityTypeMappingFragment _fragment;

            public MappingFragmentAssertions(DbEntityTypeMappingFragment fragment)
            {
                _fragment = fragment;
            }

            public MappingFragmentAssertions HasColumnCondition(string column, object value)
            {
                var con =
                    _fragment.ColumnConditions.Single(
                        cc => String.Equals(cc.Column.Name, column, StringComparison.Ordinal));
                Xunit.Assert.True(Equals(con.Value, value) && con.IsNull == null);
                return this;
            }

            public MappingFragmentAssertions HasNullabilityColumnCondition(string column, bool isNull)
            {
                Xunit.Assert.True(
                    _fragment.ColumnConditions.Any(
                        cc =>
                        String.Equals(cc.Column.Name, column, StringComparison.Ordinal) && cc.Value == null &&
                        cc.IsNull == isNull));
                return this;
            }

            public MappingFragmentAssertions HasNoColumnConditions()
            {
                Xunit.Assert.True(_fragment.ColumnConditions.Count == 0);
                return this;
            }

            public MappingFragmentAssertions HasNoColumnCondition(string column)
            {
                Xunit.Assert.True(
                    !_fragment.ColumnConditions.Any(
                        cc => String.Equals(cc.Column.Name, column, StringComparison.Ordinal)));
                return this;
            }

            public MappingFragmentAssertions HasNoPropertyConditions()
            {
                return this;
            }
        }

        private static Expression RemoveConvert(this Expression expression)
        {
            while ((expression != null)
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }
}
