// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// In conceptual-space, EdmProperty represents a property on an Entity.
    /// In store-space, EdmProperty represents a column in a table.
    /// </summary>
    public class EdmProperty : EdmMember
    {
        /// <summary> Creates a new primitive property. </summary>
        /// <returns> The newly created property. </returns>
        /// <param name="name"> The name of the property. </param>
        /// <param name="primitiveType"> The type of the property. </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty CreatePrimitive(string name, PrimitiveType primitiveType)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(primitiveType, "primitiveType");

            return CreateProperty(name, primitiveType);
        }

        /// <summary> Creates a new enum property. </summary>
        /// <returns> The newly created property. </returns>
        /// <param name="name"> The name of the property. </param>
        /// <param name="enumType"> The type of the property. </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty CreateEnum(string name, EnumType enumType)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(enumType, "enumType");

            return CreateProperty(name, enumType);
        }

        /// <summary> Creates a new complex property. </summary>
        /// <returns> The newly created property. </returns>
        /// <param name="name"> The name of the property. </param>
        /// <param name="complexType"> The type of the property. </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty CreateComplex(string name, ComplexType complexType)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(complexType, "complexType");

            var property = CreateProperty(name, complexType);

            property.Nullable = false;

            return property;
        }

        /// <summary>
        /// Creates a new instance of EdmProperty type.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="typeUsage">
        /// Property <see cref="TypeUsage" />
        /// </param>
        /// <returns>A new instance of EdmProperty type</returns>
        public static EdmProperty Create(string name, TypeUsage typeUsage)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");

            var edmType = typeUsage.EdmType;
            if (!(Helper.IsPrimitiveType(edmType)
                  || Helper.IsEnumType(edmType)
                  || Helper.IsComplexType(edmType)))
            {
                throw new ArgumentException(Strings.EdmProperty_InvalidPropertyType(edmType.FullName));
            }

            return new EdmProperty(name, typeUsage);
        }

        private static EdmProperty CreateProperty(string name, EdmType edmType)
        {
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(edmType);

            var typeUsage = TypeUsage.Create(edmType, new FacetValues());

            var property = new EdmProperty(name, typeUsage);

            return property;
        }

        /// <summary>
        /// Initializes a new instance of the property class
        /// </summary>
        /// <param name="name"> name of the property </param>
        /// <param name="typeUsage"> TypeUsage object containing the property type and its facets </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal EdmProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");
        }

        /// <summary>
        /// Initializes a new OSpace instance of the property class
        /// </summary>
        /// <param name="name"> name of the property </param>
        /// <param name="typeUsage"> TypeUsage object containing the property type and its facets </param>
        /// <param name="propertyInfo"> for the property </param>
        /// <param name="entityDeclaringType"> The declaring type of the entity containing the property </param>
        internal EdmProperty(string name, TypeUsage typeUsage, PropertyInfo propertyInfo, Type entityDeclaringType)
            : this(name, typeUsage)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(entityDeclaringType);
            Debug.Assert(name == propertyInfo.Name);

            _propertyInfo = propertyInfo;
            _entityDeclaringType = entityDeclaringType;
        }

        internal EdmProperty(string name)
            : this(name, TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))
        {
            // testing only
        }

        private readonly PropertyInfo _propertyInfo;

        private readonly Type _entityDeclaringType;

        internal PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        internal Type EntityDeclaringType
        {
            get { return _entityDeclaringType; }
        }

        /// <summary>
        /// cached dynamic method to get the property value from a CLR instance
        /// </summary>
        private Func<object, object> _memberGetter;

        /// <summary>
        /// cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        private Action<object, object> _memberSetter;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EdmProperty; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" /> can have a null value.
        /// </summary>
        /// <remarks>
        /// Nullability in the conceptual model and store model is a simple indication of whether or not
        /// the property is considered nullable. Nullability in the object model is more complex.
        /// When using convention based mapping (as usually happens with POCO entities), a property in the
        /// object model is considered nullable if and only if the underlying CLR type is nullable and
        /// the property is not part of the primary key.
        /// When using attribute based mapping (usually used with entities that derive from the EntityObject
        /// base class), a property is considered nullable if the IsNullable flag is set to true in the
        /// <see cref="System.Data.Entity.Core.Objects.DataClasses.EdmScalarPropertyAttribute" /> attribute. This flag can
        /// be set to true even if the underlying type is not nullable, and can be set to false even if the
        /// underlying type is nullable. The latter case happens as part of default code generation when
        /// a non-nullable property in the conceptual model is mapped to a nullable CLR type such as a string.
        /// In such a case, the Entity Framework treats the property as non-nullable even though the CLR would
        /// allow null to be set.
        /// There is no good reason to set a non-nullable CLR type as nullable in the object model and this
        /// should not be done even though the attribute allows it.
        /// </remarks>
        /// <returns>
        /// true if this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" /> can have a null value; otherwise, false.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public bool Nullable
        {
            get { return (bool)TypeUsage.Facets[DbProviderManifest.NullableFacetName].Value; }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            Nullable = value
                        });
            }
        }

        /// <summary>Gets the type name of the property.</summary>
        /// <returns>The type name of the property.</returns>
        public string TypeName
        {
            get { return TypeUsage.EdmType.Name; }
        }

        /// <summary>
        /// Gets the default value for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" />.
        /// </summary>
        /// <returns>
        /// The default value for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmProperty" />.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public Object DefaultValue
        {
            get { return TypeUsage.Facets[DbProviderManifest.DefaultValueFacetName].Value; }
        }

        /// <summary>
        /// cached dynamic method to get the property value from a CLR instance
        /// </summary>
        internal Func<object, object> ValueGetter
        {
            get { return _memberGetter; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberGetter, value, null);
            }
        }

        /// <summary>
        /// cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        internal Action<object, object> ValueSetter
        {
            get { return _memberSetter; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberSetter, value, null);
            }
        }

        internal bool IsKeyMember
        {
            get
            {
                var parentEntityType = DeclaringType as EntityType;

                return (parentEntityType != null) && parentEntityType.KeyMembers.Contains(this);
            }
        }

        /// <summary>Gets whether the property is a collection type property.</summary>
        /// <returns>true if the property is a collection type property; otherwise, false.</returns>
        public bool IsCollectionType
        {
            get { return TypeUsage.EdmType is CollectionType; }
        }

        /// <summary>Gets whether this property is a complex type property.</summary>
        /// <returns>true if this property is a complex type property; otherwise, false.</returns>
        public bool IsComplexType
        {
            get { return TypeUsage.EdmType is ComplexType; }
        }

        /// <summary>Gets whether this property is a primitive type.</summary>
        /// <returns>true if this property is a primitive type; otherwise, false.</returns>
        public bool IsPrimitiveType
        {
            get { return TypeUsage.EdmType is PrimitiveType; }
        }

        /// <summary>Gets whether this property is an enumeration type property.</summary>
        /// <returns>true if this property is an enumeration type property; otherwise, false.</returns>
        public bool IsEnumType
        {
            get { return TypeUsage.EdmType is EnumType; }
        }

        /// <summary>Gets whether this property is an underlying primitive type.</summary>
        /// <returns>true if this property is an underlying primitive type; otherwise, false.</returns>
        public bool IsUnderlyingPrimitiveType
        {
            get { return IsPrimitiveType || IsEnumType; }
        }

        /// <summary>Gets the complex type information for this property.</summary>
        /// <returns>The complex type information for this property.</returns>
        public ComplexType ComplexType
        {
            get { return TypeUsage.EdmType as ComplexType; }
        }

        /// <summary>Gets the primitive type information for this property.</summary>
        /// <returns>The primitive type information for this property.</returns>
        public PrimitiveType PrimitiveType
        {
            get { return TypeUsage.EdmType as PrimitiveType; }
            internal set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                var existingStoreGeneratedPattern = StoreGeneratedPattern;
                var existingConcurrencyMode = ConcurrencyMode;

                var relevantExistingFacets = new List<Facet>();

                foreach (var facetDescription in value.GetAssociatedFacetDescriptions())
                {
                    Facet facet;
                    if (TypeUsage.Facets.TryGetValue(facetDescription.FacetName, false, out facet)
                        && ((facet.Value == null && facet.Description.DefaultValue != null)
                            || (facet.Value != null && !facet.Value.Equals(facet.Description.DefaultValue))))
                    {
                        relevantExistingFacets.Add(facet);
                    }
                }

                TypeUsage = TypeUsage.Create(value, FacetValues.Create(relevantExistingFacets));

                if (existingStoreGeneratedPattern != StoreGeneratedPattern.None)
                {
                    StoreGeneratedPattern = existingStoreGeneratedPattern;
                }

                if (existingConcurrencyMode != ConcurrencyMode.None)
                {
                    ConcurrencyMode = existingConcurrencyMode;
                }
            }
        }

        /// <summary>Gets the enumeration type information for this property.</summary>
        /// <returns>The enumeration type information for this property.</returns>
        public EnumType EnumType
        {
            get { return TypeUsage.EdmType as EnumType; }
        }

        /// <summary>Gets the underlying primitive type information for this property.</summary>
        /// <returns>The underlying primitive type information for this property.</returns>
        public PrimitiveType UnderlyingPrimitiveType
        {
            get
            {
                if (!IsUnderlyingPrimitiveType)
                {
                    return null;
                }

                return IsEnumType
                           ? EnumType.UnderlyingType
                           : PrimitiveType;
            }
        }

        /// <summary>Gets or sets the concurrency mode for the property.</summary>
        /// <returns>The concurrency mode for the property.</returns>
        public ConcurrencyMode ConcurrencyMode
        {
            get { return MetadataHelper.GetConcurrencyMode(this); }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(Converter.ConcurrencyModeFacet, value));
            }
        }

        /// <summary>Gets or sets the database generation method for the database column associated with this property</summary>
        /// <returns>The store generated pattern for the property.</returns>
        public StoreGeneratedPattern StoreGeneratedPattern
        {
            get { return MetadataHelper.GetStoreGeneratedPattern(this); }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(Converter.StoreGeneratedPatternFacet, value));
            }
        }

        /// <summary>Gets or sets the kind of collection for this model.</summary>
        /// <returns>The kind of collection for this model.</returns>
        public CollectionKind CollectionKind
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(EdmConstants.CollectionKind, false, out facet)
                           ? (CollectionKind)facet.Value
                           : CollectionKind.None;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(CollectionKindFacetDescription, value));
            }
        }

        /// <summary>Gets whether the maximum length facet is constant for the database provider.</summary>
        /// <returns>true if the facet is constant; otherwise, false.</returns>
        public bool IsMaxLengthConstant
        {
            get
            {
                Facet facet;
                return
                    TypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, false, out facet)
                    && facet.Description.IsConstant;
            }
        }

        /// <summary>Gets or sets the maximum length of the property.</summary>
        /// <returns>The maximum length of the property.</returns>
        public int? MaxLength
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, false, out facet)
                           ? facet.Value as int?
                           : null;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (MaxLength != value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                MaxLength = value
                            });
                }
            }
        }

        /// <summary>Gets or sets whether this property uses the maximum length supported by the provider.</summary>
        /// <returns>true if this property uses the maximum length supported by the provider; otherwise, false.</returns>
        public bool IsMaxLength
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, false, out facet)
                       && facet.IsUnbounded;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                MaxLength = EdmConstants.UnboundedValue
                            });
                }
            }
        }

        /// <summary>Gets whether the fixed length facet is constant for the database provider.</summary>
        /// <returns>true if the facet is constant; otherwise, false.</returns>
        public bool IsFixedLengthConstant
        {
            get
            {
                Facet facet;
                return
                    TypeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, false, out facet)
                    && facet.Description.IsConstant;
            }
        }

        /// <summary>Gets or sets whether the length of this property is fixed.</summary>
        /// <returns>true if the length of this property is fixed; otherwise, false.</returns>
        public bool? IsFixedLength
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, false, out facet)
                           ? facet.Value as bool?
                           : null;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (IsFixedLength != value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                FixedLength = value
                            });
                }
            }
        }

        /// <summary>Gets whether the Unicode facet is constant for the database provider.</summary>
        /// <returns>true if the facet is constant; otherwise, false.</returns>
        public bool IsUnicodeConstant
        {
            get
            {
                Facet facet;
                return
                    TypeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, false, out facet)
                    && facet.Description.IsConstant;
            }
        }

        /// <summary>Gets or sets whether this property is a Unicode property.</summary>
        /// <returns>true if this property is a Unicode property; otherwise, false.</returns>
        public bool? IsUnicode
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, false, out facet)
                           ? facet.Value as bool?
                           : null;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (IsUnicode != value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                Unicode = value
                            });
                }
            }
        }

        /// <summary>Gets whether the precision facet is constant for the database provider.</summary>
        /// <returns>true if the facet is constant; otherwise, false.</returns>
        public bool IsPrecisionConstant
        {
            get
            {
                Facet facet;
                return
                    TypeUsage.Facets.TryGetValue(DbProviderManifest.PrecisionFacetName, false, out facet)
                    && facet.Description.IsConstant;
            }
        }

        /// <summary>Gets or sets the precision of this property.</summary>
        /// <returns>The precision of this property.</returns>
        public byte? Precision
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.PrecisionFacetName, false, out facet)
                           ? facet.Value as byte?
                           : null;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (Precision != value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                Precision = value
                            });
                }
            }
        }

        /// <summary>Gets whether the scale facet is constant for the database provider.</summary>
        /// <returns>true if the facet is constant; otherwise, false.</returns>
        public bool IsScaleConstant
        {
            get
            {
                Facet facet;
                return
                    TypeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName, false, out facet)
                    && facet.Description.IsConstant;
            }
        }

        /// <summary>Gets or sets the scale of this property.</summary>
        /// <returns>The scale of this property.</returns>
        public byte? Scale
        {
            get
            {
                Facet facet;
                return TypeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName, false, out facet)
                           ? facet.Value as byte?
                           : null;
            }
            set
            {
                Util.ThrowIfReadOnly(this);

                if (Scale != value)
                {
                    TypeUsage = TypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                Scale = value
                            });
                }
            }
        }

        /// <summary>Sets the metadata properties.</summary>
        /// <param name="metadataProperties">The metadata properties to be set.</param>
        public void SetMetadataProperties(IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotNull(metadataProperties, "metadataProperties");

            Util.ThrowIfReadOnly(this);
            AddMetadataProperties(metadataProperties.ToList());
        }
    }
}
