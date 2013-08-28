// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ModelAssertions
    {
        internal static PropertyAssertions Assert<TStructuralType>(
            this DbDatabaseMapping databaseMapping, Expression<Func<TStructuralType, object>> propertyExpression)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems
                    .OfType<StructuralType>()
                    .Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && (Type)a.Value == typeof(TStructuralType)));

            var property
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>()
                    .Where(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && ((Type)a.Value).IsAssignableFrom(typeof(TStructuralType))))
                    .SelectMany(th => th.Members.OfType<EdmProperty>()).Distinct().Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrPropertyInfo"
                                 && IsSameAs((PropertyInfo)a.Value, GetPropertyInfo(propertyExpression))));

            var columns
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => !(structuralType is EntityType) || etm.EntityType == structuralType)
                    .SelectMany(etm => etm.MappingFragments)
                    .SelectMany(tmf => tmf.ColumnMappings)
                    .Where(pm => pm.PropertyPath.Contains(property))
                    .Select(pm => pm.ColumnProperty);

            return new PropertyAssertions(property, columns);
        }
        
        internal static TypeAssertions Assert<TStructuralType>(this DbDatabaseMapping databaseMapping)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var table
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.MappingFragments)
                    .Select(tmf => tmf.Table)
                    .Distinct()
                    .Single();

            return new TypeAssertions(table, databaseMapping.Database.GetEntitySet(table), databaseMapping.Database);
        }

        internal static TypeAssertions Assert<TStructuralType>(this DbDatabaseMapping databaseMapping, string tableName)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>()
                    .Single(
                        i => i.Annotations.Any(
                            a => a.Name == "ClrType"
                                 && (Type)a.Value == typeof(TStructuralType)));

            var table
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.MappingFragments).First(
                        mf => databaseMapping.Database.GetEntitySet(mf.Table).Table == tableName)
                    .Table;

            return new TypeAssertions(table, databaseMapping.Database.GetEntitySet(table), databaseMapping.Database);
        }

        internal static TypeAssertions Assert(
            this DbDatabaseMapping databaseMapping, string tableName, string schemaName = null)
        {
            var entitySet
                = databaseMapping.Database
                    .GetEntitySets()
                    .Single(
                        es => es.Table == tableName
                              && ((schemaName == null) || es.Schema == schemaName));

            return new TypeAssertions(entitySet.ElementType, entitySet, databaseMapping.Database);
        }

        internal static MappingFragmentAssertions AssertMapping<TStructuralType>(
            this DbDatabaseMapping databaseMapping,
            string tableName)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragments
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.MappingFragments)
                    .Where(mf => databaseMapping.Database.GetEntitySet(mf.Table).Table == tableName)
                    .ToList();

            var fragment = fragments.First();

            Xunit.Assert.True(fragments.All(f => f.Table == fragment.Table));

            return new MappingFragmentAssertions(fragment);
        }

        internal static MappingFragmentAssertions AssertMapping<TStructuralType>(
            this DbDatabaseMapping databaseMapping,
            string tableName, bool isTypeOfMapping)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragment
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType && isTypeOfMapping == etm.IsHierarchyMapping)
                    .SelectMany(etm => etm.MappingFragments)
                    .Single(mf => databaseMapping.Database.GetEntitySet(mf.Table).Table == tableName);

            return new MappingFragmentAssertions(fragment);
        }

        internal static void AssertFunctionMapping<TStructuralType>(this DbDatabaseMapping databaseMapping)
        {
            var entityType
                = databaseMapping.Model.EntityTypes.Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings
                .Where(esm => esm.EntitySet.ElementType == entityType.GetRootType())
                .Select(esm => esm.ModificationFunctionMappings)
                .Single();
        }

        internal static CompositeParameterAssertions AssertFunctionMapping<TStructuralType>(
            this DbDatabaseMapping databaseMapping, Expression<Func<TStructuralType, object>> propertyExpression)
        {
            var property
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>()
                                 .Where(
                                     i => i.Annotations.Any(
                                         a => a.Name == "ClrType"
                                              && ((Type)a.Value).IsAssignableFrom(typeof(TStructuralType))))
                                 .SelectMany(th => th.Members.OfType<EdmProperty>()).Distinct().Single(
                                     i => i.Annotations.Any(
                                         a => a.Name == "ClrPropertyInfo"
                                              && IsSameAs((PropertyInfo)a.Value, GetPropertyInfo(propertyExpression))));

            var parameterBindings
                = databaseMapping
                    .EntityContainerMappings
                    .Single()
                    .EntitySetMappings
                    .SelectMany(esm => esm.ModificationFunctionMappings)
                    .SelectMany(
                        mfm => mfm.InsertFunctionMapping.ParameterBindings
                                  .Concat(mfm.UpdateFunctionMapping.ParameterBindings)
                                  .Concat(mfm.DeleteFunctionMapping.ParameterBindings))
                    .Where(p => p.MemberPath.Members.Contains(property))
                    .ToList();

            return new CompositeParameterAssertions(parameterBindings);
        }

        internal class CompositeParameterAssertions
        {
            private readonly IList<ModificationFunctionParameterBinding> _parameterBindings;

            public CompositeParameterAssertions(IList<ModificationFunctionParameterBinding> parameterBindings)
            {
                Xunit.Assert.NotEmpty(parameterBindings);

                _parameterBindings = parameterBindings;
            }

            public CompositeParameterAssertions ParameterEqual(object expected, Func<FunctionParameter, object> facet)
            {
                foreach (var parameterBinding in _parameterBindings)
                {
                    Xunit.Assert.Equal(expected, facet(parameterBinding.Parameter));
                }
                
                return this;
            }
        }

        internal static void AssertNoMapping<TStructuralType>(this DbDatabaseMapping databaseMapping)
        {
            var structuralType
                = databaseMapping.Model.NamespaceItems.OfType<StructuralType>().Single(
                    i => i.Annotations.Any(
                        a => a.Name == "ClrType"
                             && (Type)a.Value == typeof(TStructuralType)));

            var fragments
                = databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    .SelectMany(esm => esm.EntityTypeMappings)
                    .Where(etm => etm.EntityType == structuralType)
                    .SelectMany(etm => etm.MappingFragments);

            Xunit.Assert.Equal(0, fragments.Count());
        }

        private static PropertyInfo GetPropertyInfo(LambdaExpression propertyExpression)
        {
            return (PropertyInfo)((MemberExpression)propertyExpression.Body.RemoveConvert()).Member;
        }

        internal class ColumnAssertions
        {
            private readonly EdmProperty _column;

            public ColumnAssertions(EdmProperty column)
            {
                _column = column;
            }

            public ColumnAssertions DbEqual(object expected, Func<EdmProperty, object> facet)
            {
                Xunit.Assert.Equal(expected, facet(_column));

                return this;
            }

            public ColumnAssertions DbIsFalse(Func<EdmProperty, bool?> column)
            {
                Xunit.Assert.Equal(false, column(_column));

                return this;
            }
        }

        internal class PropertyAssertions
        {
            private readonly EdmProperty _property;
            private readonly IEnumerable<EdmProperty> _columns;

            public PropertyAssertions(EdmProperty property, IEnumerable<EdmProperty> columns)
            {
                _property = property;
                _columns = columns;
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

            public PropertyAssertions DbEqual(object expected, Func<EdmProperty, object> facet)
            {
                foreach (var column in _columns)
                {
                    Xunit.Assert.Equal(expected, facet(column));
                }
                
                return this;
            }

            public PropertyAssertions DbIsFalse(Func<EdmProperty, bool?> column)
            {
                foreach (var c in _columns)
                {
                    Xunit.Assert.Equal(false, column(c));
                }

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

            public PropertyAssertions MetadataPropertyEqual(object expected, string annotation)
            {
                Xunit.Assert.Equal(
                    expected, _property.MetadataProperties
                                       .Single(
                                           a => a.Name.Equals(XmlConstants.AnnotationNamespace + ":" + annotation, StringComparison.Ordinal))
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
            private readonly EntityType _table;
            private readonly EntitySet _entitySet;
            private readonly EdmModel _database;

            public TypeAssertions(EntityType table, EntitySet entitySet, EdmModel database)
            {
                _table = table;
                _entitySet = entitySet;
                _database = database;
            }

            public TypeAssertions DbEqual<T>(T expected, Func<EntityType, T> attribute)
            {
                Xunit.Assert.Equal(expected, attribute(_table));

                return this;
            }

            public TypeAssertions DbEqual<T>(T expected, Func<EntitySet, T> attribute)
            {
                Xunit.Assert.Equal(expected, attribute(_entitySet));

                return this;
            }

            public TypeAssertions HasColumns(params string[] columns)
            {
                Xunit.Assert.True(_table.Properties.Select(c => c.Name).SequenceEqual(columns));

                return this;
            }

            public TypeAssertions HasColumn(string column)
            {
                Xunit.Assert.True(_table.Properties.Any(c => c.Name == column));

                return this;
            }

            public ColumnAssertions Column(string column)
            {
                return new ColumnAssertions(_table.Properties.Single(c => c.Name == column));
            }

            public TypeAssertions HasForeignKeyColumn(string column)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyBuilders.Count(f => f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasForeignKey(IEnumerable<string> columns, string toTable)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyBuilders.Count(
                        f => _database.GetEntitySet(f.PrincipalTable).Table == toTable &&
                             f.DependentColumns.Select(c => c.Name).SequenceEqual(columns)));

                return this;
            }

            public TypeAssertions HasForeignKeyColumn(string column, string toTable)
            {
                Xunit.Assert.Equal(
                    1,
                    _table.ForeignKeyBuilders.Count(
                        f => _database.GetEntitySet(f.PrincipalTable).Table == toTable &&
                             f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasNoForeignKeyColumn(string column)
            {
                Xunit.Assert.Equal(
                    0,
                    _table.ForeignKeyBuilders.Count(f => f.DependentColumns.Any(d => d.Name == column)));

                return this;
            }

            public TypeAssertions HasNoForeignKeyColumns()
            {
                Xunit.Assert.Equal(0, _table.ForeignKeyBuilders.Count());

                return this;
            }

            public ColumnAssertions ForeignKeyColumn(string column)
            {
                return
                    new ColumnAssertions(
                        _table.ForeignKeyBuilders.SelectMany(f => f.DependentColumns).Single(c => c.Name == column));
            }
        }

        internal class MappingFragmentAssertions
        {
            private readonly MappingFragment _fragment;

            public MappingFragmentAssertions(MappingFragment fragment)
            {
                _fragment = fragment;
            }

            public MappingFragmentAssertions HasColumnCondition(string column, object value)
            {
                var con =
                    _fragment.ColumnConditions.Single(
                        cc => String.Equals(cc.ColumnProperty.Name, column, StringComparison.Ordinal));

                Xunit.Assert.True(Equals(con.Value, value) && con.IsNull == null);

                return this;
            }

            public MappingFragmentAssertions HasNullabilityColumnCondition(string column, bool isNull)
            {
                Xunit.Assert.True(
                    _fragment.ColumnConditions.Any(
                        cc =>
                        String.Equals(cc.ColumnProperty.Name, column, StringComparison.Ordinal) && cc.Value == null &&
                        cc.IsNull == isNull));

                return this;
            }

            public MappingFragmentAssertions HasNoColumnConditions()
            {
                Xunit.Assert.True(!_fragment.ColumnConditions.Any());

                return this;
            }

            public MappingFragmentAssertions HasNoColumnCondition(string column)
            {
                Xunit.Assert.True(
                    !_fragment.ColumnConditions.Any(
                        cc => String.Equals(cc.ColumnProperty.Name, column, StringComparison.Ordinal)));

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

        private static bool IsSameAs(PropertyInfo propertyInfo, PropertyInfo otherPropertyInfo)
        {
            return (propertyInfo == otherPropertyInfo) ||
                   (propertyInfo.Name == otherPropertyInfo.Name
                    && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType
                        || propertyInfo.DeclaringType.IsSubclassOf(otherPropertyInfo.DeclaringType)
                        || otherPropertyInfo.DeclaringType.IsSubclassOf(propertyInfo.DeclaringType)
                        || propertyInfo.DeclaringType.GetInterfaces().Contains(otherPropertyInfo.DeclaringType)
                        || otherPropertyInfo.DeclaringType.GetInterfaces().Contains(propertyInfo.DeclaringType)));
        }
    }
}
