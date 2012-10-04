// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents the base item class for all the metadata
    /// </summary>
    public abstract partial class MetadataItem
    {
        /// <summary>
        ///     Static Constructor which initializes all the built in types and primitive types
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static MetadataItem()
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Bootstrapping the builtin types
            ////////////////////////////////////////////////////////////////////////////////////////////////
            _builtInTypes[(int)BuiltInTypeKind.AssociationEndMember] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.AssociationSet] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.AssociationSetEnd] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.AssociationType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.AssociationType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.CollectionKind] = new EnumType();
            _builtInTypes[(int)BuiltInTypeKind.CollectionType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.ComplexType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.Documentation] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.OperationAction] = new EnumType();
            _builtInTypes[(int)BuiltInTypeKind.EdmType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EntityContainer] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EntitySet] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EntityType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EntitySetBase] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EntityTypeBase] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EnumType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EnumMember] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.Facet] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EdmFunction] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.FunctionParameter] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.GlobalItem] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.MetadataProperty] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.NavigationProperty] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.MetadataItem] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.EdmMember] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.ParameterMode] = new EnumType();
            _builtInTypes[(int)BuiltInTypeKind.PrimitiveType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.PrimitiveTypeKind] = new EnumType();
            _builtInTypes[(int)BuiltInTypeKind.EdmProperty] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.ProviderManifest] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.ReferentialConstraint] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.RefType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.RelationshipEndMember] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.RelationshipMultiplicity] = new EnumType();
            _builtInTypes[(int)BuiltInTypeKind.RelationshipSet] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.RelationshipType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.RowType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.SimpleType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.StructuralType] = new ComplexType();
            _builtInTypes[(int)BuiltInTypeKind.TypeUsage] = new ComplexType();

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Initialize item attributes for all the built-in complex types
            ////////////////////////////////////////////////////////////////////////////////////////////////

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem),
                EdmConstants.ItemType,
                false /*isAbstract*/,
                null);

            // populate the attributes for item attributes
            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataProperty),
                EdmConstants.ItemAttribute,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem),
                EdmConstants.GlobalItem,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.TypeUsage),
                EdmConstants.TypeUsage,
                false, /*isAbstract*/
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            //populate the attributes for the edm type
            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType),
                EdmConstants.EdmType,
                true, /*isAbstract*/
                (ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType),
                EdmConstants.SimpleType,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EnumType),
                EdmConstants.EnumerationType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.PrimitiveType),
                EdmConstants.PrimitiveType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.SimpleType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.CollectionType),
                EdmConstants.CollectionType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RefType),
                EdmConstants.RefType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember),
                EdmConstants.Member,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmProperty),
                EdmConstants.Property,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.NavigationProperty),
                EdmConstants.NavigationProperty,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.ProviderManifest),
                EdmConstants.ProviderManifest,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipEndMember),
                EdmConstants.RelationshipEnd,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmMember));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationEndMember),
                EdmConstants.AssociationEnd,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipEndMember));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EnumMember),
                EdmConstants.EnumerationMember,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.ReferentialConstraint),
                EdmConstants.ReferentialConstraint,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            // Structural Type hierarchy
            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType),
                EdmConstants.StructuralType,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RowType),
                EdmConstants.RowType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.ComplexType),
                EdmConstants.ComplexType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase),
                EdmConstants.ElementType,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.StructuralType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityType),
                EdmConstants.EntityType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipType),
                EdmConstants.RelationshipType,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityTypeBase));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationType),
                EdmConstants.AssociationType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.Facet),
                EdmConstants.Facet,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntityContainer),
                EdmConstants.EntityContainerType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.GlobalItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase),
                EdmConstants.BaseEntitySetType,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySet),
                EdmConstants.EntitySetType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipSet),
                EdmConstants.RelationshipSet,
                true /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EntitySetBase));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationSet),
                EdmConstants.AssociationSetType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.RelationshipSet));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.AssociationSetEnd),
                EdmConstants.AssociationSetEndType,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.FunctionParameter),
                EdmConstants.FunctionParameter,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmFunction),
                EdmConstants.Function,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.EdmType));

            InitializeBuiltInTypes(
                (ComplexType)GetBuiltInType(BuiltInTypeKind.Documentation),
                EdmConstants.Documentation,
                false /*isAbstract*/,
                (ComplexType)GetBuiltInType(BuiltInTypeKind.MetadataItem));

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Initialize item attributes for all the built-in enum types
            ////////////////////////////////////////////////////////////////////////////////////////////////
            InitializeEnumType(
                BuiltInTypeKind.OperationAction,
                EdmConstants.DeleteAction,
                new[] { EdmConstants.None, EdmConstants.Cascade, EdmConstants.Restrict });

            InitializeEnumType(
                BuiltInTypeKind.RelationshipMultiplicity,
                EdmConstants.RelationshipMultiplicity,
                new[] { EdmConstants.One, EdmConstants.ZeroToOne, EdmConstants.Many });

            InitializeEnumType(
                BuiltInTypeKind.ParameterMode,
                EdmConstants.ParameterMode,
                new[] { EdmConstants.In, EdmConstants.Out, EdmConstants.InOut });

            InitializeEnumType(
                BuiltInTypeKind.CollectionKind,
                EdmConstants.CollectionKind,
                new[] { EdmConstants.NoneCollectionKind, EdmConstants.ListCollectionKind, EdmConstants.BagCollectionKind });

            InitializeEnumType(
                BuiltInTypeKind.PrimitiveTypeKind,
                EdmConstants.PrimitiveTypeKind,
                Enum.GetNames(typeof(PrimitiveTypeKind)));

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Bootstrapping the general facet descriptions
            ////////////////////////////////////////////////////////////////////////////////////////////////

            // Other type non-specific facets
            var generalFacetDescriptions = new FacetDescription[2];

            _nullableFacetDescription = new FacetDescription(
                DbProviderManifest.NullableFacetName,
                EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                null,
                null,
                true);
            generalFacetDescriptions[0] = (_nullableFacetDescription);
            _defaultValueFacetDescription = new FacetDescription(
                DbProviderManifest.DefaultValueFacetName,
                GetBuiltInType(BuiltInTypeKind.EdmType),
                null,
                null,
                null);
            generalFacetDescriptions[1] = (_defaultValueFacetDescription);
            _generalFacetDescriptions = Array.AsReadOnly(generalFacetDescriptions);

            _collectionKindFacetDescription = new FacetDescription(
                XmlConstants.CollectionKind,
                GetBuiltInType(BuiltInTypeKind.EnumType),
                null,
                null,
                null);

            ////////////////////////////////////////////////////////////////////////////////////////////////
            // Add properties for the built-in complex types
            ////////////////////////////////////////////////////////////////////////////////////////////////
            var stringTypeUsage = TypeUsage.Create(EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String));
            var booleanTypeUsage = TypeUsage.Create(EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean));
            var edmTypeUsage = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType));
            var typeUsageTypeUsage = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage));
            var complexTypeUsage = TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.ComplexType));

            // populate the attributes for item attributes
            AddBuiltInTypeProperties(
                BuiltInTypeKind.MetadataProperty,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.TypeUsage, typeUsageTypeUsage),
                        new EdmProperty(EdmConstants.Value, complexTypeUsage)
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.MetadataItem,
                new[]
                    {
                        new EdmProperty(
                            EdmConstants.ItemAttributes,
                            TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.MetadataProperty).GetCollectionType())),
                        new EdmProperty(EdmConstants.Documentation, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.Documentation)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.TypeUsage,
                new[]
                    {
                        new EdmProperty(EdmConstants.EdmType, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType))),
                        new EdmProperty(EdmConstants.Facets, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.Facet)))
                    });

            //populate the attributes for the edm type
            AddBuiltInTypeProperties(
                BuiltInTypeKind.EdmType,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.Namespace, stringTypeUsage),
                        new EdmProperty(EdmConstants.Abstract, booleanTypeUsage),
                        new EdmProperty(EdmConstants.Sealed, booleanTypeUsage),
                        new EdmProperty(EdmConstants.BaseType, complexTypeUsage)
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EnumType,
                new[] { new EdmProperty(EdmConstants.EnumMembers, stringTypeUsage) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.CollectionType,
                new[] { new EdmProperty(EdmConstants.TypeUsage, typeUsageTypeUsage) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.RefType,
                new[] { new EdmProperty(EdmConstants.EntityType, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntityType))) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EdmMember,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.TypeUsage, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EdmProperty,
                new[]
                    {
                        new EdmProperty(EdmConstants.Nullable, stringTypeUsage),
                        new EdmProperty(EdmConstants.DefaultValue, complexTypeUsage)
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.NavigationProperty,
                new[]
                    {
                        new EdmProperty(EdmConstants.RelationshipTypeName, stringTypeUsage),
                        new EdmProperty(EdmConstants.ToEndMemberName, stringTypeUsage)
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.RelationshipEndMember,
                new[]
                    {
                        new EdmProperty(EdmConstants.OperationBehaviors, complexTypeUsage),
                        new EdmProperty(EdmConstants.RelationshipMultiplicity, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EnumType)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EnumMember,
                new[] { new EdmProperty(EdmConstants.Name, stringTypeUsage) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.ReferentialConstraint,
                new[]
                    {
                        new EdmProperty(EdmConstants.ToRole, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.RelationshipEndMember))),
                        new EdmProperty(EdmConstants.FromRole, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.RelationshipEndMember))),
                        new EdmProperty(
                            EdmConstants.ToProperties, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmProperty).GetCollectionType())),
                        new EdmProperty(
                            EdmConstants.FromProperties, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmProperty).GetCollectionType()))
                    });

            // Structural Type hierarchy
            AddBuiltInTypeProperties(
                BuiltInTypeKind.StructuralType,
                new[] { new EdmProperty(EdmConstants.Members, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmMember))) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EntityTypeBase,
                new[] { new EdmProperty(EdmConstants.KeyMembers, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmMember))) });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.Facet,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.EdmType, edmTypeUsage),
                        new EdmProperty(EdmConstants.Value, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EdmType)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EntityContainer,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.EntitySets, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntitySet)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EntitySetBase,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.EntityType, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntityType))),
                        new EdmProperty(EdmConstants.Schema, stringTypeUsage),
                        new EdmProperty(EdmConstants.Table, stringTypeUsage)
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.AssociationSet,
                new[]
                    {
                        new EdmProperty(
                            EdmConstants.AssociationSetEnds,
                            TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.AssociationSetEnd).GetCollectionType()))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.AssociationSetEnd,
                new[]
                    {
                        new EdmProperty(EdmConstants.Role, stringTypeUsage),
                        new EdmProperty(EdmConstants.EntitySetType, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EntitySet)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.FunctionParameter,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.Mode, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.EnumType))),
                        new EdmProperty(EdmConstants.TypeUsage, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.TypeUsage)))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.EdmFunction,
                new[]
                    {
                        new EdmProperty(EdmConstants.Name, stringTypeUsage),
                        new EdmProperty(EdmConstants.Namespace, stringTypeUsage),
                        new EdmProperty(EdmConstants.ReturnParameter, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.FunctionParameter))),
                        new EdmProperty(
                            EdmConstants.Parameters, TypeUsage.Create(GetBuiltInType(BuiltInTypeKind.FunctionParameter).GetCollectionType()))
                    });

            AddBuiltInTypeProperties(
                BuiltInTypeKind.Documentation,
                new[]
                    {
                        new EdmProperty(EdmConstants.Summary, stringTypeUsage),
                        new EdmProperty(EdmConstants.LongDescription, stringTypeUsage)
                    });

            // Set all types to be readonly, used SetReadOnly to skip validation method to 
            for (var i = 0; i < _builtInTypes.Length; i++)
            {
                _builtInTypes[i].SetReadOnly();
            }
        }

        private static readonly EdmType[] _builtInTypes = new EdmType[EdmConstants.NumBuiltInTypes];
        private static readonly ReadOnlyCollection<FacetDescription> _generalFacetDescriptions;
        private static readonly FacetDescription _nullableFacetDescription;
        private static readonly FacetDescription _defaultValueFacetDescription;
        private static readonly FacetDescription _collectionKindFacetDescription;

        internal static FacetDescription DefaultValueFacetDescription
        {
            get { return _defaultValueFacetDescription; }
        }

        internal static FacetDescription CollectionKindFacetDescription
        {
            get { return _collectionKindFacetDescription; }
        }

        internal static FacetDescription NullableFacetDescription
        {
            get { return _nullableFacetDescription; }
        }

        internal static EdmProviderManifest EdmProviderManifest
        {
            get { return EdmProviderManifest.Instance; }
        }

        /// <summary>
        ///     Returns the list of EDM builtin types
        /// </summary>
        public static EdmType GetBuiltInType(BuiltInTypeKind builtInTypeKind)
        {
            return _builtInTypes[(int)builtInTypeKind];
        }

        /// <summary>
        ///     Returns the list of facet descriptions for a given type
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ReadOnlyCollection<FacetDescription> GetGeneralFacetDescriptions()
        {
            return _generalFacetDescriptions;
        }

        /// <summary>
        ///     Initialize all the build in type with the given type attributes and properties
        /// </summary>
        /// <param name="builtInType"> The built In type which is getting initialized </param>
        /// <param name="name"> name of the built in type </param>
        /// <param name="isAbstract"> whether the type is abstract or not </param>
        /// <param name="isSealed"> whether the type is sealed or not </param>
        /// <param name="baseType"> The base type of the built in type </param>
        private static void InitializeBuiltInTypes(
            ComplexType builtInType,
            string name,
            bool isAbstract,
            ComplexType baseType)
        {
            // Initialize item attributes for all ancestor types
            EdmType.Initialize(builtInType, name, EdmConstants.EdmNamespace, DataSpace.CSpace, isAbstract, baseType);
        }

        /// <summary>
        ///     Add properties for all the build in complex type
        /// </summary>
        /// <param name="builtInTypeKind"> The type of the built In type whose properties are being added </param>
        /// <param name="properties"> properties of the built in type </param>
        private static void AddBuiltInTypeProperties(BuiltInTypeKind builtInTypeKind, EdmProperty[] properties)
        {
            var complexType = (ComplexType)GetBuiltInType(builtInTypeKind);
            if (properties != null)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    complexType.AddMember(properties[i]);
                }
            }
        }

        /// <summary>
        ///     Initializes the enum type
        /// </summary>
        /// <param name="builtInTypeKind"> The built-in type kind enum value of this enum type </param>
        /// <param name="name"> The name of this enum type </param>
        /// <param name="enumMemberNames"> The member names of this enum type </param>
        private static void InitializeEnumType(
            BuiltInTypeKind builtInTypeKind,
            string name,
            string[] enumMemberNames)
        {
            var enumType = (EnumType)GetBuiltInType(builtInTypeKind);

            // Initialize item attributes for all ancestor types
            EdmType.Initialize(
                enumType,
                name,
                EdmConstants.EdmNamespace,
                DataSpace.CSpace,
                false,
                null);

            for (var i = 0; i < enumMemberNames.Length; i++)
            {
                enumType.AddMember(new EnumMember(enumMemberNames[i], i));
            }
        }
    }
}
