// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EdmModelDiffer
    {
        private static readonly PrimitiveTypeKind[] _validIdentityTypes
            = new[]
                  {
                      PrimitiveTypeKind.Byte,
                      PrimitiveTypeKind.Decimal,
                      PrimitiveTypeKind.Guid,
                      PrimitiveTypeKind.Int16,
                      PrimitiveTypeKind.Int32,
                      PrimitiveTypeKind.Int64
                  };

        private class ModelMetadata
        {
            public XDocument Model { get; set; }
            public StoreItemCollection StoreItemCollection { get; set; }
            public DbProviderManifest ProviderManifest { get; set; }
            public DbProviderInfo ProviderInfo { get; set; }
        }

        private ModelMetadata _source;
        private ModelMetadata _target;

        private bool _consistentProviders;

        public IEnumerable<MigrationOperation> Diff(XDocument sourceModel, XDocument targetModel, bool? includeSystemOperations = null)
        {
            DbProviderInfo providerInfo;

            _source
                = new ModelMetadata
                      {
                          Model = sourceModel,
                          StoreItemCollection = sourceModel.GetStoreItemCollection(out providerInfo),
                          ProviderManifest = GetProviderManifest(providerInfo),
                          ProviderInfo = providerInfo
                      };

            _target
                = new ModelMetadata
                      {
                          Model = targetModel,
                          StoreItemCollection = targetModel.GetStoreItemCollection(out providerInfo),
                          ProviderManifest = GetProviderManifest(providerInfo),
                          ProviderInfo = providerInfo
                      };

            _consistentProviders
                = _source.ProviderInfo.ProviderInvariantName.EqualsIgnoreCase(
                    _target.ProviderInfo.ProviderInvariantName)
                  && _source.ProviderInfo.ProviderManifestToken.EqualsIgnoreCase(
                      _target.ProviderInfo.ProviderManifestToken);

            var renamedColumns = FindRenamedColumns().ToList();
            var addedColumns = FindAddedColumns(renamedColumns).ToList();
            var alteredColumns = FindChangedColumns().ToList();
            var removedColumns = FindRemovedColumns(renamedColumns).ToList();
            var renamedTables = FindRenamedTables().ToList();
            var movedTables = FindMovedTables().ToList();
            var addedTables = FindAddedTables(renamedTables).ToList();

            var columnNormalizedSourceModel = BuildColumnNormalizedSourceModel(renamedColumns);

            var addedForeignKeys = FindAddedForeignKeys(columnNormalizedSourceModel).ToList();
            var removedTables = FindRemovedTables(renamedTables).ToList();
            var removedForeignKeys = FindRemovedForeignKeys(columnNormalizedSourceModel).ToList();
            var changedPrimaryKeys = FindChangedPrimaryKeys(columnNormalizedSourceModel).ToList();

            if (includeSystemOperations == null)
            {
                includeSystemOperations
                    = sourceModel.HasSystemOperations() && targetModel.HasSystemOperations();
            }

            return renamedTables
                .Concat<MigrationOperation>(movedTables)
                .Concat(removedForeignKeys)
                .Concat(removedForeignKeys.Select(fko => fko.CreateDropIndexOperation()))
                .Concat(renamedColumns)
                .Concat(addedTables)
                .Concat(addedColumns)
                .Concat(alteredColumns)
                .Concat(changedPrimaryKeys)
                .Concat(addedForeignKeys.Select(fko => fko.CreateCreateIndexOperation()))
                .Concat(addedForeignKeys)
                .Concat(removedColumns)
                .Concat(removedTables)
                .Where(o => (includeSystemOperations == true) || !o.IsSystem)
                .ToList();
        }

        private XDocument BuildColumnNormalizedSourceModel(IEnumerable<RenameColumnOperation> renamedColumns)
        {
            Contract.Requires(renamedColumns != null);

            var columnNormalizedSourceModel = new XDocument(_source.Model); // clone

            renamedColumns.Each(
                rc =>
                    {
                        var entitySet
                            = (from es in _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                               where
                                   GetQualifiedTableName(es.TableAttribute(), es.SchemaAttribute()).EqualsIgnoreCase(
                                       rc.Table)
                               select es.NameAttribute()).Single();

                        var principalDependents
                            = from pd in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.PrincipalNames)
                                  .Concat(columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.DependentNames))
                              where pd.RoleAttribute().EqualsIgnoreCase(entitySet)
                              from pr in pd.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                              where pr.NameAttribute().EqualsIgnoreCase(rc.Name)
                              select pr;

                        principalDependents.Each(pd => pd.SetAttributeValue("Name", rc.NewName));

                        var keyProperties
                            = from et in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                              where et.NameAttribute().EqualsIgnoreCase(entitySet)
                              from pr in
                                  et.Descendants(EdmXNames.Ssdl.KeyNames).Descendants(EdmXNames.Ssdl.PropertyRefNames)
                              where pr.NameAttribute().EqualsIgnoreCase(rc.Name)
                              select pr;

                        keyProperties.Each(pr => pr.SetAttributeValue("Name", rc.NewName));
                    });

            return columnNormalizedSourceModel;
        }

        private IEnumerable<RenameTableOperation> FindRenamedTables()
        {
            return from es1 in _source.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   from es2 in _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   where es1.NameAttribute().EqualsIgnoreCase(es2.NameAttribute())
                         && !es1.TableAttribute().EqualsIgnoreCase(es2.TableAttribute())
                   select
                       new RenameTableOperation(
                       GetQualifiedTableName(es1.TableAttribute(), es1.SchemaAttribute()), es2.TableAttribute());
        }

        private IEnumerable<CreateTableOperation> FindAddedTables(IEnumerable<RenameTableOperation> renamedTables)
        {
            return _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                .Except(
                    _source.Model.Descendants(EdmXNames.Ssdl.EntitySetNames),
                    (es1, es2) => es1.NameAttribute().EqualsIgnoreCase(es2.NameAttribute()))
                .Where(es => !renamedTables.Any(rt => rt.NewName.EqualsIgnoreCase(es.TableAttribute())))
                .Select(
                    es =>
                    BuildCreateTableOperation(
                        es.NameAttribute(), es.TableAttribute(), es.SchemaAttribute(), es.IsSystemAttribute(), _target));
        }

        private IEnumerable<MoveTableOperation> FindMovedTables()
        {
            return from es1 in _source.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   from es2 in _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   let isSystem = es2.IsSystemAttribute().EqualsIgnoreCase("true")
                   where es1.NameAttribute().EqualsIgnoreCase(es2.NameAttribute())
                         && !es1.SchemaAttribute().EqualsIgnoreCase(es2.SchemaAttribute())
                   select
                       new MoveTableOperation(
                       GetQualifiedTableName(es2.TableAttribute(), es1.SchemaAttribute()),
                       es2.SchemaAttribute())
                           {
                               IsSystem = isSystem,
                               CreateTableOperation
                                   = isSystem
                                         ? BuildCreateTableOperation(
                                             es2.NameAttribute(),
                                             es2.TableAttribute(),
                                             es2.SchemaAttribute(),
                                             es2.IsSystemAttribute(),
                                             _target)
                                         : null
                           };
        }

        private IEnumerable<DropTableOperation> FindRemovedTables(IEnumerable<RenameTableOperation> renamedTables)
        {
            return _source.Model.Descendants(EdmXNames.Ssdl.EntitySetNames).Except(
                _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames),
                (es1, es2) => es1.NameAttribute().EqualsIgnoreCase(es2.NameAttribute()))
                .Where(es => !renamedTables.Any(rt => rt.Name.EqualsIgnoreCase(es.TableAttribute())))
                .Select(
                    es => new DropTableOperation(
                              GetQualifiedTableName(es.TableAttribute(), es.SchemaAttribute()),
                              BuildCreateTableOperation(
                                  es.NameAttribute(),
                                  es.TableAttribute(),
                                  es.SchemaAttribute(),
                                  es.IsSystemAttribute(),
                                  _source))
                              {
                                  IsSystem = es.IsSystemAttribute().EqualsIgnoreCase("true")
                              });
        }

        private IEnumerable<DropColumnOperation> FindRemovedColumns(IEnumerable<RenameColumnOperation> renamedColumns)
        {
            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from c in t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                       .Except(
                           t2.Descendants(EdmXNames.Ssdl.PropertyNames),
                           (c1, c2) => c1.NameAttribute().EqualsIgnoreCase(c2.NameAttribute()))
                   where !renamedColumns.Any(rc => rc.Name.EqualsIgnoreCase(c.NameAttribute()))
                   select new DropColumnOperation(
                       t,
                       c.NameAttribute(),
                       new AddColumnOperation(t, BuildColumnModel(c, t1.NameAttribute(), _source)));
        }

        private IEnumerable<RenameColumnOperation> FindRenamedColumns()
        {
            return FindRenamedMappedColumns()
                .Concat(FindRenamedIndependentAssociationColumns())
                .Concat(FindRenamedDiscriminatorColumns())
                .Distinct(
                    new DynamicEqualityComparer<RenameColumnOperation>(
                        (c1, c2) => c1.Table.EqualsIgnoreCase(c2.Table)
                                    && c1.Name.EqualsIgnoreCase(c2.Name)
                                    && c1.NewName.EqualsIgnoreCase(c2.NewName)));
        }

        private IEnumerable<RenameColumnOperation> FindRenamedMappedColumns()
        {
            return from etm1 in _source.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   from etm2 in _target.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   where etm1.TypeNameAttribute().EqualsIgnoreCase(etm2.TypeNameAttribute())
                   from mf1 in etm1.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   from mf2 in etm2.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   where mf1.StoreEntitySetAttribute().EqualsIgnoreCase(mf2.StoreEntitySetAttribute())
                   let t = GetQualifiedTableName(_target.Model, mf1.StoreEntitySetAttribute())
                   from cr in FindRenamedMappedColumns(mf1, mf2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(
            XElement parent1, XElement parent2, string table)
        {
            Contract.Requires(parent1 != null);
            Contract.Requires(parent2 != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(table));

            return (from p1 in parent1.Elements(EdmXNames.Msl.ScalarPropertyNames)
                    from p2 in parent2.Elements(EdmXNames.Msl.ScalarPropertyNames)
                    where p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute())
                    where !p1.ColumnNameAttribute().EqualsIgnoreCase(p2.ColumnNameAttribute())
                    select new RenameColumnOperation(table, p1.ColumnNameAttribute(), p2.ColumnNameAttribute()))
                .Concat(
                    from p1 in parent1.Elements(EdmXNames.Msl.ComplexPropertyNames)
                    from p2 in parent2.Elements(EdmXNames.Msl.ComplexPropertyNames)
                    where p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute())
                    from cr in FindRenamedMappedColumns(p1, p2, table)
                    select cr);
        }

        private IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns()
        {
            return from mf1 in _source.Model.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   from mf2 in _target.Model.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   where mf1.StoreEntitySetAttribute().EqualsIgnoreCase(mf2.StoreEntitySetAttribute())
                   let t = GetQualifiedTableName(_target.Model, mf2.StoreEntitySetAttribute())
                   from cr in FindRenamedDiscriminatorColumns(mf1, mf2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(
            XElement parent1, XElement parent2, string table)
        {
            Contract.Requires(parent1 != null);
            Contract.Requires(parent2 != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(table));

            return from p1 in parent1.Elements(EdmXNames.Msl.ConditionNames)
                   from p2 in parent2.Elements(EdmXNames.Msl.ConditionNames)
                   where p1.ValueAttribute().EqualsIgnoreCase(p2.ValueAttribute())
                   where !p1.ColumnNameAttribute().EqualsIgnoreCase(p2.ColumnNameAttribute())
                   select new RenameColumnOperation(table, p1.ColumnNameAttribute(), p2.ColumnNameAttribute());
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<RenameColumnOperation> FindRenamedIndependentAssociationColumns()
        {
            return from a1 in _source.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                   from a2 in _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                   where a1.NameAttribute().EqualsIgnoreCase(a2.NameAttribute())
                   let d1 = a1.Descendants(EdmXNames.Ssdl.DependentNames).Single()
                   let d2 = a2.Descendants(EdmXNames.Ssdl.DependentNames).Single()
                   from n1 in d1.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                       .Select(x => x.NameAttribute()).Select(
                           (name, index) => new
                                                {
                                                    name,
                                                    index
                                                })
                   from n2 in d2.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                       .Select(x => x.NameAttribute()).Select(
                           (name, index) => new
                                                {
                                                    name,
                                                    index
                                                })
                   where (n1.index == n2.index)
                         && !n1.name.EqualsIgnoreCase(n2.name)
                   let t = GetQualifiedTableName(_target.Model, d2.RoleAttribute())
                   select new RenameColumnOperation(t, n1.name, n2.name);
        }

        private IEnumerable<AddColumnOperation> FindAddedColumns(IEnumerable<RenameColumnOperation> renamedColumns)
        {
            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from p2 in t2.Descendants(EdmXNames.Ssdl.PropertyNames)
                   let columnName = p2.NameAttribute()
                   where !t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                              .Any(p1 => columnName.EqualsIgnoreCase(p1.NameAttribute()))
                         && !renamedColumns
                                 .Any(cr => cr.Table.EqualsIgnoreCase(t) && cr.NewName.EqualsIgnoreCase(columnName))
                   select new AddColumnOperation(t, BuildColumnModel(p2, t2.NameAttribute(), _target));
        }

        private IEnumerable<AlterColumnOperation> FindChangedColumns()
        {
            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from p1 in t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                   from p2 in t2.Descendants(EdmXNames.Ssdl.PropertyNames)
                   where p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute())
                         && !DiffColumns(p1, p2)
                   select BuildAlterColumnOperation(t, p2, t2.NameAttribute(), _target, p1, t1.NameAttribute(), _source);
        }

        private AlterColumnOperation BuildAlterColumnOperation(
            string table,
            XElement targetProperty,
            string targetEntitySetName,
            ModelMetadata targetModelMetadata,
            XElement sourceProperty,
            string sourceEntitySetName,
            ModelMetadata sourceModelMetadata)
        {
            var targetModel = BuildColumnModel(targetProperty, targetEntitySetName, targetModelMetadata);
            var sourceModel = BuildColumnModel(sourceProperty, sourceEntitySetName, sourceModelMetadata);

            return new AlterColumnOperation(
                table,
                targetModel,
                isDestructiveChange: targetModel.IsNarrowerThan(sourceModel, _target.ProviderManifest),
                inverse: new AlterColumnOperation(
                    table,
                    sourceModel,
                    isDestructiveChange: sourceModel.IsNarrowerThan(targetModel, _target.ProviderManifest)));
        }

        private bool DiffColumns(XElement column1, XElement column2)
        {
            Contract.Requires(column1 != null);
            Contract.Requires(column2 != null);

            if (_consistentProviders)
            {
                return CanonicalDeepEquals(column1, column2);
            }

            var c1 = new XElement(column1); // clone
            var c2 = new XElement(column2);

            c1.SetAttributeValue("Type", null);
            c2.SetAttributeValue("Type", null);

            if (((c1.MaxLengthAttribute() != null) && (c2.MaxLengthAttribute() == null))
                || ((c1.MaxLengthAttribute() == null) && (c2.MaxLengthAttribute() != null)))
            {
                c1.SetAttributeValue("MaxLength", null);
                c2.SetAttributeValue("MaxLength", null);
            }

            return CanonicalDeepEquals(c1, c2);
        }

        private static bool CanonicalDeepEquals(XElement element1, XElement element2)
        {
            var canonical1 = Canonicalize(element1);
            var canonical2 = Canonicalize(element2);

            return XNode.DeepEquals(canonical1, canonical2);
        }

        private static XElement Canonicalize(XElement element)
        {
            if (element == null)
            {
                return null;
            }

            var canonical = new XElement(element);

            foreach (var e in canonical.DescendantsAndSelf())
            {
                if (e.Name.Namespace
                    != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }

                e.ReplaceAttributes(
                    e.Attributes()
                        .Select(
                            a => a.IsNamespaceDeclaration
                                     ? null
                                     : (a.Name.Namespace != XNamespace.None)
                                           ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value)
                                           : a)
                        .OrderBy(a => a.Name.LocalName));
            }

            return canonical;
        }

        private IEnumerable<AddForeignKeyOperation> FindAddedForeignKeys(XDocument columnNormalizedSourceModel)
        {
            return _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                .Where(
                    a2 =>
                    !columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.AssociationNames).Any(
                        a1 => DiffAssociations(a1, a2)))
                .Select(a => BuildAddForeignKeyOperation(_target.Model, a));
        }

        private IEnumerable<DropForeignKeyOperation> FindRemovedForeignKeys(XDocument columnNormalizedSourceModel)
        {
            return columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.AssociationNames)
                .Where(
                    a1 =>
                    !_target.Model.Descendants(EdmXNames.Ssdl.AssociationNames).Any(a2 => DiffAssociations(a2, a1)))
                .Select(
                    a => BuildDropForeignKeyOperation(
                        _source.Model,
                        _source.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                             .Single(a2 => a2.NameAttribute().EqualsIgnoreCase(a.NameAttribute()))));
        }

        private IEnumerable<PrimaryKeyOperation> FindChangedPrimaryKeys(XDocument columnNormalizedSourceModel)
        {
            Contract.Requires(columnNormalizedSourceModel != null);

            return from et1 in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from et2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where et1.NameAttribute().EqualsIgnoreCase(et2.NameAttribute())
                   where !CanonicalDeepEquals(
                       et1.Descendants(EdmXNames.Ssdl.KeyNames).Single(),
                       et2.Descendants(EdmXNames.Ssdl.KeyNames).Single())
                   from pko in BuildChangePrimaryKeyOperations(
                       GetQualifiedTableName(_source.Model, et1.NameAttribute()),
                       GetQualifiedTableName(_target.Model, et2.NameAttribute()),
                       _source.Model
                       .Descendants(EdmXNames.Ssdl.EntityTypeNames)
                       .Single(et => et.NameAttribute().EqualsIgnoreCase(et1.NameAttribute()))
                       .Descendants(EdmXNames.Ssdl.KeyNames).Single(),
                       _target.Model
                       .Descendants(EdmXNames.Ssdl.EntityTypeNames)
                       .Single(et => et.NameAttribute().EqualsIgnoreCase(et1.NameAttribute()))
                       .Descendants(EdmXNames.Ssdl.KeyNames).Single())
                   select pko;
        }

        private static IEnumerable<PrimaryKeyOperation> BuildChangePrimaryKeyOperations(
            string oldTable, string newTable, XElement oldKey, XElement newKey)
        {
            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation
                                              {
                                                  Table = oldTable
                                              };

            oldKey.Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                pr => dropPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            yield return dropPrimaryKeyOperation;

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                             {
                                                 Table = newTable
                                             };

            newKey.Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                pr => addPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            yield return addPrimaryKeyOperation;
        }

        private static bool DiffAssociations(XElement a1, XElement a2)
        {
            Contract.Requires(a1 != null);
            Contract.Requires(a2 != null);

            return CanonicalDeepEquals(
                a1.Descendants(EdmXNames.Ssdl.PrincipalNames).Single(),
                a2.Descendants(EdmXNames.Ssdl.PrincipalNames).Single())
                   && CanonicalDeepEquals(
                       a1.Descendants(EdmXNames.Ssdl.DependentNames).Single(),
                       a2.Descendants(EdmXNames.Ssdl.DependentNames).Single())
                   && CanonicalDeepEquals(
                       a2.Descendants(EdmXNames.Ssdl.EndNames).First().Descendants(EdmXNames.Ssdl.OnDeleteNames).
                          SingleOrDefault(),
                       a1.Descendants(EdmXNames.Ssdl.EndNames).First().Descendants(EdmXNames.Ssdl.OnDeleteNames).
                          SingleOrDefault())
                   && CanonicalDeepEquals(
                       a2.Descendants(EdmXNames.Ssdl.EndNames).Last().Descendants(EdmXNames.Ssdl.OnDeleteNames).
                          SingleOrDefault(),
                       a1.Descendants(EdmXNames.Ssdl.EndNames).Last().Descendants(EdmXNames.Ssdl.OnDeleteNames).
                          SingleOrDefault());
        }

        private CreateTableOperation BuildCreateTableOperation(
            string entitySetName,
            string tableName,
            string schema,
            string isSystem,
            ModelMetadata modelMetadata)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(entitySetName));
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));
            Contract.Requires(modelMetadata != null);

            var createTableOperation = new CreateTableOperation(GetQualifiedTableName(tableName, schema));

            var entityTypeElement = modelMetadata.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                .Single(et => et.NameAttribute().EqualsIgnoreCase(entitySetName));

            entityTypeElement
                .Descendants(EdmXNames.Ssdl.PropertyNames)
                .Each(p => createTableOperation.Columns.Add(BuildColumnModel(p, entitySetName, modelMetadata)));

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation();

            entityTypeElement
                .Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => addPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            createTableOperation.PrimaryKey = addPrimaryKeyOperation;
            createTableOperation.IsSystem = isSystem.EqualsIgnoreCase("true");

            return createTableOperation;
        }

        private static ColumnModel BuildColumnModel(
            XElement property, string entitySetName, ModelMetadata modelMetadata)
        {
            Contract.Requires(property != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(entitySetName));
            Contract.Requires(modelMetadata != null);

            var nameAttribute = property.NameAttribute();
            var nullableAttribute = property.NullableAttribute();
            var maxLengthAttribute = property.MaxLengthAttribute();
            var precisionAttribute = property.PrecisionAttribute();
            var scaleAttribute = property.ScaleAttribute();
            var storeGeneratedPatternAttribute = property.StoreGeneratedPatternAttribute();
            var storeType = property.TypeAttribute();

            var entityType
                = modelMetadata.StoreItemCollection
                    .OfType<EntityType>()
                    .Single(et => et.Name.EqualsIgnoreCase(entitySetName));

            var edmProperty
                = entityType.Properties[nameAttribute];

            var typeUsage = modelMetadata.ProviderManifest.GetEdmType(edmProperty.TypeUsage);

            var defaultStoreTypeName = modelMetadata.ProviderManifest.GetStoreType(typeUsage).EdmType.Name;

            var column
                = new ColumnModel(((PrimitiveType)edmProperty.TypeUsage.EdmType).PrimitiveTypeKind, typeUsage)
                      {
                          Name = nameAttribute,
                          IsNullable
                              = !string.IsNullOrWhiteSpace(nullableAttribute)
                                && !Convert.ToBoolean(nullableAttribute, CultureInfo.InvariantCulture)
                                    ? false
                                    : (bool?)null,
                          MaxLength
                              = !string.IsNullOrWhiteSpace(maxLengthAttribute)
                                    ? Convert.ToInt32(maxLengthAttribute, CultureInfo.InvariantCulture)
                                    : (int?)null,
                          Precision
                              = !string.IsNullOrWhiteSpace(precisionAttribute)
                                    ? Convert.ToByte(precisionAttribute, CultureInfo.InvariantCulture)
                                    : (byte?)null,
                          Scale
                              = !string.IsNullOrWhiteSpace(scaleAttribute)
                                    ? Convert.ToByte(scaleAttribute, CultureInfo.InvariantCulture)
                                    : (byte?)null,
                          StoreType
                              = !storeType.EqualsIgnoreCase(defaultStoreTypeName)
                                    ? storeType
                                    : null
                      };

            column.IsIdentity
                = !string.IsNullOrWhiteSpace(storeGeneratedPatternAttribute)
                  && storeGeneratedPatternAttribute.EqualsIgnoreCase("Identity")
                  && _validIdentityTypes.Contains(column.Type);

            Facet facet;
            if (typeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, true, out facet)
                && facet.Value != null
                && (bool)facet.Value)
            {
                column.IsFixedLength = true;
            }

            if (typeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, true, out facet)
                && facet.Value != null
                && !(bool)facet.Value)
            {
                column.IsUnicode = false;
            }

            var isComputed
                = !string.IsNullOrWhiteSpace(storeGeneratedPatternAttribute)
                  && storeGeneratedPatternAttribute.EqualsIgnoreCase("Computed");

            if ((column.Type == PrimitiveTypeKind.Binary)
                && (typeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out facet)
                    && (facet.Value is int)
                    && ((int)facet.Value == 8))
                && isComputed)
            {
                column.IsTimestamp = true;
            }

            return column;
        }

        private AddForeignKeyOperation BuildAddForeignKeyOperation(XDocument edmx, XElement association)
        {
            Contract.Requires(edmx != null);
            Contract.Requires(association != null);

            var addForeignKeyOperation = new AddForeignKeyOperation();

            BuildForeignKeyOperation(edmx, association, addForeignKeyOperation);

            var principal = association.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();

            principal.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => addForeignKeyOperation.PrincipalColumns.Add(pr.NameAttribute()));

            var onDelete = association.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault();

            if ((onDelete != null)
                && onDelete.ActionAttribute().EqualsIgnoreCase("Cascade"))
            {
                addForeignKeyOperation.CascadeDelete = true;
            }

            return addForeignKeyOperation;
        }

        private DropForeignKeyOperation BuildDropForeignKeyOperation(XDocument edmx, XElement association)
        {
            Contract.Requires(edmx != null);
            Contract.Requires(association != null);

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(BuildAddForeignKeyOperation(edmx, association));

            BuildForeignKeyOperation(edmx, association, dropForeignKeyOperation);

            return dropForeignKeyOperation;
        }

        private void BuildForeignKeyOperation(
            XDocument edmx, XElement association, ForeignKeyOperation foreignKeyOperation)
        {
            Contract.Requires(edmx != null);
            Contract.Requires(association != null);
            Contract.Requires(foreignKeyOperation != null);

            var principal = association.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();
            var dependent = association.Descendants(EdmXNames.Ssdl.DependentNames).Single();

            var principalRole =
                association.Descendants(EdmXNames.Ssdl.EndNames).Single(
                    r => r.RoleAttribute() == principal.RoleAttribute());
            var dependentRole =
                association.Descendants(EdmXNames.Ssdl.EndNames).Single(
                    r => r.RoleAttribute() == dependent.RoleAttribute());

            var principalTable = GetQualifiedTableNameFromType(edmx, principalRole.TypeAttribute());
            var dependentTable = GetQualifiedTableNameFromType(edmx, dependentRole.TypeAttribute());

            foreignKeyOperation.PrincipalTable = principalTable;
            foreignKeyOperation.DependentTable = dependentTable;

            dependent.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => foreignKeyOperation.DependentColumns.Add(pr.NameAttribute()));
        }

        private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
        {
            var providerFactory = DbProviderFactories.GetFactory(providerInfo.ProviderInvariantName);

            return providerFactory.GetProviderServices().GetProviderManifest(providerInfo.ProviderManifestToken);
        }

        public virtual string GetQualifiedTableName(string table, string schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));

            return schema + "." + table;
        }

        private string GetQualifiedTableName(XDocument model, string entitySetName)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(entitySetName));

            var schemaAndTable
                = (from es in model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   where es.NameAttribute().EqualsIgnoreCase(entitySetName)
                   select new
                              {
                                  Schema = es.SchemaAttribute(),
                                  Table = es.TableAttribute()
                              })
                    .Single();

            return GetQualifiedTableName(schemaAndTable.Table, schemaAndTable.Schema);
        }

        private string GetQualifiedTableNameFromType(XDocument model, string entityTypeName)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(entityTypeName));

            var schemaAndTable
                = (from es in model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   where es.EntityTypeAttribute().EqualsIgnoreCase(entityTypeName)
                   select new
                              {
                                  Schema = es.SchemaAttribute(),
                                  Table = es.TableAttribute()
                              })
                    .Single();

            return GetQualifiedTableName(schemaAndTable.Table, schemaAndTable.Schema);
        }
    }
}
