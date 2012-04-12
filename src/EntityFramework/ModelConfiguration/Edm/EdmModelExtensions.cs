namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Services;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal static class EdmModelExtensions
    {
        private const string ProviderInfoAnnotation = "ProviderInfo";

        public static EdmModel Initialize(this EdmModel model, double version = DataModelVersions.Version3)
        {
            Contract.Requires(model != null);

            model.Name = "CodeFirstModel";
            model.Version = version;
            model.Containers.Add(
                new EdmEntityContainer
                    {
                        Name = "CodeFirstContainer"
                    });
            model.Namespaces.Add(
                new EdmNamespace
                    {
                        Name = "CodeFirstNamespace"
                    });

            return model;
        }

        public static DbProviderInfo GetProviderInfo(this EdmModel model)
        {
            Contract.Requires(model != null);

            return (DbProviderInfo)model.Annotations.GetAnnotation(ProviderInfoAnnotation);
        }

        public static void SetProviderInfo(this EdmModel model, DbProviderInfo providerInfo)
        {
            Contract.Requires(model != null);
            Contract.Requires(providerInfo != null);

            model.Annotations.SetAnnotation(ProviderInfoAnnotation, providerInfo);
        }

        public static bool HasCascadeDeletePath(
            this EdmModel model, EdmEntityType sourceEntityType, EdmEntityType targetEntityType)
        {
            Contract.Requires(model != null);
            Contract.Requires(sourceEntityType != null);
            Contract.Requires(targetEntityType != null);

            return (from a in model.GetAssociationTypes()
                    from ae in a.Members.Cast<EdmAssociationEnd>()
                    where ae.EntityType == sourceEntityType
                          && ae.DeleteAction == EdmOperationAction.Cascade
                    select a.GetOtherEnd(ae).EntityType)
                .Any(
                    et => (et == targetEntityType)
                          || model.HasCascadeDeletePath(et, targetEntityType));
        }

        public static IEnumerable<Type> GetClrTypes(this EdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Containers.Count == 1);

            return model.GetEntityTypes().Select(e => e.GetClrType())
                .Union(model.GetComplexTypes().Select(ct => ct.GetClrType()));
        }

        public static EdmNavigationProperty GetNavigationProperty(this EdmModel model, PropertyInfo propertyInfo)
        {
            Contract.Requires(model != null);
            Contract.Requires(propertyInfo != null);

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
            Contract.Requires(model != null);

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
            Contract.Requires(model != null);

            model.ValidateAndSerializeCsdl(XmlWriter.Create(Stream.Null));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static List<DataModelErrorEventArgs> GetCsdlErrors(this EdmModel model)
        {
            Contract.Requires(model != null);

            return model.SerializeAndGetCsdlErrors(XmlWriter.Create(Stream.Null));
        }

        public static void ValidateAndSerializeCsdl(this EdmModel model, XmlWriter writer)
        {
            Contract.Requires(model != null);
            Contract.Requires(writer != null);

            var validationErrors = model.SerializeAndGetCsdlErrors(writer);

            if (validationErrors.Count > 0)
            {
                throw new ModelValidationException(validationErrors);
            }
        }

        public static List<DataModelErrorEventArgs> SerializeAndGetCsdlErrors(this EdmModel model, XmlWriter writer)
        {
            Contract.Requires(model != null);
            Contract.Requires(writer != null);

            var validationErrors = new List<DataModelErrorEventArgs>();
            var csdlSerializer = new CsdlSerializer();

            csdlSerializer.OnError += (s, e) => validationErrors.Add(e);

            csdlSerializer.Serialize(model, writer);

            return validationErrors;
        }

        public static DbDatabaseMapping GenerateDatabaseMapping(
            this EdmModel model, DbProviderManifest providerManifest)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return new DatabaseMappingGenerator(providerManifest).Generate(model);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmNamespaceItem GetStructuralType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            return (EdmNamespaceItem)model.GetEntityType(name) ?? model.GetComplexType(name);
        }

        public static EdmEntityType GetEntityType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EdmEntityType GetEntityType(this EdmModel model, Type clrType)
        {
            Contract.Requires(model != null);
            Contract.Requires(clrType != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static EdmComplexType GetComplexType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EdmComplexType GetComplexType(this EdmModel model, Type clrType)
        {
            Contract.Requires(model != null);
            Contract.Requires(clrType != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes.SingleOrDefault(e => e.GetClrType() == clrType);
        }

        public static EdmEnumType GetEnumType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EnumTypes.SingleOrDefault(e => e.Name == name);
        }

        public static EdmEntityType AddEntityType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            var entityType = new EdmEntityType
                                 {
                                     Name = name
                                 };

            model.Namespaces.Single().EntityTypes.Add(entityType);

            return entityType;
        }

        public static EdmEntitySet GetEntitySet(this EdmModel model, EdmEntityType entityType)
        {
            Contract.Requires(model != null);
            Contract.Requires(entityType != null);
            Contract.Assert(model.Containers.Count == 1);

            return model.GetEntitySets().SingleOrDefault(e => e.ElementType == entityType.GetRootType());
        }

        public static EdmAssociationSet GetAssociationSet(this EdmModel model, EdmAssociationType associationType)
        {
            Contract.Requires(model != null);
            Contract.Requires(associationType != null);
            Contract.Assert(model.Containers.Count == 1);

            return model.Containers.Single().AssociationSets.SingleOrDefault(a => a.ElementType == associationType);
        }

        public static IEnumerable<EdmEntitySet> GetEntitySets(this EdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Containers.Count == 1);

            return model.Containers.Single().EntitySets;
        }

        public static EdmEntitySet AddEntitySet(
            this EdmModel model, string name, EdmEntityType elementType)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(elementType != null);
            Contract.Assert(model.Containers.Count == 1);

            var entitySet = new EdmEntitySet
                                {
                                    Name = name,
                                    ElementType = elementType
                                };

            model.Containers.Single().EntitySets.Add(entitySet);

            return entitySet;
        }

        public static EdmComplexType AddComplexType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            var complexType = new EdmComplexType
                                  {
                                      Name = name
                                  };

            model.Namespaces.Single().ComplexTypes.Add(complexType);

            return complexType;
        }

        public static EdmEnumType AddEnumType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            var enumType = new EdmEnumType
                               {
                                   Name = name
                               };

            model.Namespaces.Single().EnumTypes.Add(enumType);

            return enumType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmAssociationType GetAssociationType(this EdmModel model, string name)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().AssociationTypes.SingleOrDefault(a => a.Name == name);
        }

        public static IEnumerable<EdmAssociationType> GetAssociationTypes(this EdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().AssociationTypes;
        }

        public static IEnumerable<EdmEntityType> GetEntityTypes(this EdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().EntityTypes;
        }

        public static IEnumerable<EdmComplexType> GetComplexTypes(this EdmModel model)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.Namespaces.Single().ComplexTypes;
        }

        public static IEnumerable<EdmAssociationType> GetAssociationTypesBetween(
            this EdmModel model, EdmEntityType first, EdmEntityType second)
        {
            Contract.Requires(model != null);
            Contract.Assert(model.Namespaces.Count == 1);

            return model.GetAssociationTypes().Where(
                a => (a.SourceEnd.EntityType == first && a.TargetEnd.EntityType == second)
                     || (a.SourceEnd.EntityType == second && a.TargetEnd.EntityType == first));
        }

        public static EdmAssociationType AddAssociationType(
            this EdmModel model,
            string name,
            EdmEntityType sourceEntityType,
            EdmAssociationEndKind sourceAssociationEndKind,
            EdmEntityType targetEntityType,
            EdmAssociationEndKind targetAssociationEndKind)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(sourceEntityType != null);
            Contract.Requires(targetEntityType != null);
            Contract.Assert(model.Namespaces.Count == 1);

            var associationType = new EdmAssociationType().Initialize();

            associationType.Name = name;
            associationType.SourceEnd.Name = name + "_Source";
            associationType.SourceEnd.EntityType = sourceEntityType;
            associationType.SourceEnd.EndKind = sourceAssociationEndKind;
            associationType.TargetEnd.Name = name + "_Target";
            associationType.TargetEnd.EntityType = targetEntityType;
            associationType.TargetEnd.EndKind = targetAssociationEndKind;

            model.AddAssociationType(associationType);

            return associationType;
        }

        public static void AddAssociationType(this EdmModel model, EdmAssociationType associationType)
        {
            Contract.Requires(model != null);
            Contract.Requires(associationType != null);

            model.Namespaces.Single().AssociationTypes.Add(associationType);
        }

        public static void RemoveEntityType(
            this EdmModel model, EdmEntityType entityType)
        {
            Contract.Requires(model != null);
            Contract.Requires(entityType != null);
            Contract.Assert(model.Namespaces.Count == 1);
            Contract.Assert(model.Containers.Count == 1);

            model.Namespaces.Single().EntityTypes.Remove(entityType);

            var container = model.Containers.Single();

            container.EntitySets.Remove(container.EntitySets.SingleOrDefault(a => a.ElementType == entityType));
        }

        public static void ReplaceEntitySet(
            this EdmModel model, EdmEntityType entityType, EdmEntitySet newSet)
        {
            Contract.Requires(model != null);
            Contract.Requires(entityType != null);
            Contract.Assert(model.Containers.Count == 1);

            var container = model.Containers.Single();

            var entitySet = container.EntitySets.SingleOrDefault(a => a.ElementType == entityType);
            container.EntitySets.Remove(entitySet);

            if (entitySet != null
                && newSet != null)
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

        public static void RemoveAssociationType(
            this EdmModel model, EdmAssociationType associationType)
        {
            Contract.Requires(model != null);
            Contract.Requires(associationType != null);
            Contract.Assert(model.Namespaces.Count == 1);
            Contract.Assert(model.Containers.Count == 1);

            model.Namespaces.Single().AssociationTypes.Remove(associationType);

            var container = model.Containers.Single();

            container.AssociationSets.Remove(
                container.AssociationSets.SingleOrDefault(a => a.ElementType == associationType));
        }

        public static EdmAssociationSet AddAssociationSet(
            this EdmModel model, string name, EdmAssociationType associationType)
        {
            Contract.Requires(model != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(associationType != null);
            Contract.Assert(model.Containers.Count == 1);

            var associationSet = new EdmAssociationSet
                                     {
                                         Name = name,
                                         ElementType = associationType,
                                         SourceSet = model.GetEntitySet(associationType.SourceEnd.EntityType),
                                         TargetSet = model.GetEntitySet(associationType.TargetEnd.EntityType)
                                     };

            model.Containers.Single().AssociationSets.Add(associationSet);

            return associationSet;
        }

        public static IEnumerable<EdmEntityType> GetDerivedTypes(
            this EdmModel model, EdmEntityType entityType)
        {
            Contract.Requires(model != null);
            Contract.Requires(entityType != null);

            return model.GetEntityTypes().Where(et => et.BaseType == entityType);
        }

        public static IEnumerable<EdmEntityType> GetSelfAndAllDerivedTypes(
            this EdmModel model, EdmEntityType entityType)
        {
            Contract.Requires(model != null);
            Contract.Requires(entityType != null);

            var entityTypes = new List<EdmEntityType>();
            AddSelfAndAllDerivedTypes(model, entityType, entityTypes);
            return entityTypes;
        }

        private static void AddSelfAndAllDerivedTypes(
            EdmModel model, EdmEntityType entityType, List<EdmEntityType> entityTypes)
        {
            entityTypes.Add(entityType);
            foreach (var derivedType in model.GetEntityTypes().Where(et => et.BaseType == entityType))
            {
                AddSelfAndAllDerivedTypes(model, derivedType, entityTypes);
            }
        }
    }
}
