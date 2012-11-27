// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal static class EdmModelExtensions
    {
        private const string ProviderInfoAnnotation = "ProviderInfo";

        public static EdmModel Initialize(this EdmModel model, double version = XmlConstants.EdmVersionForV3)
        {
            DebugCheck.NotNull(model);

            model.Name = "CodeFirstModel";
            model.Version = version;
            model.Containers.Add(new EntityContainer("CodeFirstContainer", DataSpace.CSpace));
            model.Namespaces.Add(
                new EdmNamespace
                    {
                        Name = "CodeFirstNamespace"
                    });

            return model;
        }

        public static DbProviderInfo GetProviderInfo(this EdmModel model)
        {
            DebugCheck.NotNull(model);

            return (DbProviderInfo)model.Annotations.GetAnnotation(ProviderInfoAnnotation);
        }

        public static void SetProviderInfo(this EdmModel model, DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(providerInfo);

            model.Annotations.SetAnnotation(ProviderInfoAnnotation, providerInfo);
        }

        public static bool HasCascadeDeletePath(
            this EdmModel model, EntityType sourceEntityType, EntityType targetEntityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(sourceEntityType);
            DebugCheck.NotNull(targetEntityType);

            return (from a in model.GetAssociationTypes()
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
            Debug.Assert(model.Containers.Count == 1);

            return model.GetEntityTypes().Select(e => e.GetClrType())
                        .Union(model.GetComplexTypes().Select(ct => ct.GetClrType()));
        }

        public static NavigationProperty GetNavigationProperty(this EdmModel model, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(propertyInfo);

            var navigationProperties
                = (from e in model.GetEntityTypes()
                   let np = e.GetNavigationProperty(propertyInfo)
                   where np != null
                   select np);

            return navigationProperties.FirstOrDefault();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static EdmItemCollection ToEdmItemCollection(this EdmModel model)
        {
            DebugCheck.NotNull(model);

            var stringBuilder = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(
                stringBuilder, new XmlWriterSettings
                                   {
                                       Indent = true
                                   }))
            {
                model.ValidateAndSerializeCsdl(xmlWriter);
            }

            using (var xmlReader = XmlReader.Create(new StringReader(stringBuilder.ToString())))
            {
                return new EdmItemCollection(new[] { xmlReader });
            }
        }

        public static void ValidateCsdl(this EdmModel model)
        {
            DebugCheck.NotNull(model);

            model.ValidateAndSerializeCsdl(XmlWriter.Create(Stream.Null));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static List<DataModelErrorEventArgs> GetCsdlErrors(this EdmModel model)
        {
            DebugCheck.NotNull(model);

            return model.SerializeAndGetCsdlErrors(XmlWriter.Create(Stream.Null));
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

        public static List<DataModelErrorEventArgs> SerializeAndGetCsdlErrors(this EdmModel model, XmlWriter writer)
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
            this EdmModel model, DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Namespaces.Count == 1);

            return new DatabaseMappingGenerator(providerManifest).Generate(model);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmType GetStructuralType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            return (EdmType)model.GetEntityType(name) ?? model.GetComplexType(name);
        }

        public static EntityType GetEntityType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EntityType GetEntityType(this EdmModel model, Type clrType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(clrType);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static ComplexType GetComplexType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes.SingleOrDefault(e => e.Name == name);
        }

        public static ComplexType GetComplexType(this EdmModel model, Type clrType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(clrType);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static EnumType GetEnumType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EnumTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EntityType AddEntityType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            var entityType
                = new EntityType(
                    name,
                    XmlConstants.GetCsdlNamespace(model.Version),
                    DataSpace.CSpace);

            model.Namespaces.Single().EntityTypes.Add(entityType);

            return entityType;
        }

        public static EntitySet GetEntitySet(this EdmModel model, EntityType entityType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(entityType);
            Debug.Assert(model.Containers.Count == 1);

            return model.GetEntitySets().SingleOrDefault(e => e.ElementType == entityType.GetRootType());
        }

        public static AssociationSet GetAssociationSet(this EdmModel model, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationType);
            Debug.Assert(model.Containers.Count == 1);

            return model.Containers.Single().AssociationSets.SingleOrDefault(a => a.ElementType == associationType);
        }

        public static IEnumerable<EntitySet> GetEntitySets(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Containers.Count == 1);

            return model.Containers.Single().EntitySets;
        }

        public static EntitySet AddEntitySet(
            this EdmModel model, string name, EntityType elementType, string table = null)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(elementType);
            Debug.Assert(model.Containers.Count == 1);

            var entitySet = new EntitySet(name, null, table, null, elementType);

            // TODO: METADATA: Naming uniqueness constraint
            model.Containers.Single().AddEntitySetBase(entitySet);

            return entitySet;
        }

        public static ComplexType AddComplexType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            var complexType
                = new ComplexType(
                    name,
                    XmlConstants.GetCsdlNamespace(model.Version),
                    DataSpace.CSpace);

            model.Namespaces.Single().ComplexTypes.Add(complexType);

            return complexType;
        }

        public static EnumType AddEnumType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            var enumType
                = new EnumType(
                    name,
                    XmlConstants.GetCsdlNamespace(model.Version),
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                    false,
                    DataSpace.CSpace);

            model.Namespaces.Single().EnumTypes.Add(enumType);

            return enumType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static AssociationType GetAssociationType(this EdmModel model, string name)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().AssociationTypes.SingleOrDefault(a => a.Name == name);
        }

        public static IEnumerable<AssociationType> GetAssociationTypes(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().AssociationTypes;
        }

        public static IEnumerable<EntityType> GetEntityTypes(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes;
        }

        public static IEnumerable<ComplexType> GetComplexTypes(this EdmModel model)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes;
        }

        public static IEnumerable<AssociationType> GetAssociationTypesBetween(
            this EdmModel model, EntityType first, EntityType second)
        {
            DebugCheck.NotNull(model);
            Debug.Assert(model.Namespaces.Count == 1);

            return model.GetAssociationTypes().Where(
                a => (a.SourceEnd.GetEntityType() == first && a.TargetEnd.GetEntityType() == second)
                     || (a.SourceEnd.GetEntityType() == second && a.TargetEnd.GetEntityType() == first));
        }

        public static AssociationType AddAssociationType(
            this EdmModel model,
            string name,
            EntityType sourceEntityType,
            RelationshipMultiplicity sourceAssociationEndKind,
            EntityType targetEntityType,
            RelationshipMultiplicity targetAssociationEndKind)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(sourceEntityType);
            DebugCheck.NotNull(targetEntityType);
            Debug.Assert(model.Namespaces.Count == 1);

            var associationType
                = new AssociationType(
                    name,
                    XmlConstants.GetCsdlNamespace(model.Version),
                    false,
                    DataSpace.CSpace)
                      {
                          SourceEnd =
                              new AssociationEndMember(
                              name + "_Source", new RefType(sourceEntityType), sourceAssociationEndKind),
                          TargetEnd =
                              new AssociationEndMember(
                              name + "_Target", new RefType(targetEntityType), targetAssociationEndKind)
                      };

            model.AddAssociationType(associationType);

            return associationType;
        }

        public static void AddAssociationType(this EdmModel model, AssociationType associationType)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(associationType);

            model.Namespaces.Single().AssociationTypes.Add(associationType);
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
            Debug.Assert(model.Namespaces.Count == 1);
            Debug.Assert(model.Containers.Count == 1);

            model.Namespaces.Single().EntityTypes.Remove(entityType);

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
            Debug.Assert(model.Containers.Count == 1);

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
            Debug.Assert(model.Namespaces.Count == 1);
            Debug.Assert(model.Containers.Count == 1);

            model.Namespaces.Single().AssociationTypes.Remove(associationType);

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
            Debug.Assert(model.Containers.Count == 1);

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

            return model.GetEntityTypes().Where(et => et.BaseType == entityType);
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
            foreach (var derivedType in model.GetEntityTypes().Where(et => et.BaseType == entityType))
            {
                AddSelfAndAllDerivedTypes(model, derivedType, entityTypes);
            }
        }
    }
}
