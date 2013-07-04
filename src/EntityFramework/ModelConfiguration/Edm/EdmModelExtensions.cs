// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.Edm.Validation;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class EdmModelExtensions
    {
        public const string DefaultSchema = "dbo";
        public const string DefaultModelNamespace = "CodeFirstNamespace";
        public const string DefaultStoreNamespace = "CodeFirstDatabaseSchema";

        public static EntityType AddTable(this EdmModel database, string name)
        {
            DebugCheck.NotEmpty(name);

            var uniqueIdentifier = database.EntityTypes.UniquifyName(name);

            var table
                = new EntityType(
                    uniqueIdentifier,
                    DefaultStoreNamespace,
                    DataSpace.SSpace);

            database.AddItem(table);
            database.AddEntitySet(table.Name, table, uniqueIdentifier);

            return table;
        }

        public static EntityType AddTable(
            this EdmModel database, string name, EntityType pkSource)
        {
            var table = database.AddTable(name);

            // Add PK columns to the new table
            foreach (var property in pkSource.KeyProperties)
            {
                table.AddKeyMember(property.Clone());
            }

            return table;
        }

        public static EdmFunction AddFunction(this EdmModel database, string name, EdmFunctionPayload functionPayload)
        {
            DebugCheck.NotNull(database);
            DebugCheck.NotEmpty(name);

            var uniqueIdentifier = database.Functions.UniquifyName(name);

            var function
                = new EdmFunction(
                    uniqueIdentifier,
                    DefaultStoreNamespace,
                    DataSpace.SSpace,
                    functionPayload);

            database.AddItem(function);

            return function;
        }

        public static EntityType FindTableByName(this EdmModel database, DatabaseName tableName)
        {
            DebugCheck.NotNull(tableName);

            return database.EntityTypes.SingleOrDefault(
                t =>
                    {
                        var databaseName = t.GetTableName();
                        return databaseName != null
                                   ? databaseName.Equals(tableName)
                                   : string.Equals(t.Name, tableName.Name, StringComparison.Ordinal);
                    });
        }

        public static bool HasCascadeDeletePath(
            this EdmModel model, EntityType sourceEntityType, EntityType targetEntityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(sourceEntityType);
            DebugCheck.NotNull(targetEntityType);

            return (from a in model.AssociationTypes
                    from ae in a.Members.Cast<AssociationEndMember>()
                    where ae.GetEntityType() == sourceEntityType
                          && ae.DeleteBehavior == OperationAction.Cascade
                    select a.GetOtherEnd(ae).GetEntityType())
                .Any(
                    et => (et == targetEntityType)
                          || model.HasCascadeDeletePath(et, targetEntityType));
        }

        public static IEnumerable<Type> GetClrTypes(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Containers.Count() == 1);

            return model.EntityTypes.Select(e => e.GetClrType())
                        .Union(model.ComplexTypes.Select(ct => ct.GetClrType()));
        }

        public static NavigationProperty GetNavigationProperty(this EdmModel model, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(propertyInfo);

            var navigationProperties
                = (from e in model.EntityTypes
                   let np = e.GetNavigationProperty(propertyInfo)
                   where np != null
                   select np);

            return navigationProperties.FirstOrDefault();
        }

        public static void ValidateAndSerializeCsdl(this EdmModel model, XmlWriter writer)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(writer);

            var validationErrors = model.SerializeAndGetCsdlErrors(writer);

            if (validationErrors.Count > 0)
            {
                throw new ModelValidationException(validationErrors);
            }
        }

        private static List<DataModelErrorEventArgs> SerializeAndGetCsdlErrors(this EdmModel model, XmlWriter writer)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(writer);

            var validationErrors = new List<DataModelErrorEventArgs>();
            var csdlSerializer = new CsdlSerializer();

            csdlSerializer.OnError += (s, e) => validationErrors.Add(e);

            csdlSerializer.Serialize(model, writer);

            return validationErrors;
        }

        public static DbDatabaseMapping GenerateDatabaseMapping(
            this EdmModel model, DbProviderInfo providerInfo, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(model);

            return new DatabaseMappingGenerator(providerInfo, providerManifest).Generate(model);
        }

        public static EdmType GetStructuralOrEnumType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return model.GetStructuralType(name) ?? model.GetEnumType(name);
        }

        public static EdmType GetStructuralType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return (EdmType)model.GetEntityType(name) ?? model.GetComplexType(name);
        }

        public static EntityType GetEntityType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return model.EntityTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EntityType GetEntityType(this EdmModel model, Type clrType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(clrType);

            return model.EntityTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static ComplexType GetComplexType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return model.ComplexTypes.SingleOrDefault(e => e.Name == name);
        }

        public static ComplexType GetComplexType(this EdmModel model, Type clrType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(clrType);

            return model.ComplexTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static EnumType GetEnumType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return model.EnumTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EntityType AddEntityType(this EdmModel model, string name, string modelNamespace = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            var entityType
                = new EntityType(
                    name,
                    modelNamespace ?? DefaultModelNamespace,
                    DataSpace.CSpace);

            model.AddItem(entityType);

            return entityType;
        }

        public static EntitySet GetEntitySet(this EdmModel model, EntityType entityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);
            Debug.Assert(model.Containers.Count() == 1);

            return model.GetEntitySets().SingleOrDefault(e => e.ElementType == entityType.GetRootType());
        }

        public static AssociationSet GetAssociationSet(this EdmModel model, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationType);
            Debug.Assert(model.Containers.Count() == 1);

            return model.Containers.Single().AssociationSets.SingleOrDefault(a => a.ElementType == associationType);
        }

        public static IEnumerable<EntitySet> GetEntitySets(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Containers.Count() == 1);

            return model.Containers.Single().EntitySets;
        }

        public static EntitySet AddEntitySet(
            this EdmModel model, string name, EntityType elementType, string table = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(elementType);
            Debug.Assert(model.Containers.Count() == 1);

            var entitySet = new EntitySet(name, null, table, null, elementType);

            model.Containers.Single().AddEntitySetBase(entitySet);

            return entitySet;
        }

        public static ComplexType AddComplexType(this EdmModel model, string name, string modelNamespace = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            var complexType
                = new ComplexType(
                    name,
                    modelNamespace ?? DefaultModelNamespace,
                    DataSpace.CSpace);

            model.AddItem(complexType);

            return complexType;
        }

        public static EnumType AddEnumType(this EdmModel model, string name, string modelNamespace = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            var enumType
                = new EnumType(
                    name,
                    modelNamespace ?? DefaultModelNamespace,
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                    false,
                    DataSpace.CSpace);

            model.AddItem(enumType);

            return enumType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static AssociationType GetAssociationType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);

            return model.AssociationTypes.SingleOrDefault(a => a.Name == name);
        }

        public static IEnumerable<AssociationType> GetAssociationTypesBetween(
            this EdmModel model, EntityType first, EntityType second)
        {
            DebugCheck.NotNull(model);

            return model.AssociationTypes.Where(
                a => (a.SourceEnd.GetEntityType() == first && a.TargetEnd.GetEntityType() == second)
                     || (a.SourceEnd.GetEntityType() == second && a.TargetEnd.GetEntityType() == first));
        }

        public static AssociationType AddAssociationType(
            this EdmModel model,
            string name,
            EntityType sourceEntityType,
            RelationshipMultiplicity sourceAssociationEndKind,
            EntityType targetEntityType,
            RelationshipMultiplicity targetAssociationEndKind,
            string modelNamespace = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(sourceEntityType);
            DebugCheck.NotNull(targetEntityType);

            var associationType
                = new AssociationType(
                    name,
                    modelNamespace ?? DefaultModelNamespace,
                    false,
                    DataSpace.CSpace)
                    {
                        SourceEnd =
                            new AssociationEndMember(
                                name + "_Source", sourceEntityType.GetReferenceType(), sourceAssociationEndKind),
                        TargetEnd =
                            new AssociationEndMember(
                                name + "_Target", targetEntityType.GetReferenceType(), targetAssociationEndKind)
                    };

            model.AddAssociationType(associationType);

            return associationType;
        }

        public static void AddAssociationType(this EdmModel model, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationType);

            model.AddItem(associationType);
        }

        public static void AddAssociationSet(this EdmModel model, AssociationSet associationSet)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationSet);

            model.Containers.Single().AddEntitySetBase(associationSet);
        }

        public static void RemoveEntityType(
            this EdmModel model, EntityType entityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);
            Debug.Assert(model.Containers.Count() == 1);

            model.RemoveItem(entityType);

            var container = model.Containers.Single();

            var entitySet = container.EntitySets.SingleOrDefault(a => a.ElementType == entityType);

            if (entitySet != null)
            {
                container.RemoveEntitySetBase(entitySet);
            }
        }

        public static void ReplaceEntitySet(
            this EdmModel model, EntityType entityType, EntitySet newSet)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);
            Debug.Assert(model.Containers.Count() == 1);

            var container = model.Containers.Single();
            var entitySet = container.EntitySets.SingleOrDefault(a => a.ElementType == entityType);

            if (entitySet != null)
            {
                container.RemoveEntitySetBase(entitySet);

                if (newSet != null)
                {
                    // Update AssociationSets to point to entitySet instead of derivedEntitySet
                    foreach (var associationSet in model.Containers.Single().AssociationSets)
                    {
                        if (associationSet.SourceSet == entitySet)
                        {
                            associationSet.SourceSet = newSet;
                        }
                        if (associationSet.TargetSet == entitySet)
                        {
                            associationSet.TargetSet = newSet;
                        }
                    }
                }
            }
        }

        public static void RemoveAssociationType(
            this EdmModel model, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationType);
            Debug.Assert(model.Containers.Count() == 1);

            model.RemoveItem(associationType);

            var container = model.Containers.Single();

            var associationSet
                = container.AssociationSets.SingleOrDefault(a => a.ElementType == associationType);

            if (associationSet != null)
            {
                container.RemoveEntitySetBase(associationSet);
            }
        }

        public static AssociationSet AddAssociationSet(
            this EdmModel model, string name, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(associationType);
            Debug.Assert(model.Containers.Count() == 1);

            var associationSet
                = new AssociationSet(name, associationType)
                    {
                        SourceSet = model.GetEntitySet(associationType.SourceEnd.GetEntityType()),
                        TargetSet = model.GetEntitySet(associationType.TargetEnd.GetEntityType())
                    };

            model.Containers.Single().AddEntitySetBase(associationSet);

            return associationSet;
        }

        public static IEnumerable<EntityType> GetDerivedTypes(
            this EdmModel model, EntityType entityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);

            return model.EntityTypes.Where(et => et.BaseType == entityType);
        }

        public static IEnumerable<EntityType> GetSelfAndAllDerivedTypes(
            this EdmModel model, EntityType entityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);

            var entityTypes = new List<EntityType>();
            AddSelfAndAllDerivedTypes(model, entityType, entityTypes);
            return entityTypes;
        }

        private static void AddSelfAndAllDerivedTypes(
            EdmModel model, EntityType entityType, List<EntityType> entityTypes)
        {
            entityTypes.Add(entityType);
            foreach (var derivedType in model.EntityTypes.Where(et => et.BaseType == entityType))
            {
                AddSelfAndAllDerivedTypes(model, derivedType, entityTypes);
            }
        }
    }
}
