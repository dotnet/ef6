// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    ///     Represent the edm property class
    /// </summary>
    public sealed class EdmProperty : EdmMember
    {
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty Primitive(string name, PrimitiveType primitiveType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(primitiveType != null);

            return CreateProperty(name, primitiveType);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty Enum(string name, EnumType enumType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(enumType != null);

            return CreateProperty(name, enumType);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static EdmProperty Complex(string name, ComplexType complexType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(complexType != null);

            var property = CreateProperty(name, complexType);

            property.Nullable = false;

            return property;
        }

        private static EdmProperty CreateProperty(string name, EdmType edmType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(edmType != null);

            var typeUsage = TypeUsage.Create(edmType, new FacetValues());

            var property = new EdmProperty(name, typeUsage);

            return property;
        }

        /// <summary>
        ///     Initializes a new instance of the property class
        /// </summary>
        /// <param name="name"> name of the property </param>
        /// <param name="typeUsage"> TypeUsage object containing the property type and its facets </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal EdmProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            EntityUtil.CheckStringArgument(name, "name");
            EntityUtil.GenericCheckArgumentNull(typeUsage, "typeUsage");
        }

        /// <summary>
        ///     Initializes a new OSpace instance of the property class
        /// </summary>
        /// <param name="name"> name of the property </param>
        /// <param name="typeUsage"> TypeUsage object containing the property type and its facets </param>
        /// <param name="propertyInfo"> for the property </param>
        /// <param name="entityDeclaringType"> The declaring type of the entity containing the property </param>
        internal EdmProperty(string name, TypeUsage typeUsage, PropertyInfo propertyInfo, RuntimeTypeHandle entityDeclaringType)
            : this(name, typeUsage)
        {
            if (propertyInfo != null)
            {
                Debug.Assert(name == propertyInfo.Name, "different PropertyName");

                var method = propertyInfo.GetGetMethod(true);
                PropertyGetterHandle = ((null != method) ? method.MethodHandle : default(RuntimeMethodHandle));

                method = propertyInfo.GetSetMethod(true); // return public or non-public getter
                PropertySetterHandle = ((null != method) ? method.MethodHandle : default(RuntimeMethodHandle));

                EntityDeclaringType = entityDeclaringType;
            }
        }

        /// <summary>
        ///     Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd
        /// </summary>
        internal readonly RuntimeMethodHandle PropertyGetterHandle;

        /// <summary>
        ///     Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd
        /// </summary>
        internal readonly RuntimeMethodHandle PropertySetterHandle;

        /// <summary>
        ///     Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd
        /// </summary>
        internal readonly RuntimeTypeHandle EntityDeclaringType;

        /// <summary>
        ///     cached dynamic method to get the property value from a CLR instance
        /// </summary>
        private Func<object, object> _memberGetter;

        /// <summary>
        ///     cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        private Action<object, object> _memberSetter;

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EdmProperty; }
        }

        /// <summary>
        ///     Returns true if this property is nullable.
        /// </summary>
        /// <remarks>
        ///     Nullability in the conceptual model and store model is a simple indication of whether or not
        ///     the property is considered nullable. Nullability in the object model is more complex.
        ///     When using convention based mapping (as usually happens with POCO entities), a property in the
        ///     object model is considered nullable if and only if the underlying CLR type is nullable and
        ///     the property is not part of the primary key.
        ///     When using attribute based mapping (usually used with entities that derive from the EntityObject
        ///     base class), a property is considered nullable if the IsNullable flag is set to true in the
        ///     <see cref="System.Data.Entity.Core.Objects.DataClasses.EdmScalarPropertyAttribute" /> attribute. This flag can
        ///     be set to true even if the underlying type is not nullable, and can be set to false even if the
        ///     underlying type is nullable. The latter case happens as part of default code generation when
        ///     a non-nullable property in the conceptual model is mapped to a nullable CLR type such as a string.
        ///     In such a case, the Entity Framework treats the property as non-nullable even though the CLR would
        ///     allow null to be set.
        ///     There is no good reason to set a non-nullable CLR type as nullable in the object model and this
        ///     should not be done even though the attribute allows it.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public bool Nullable
        {
            get { return (bool)TypeUsage.Facets[DbProviderManifest.NullableFacetName].Value; }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(NullableFacetDescription, value));
            }
        }

        /// <summary>
        ///     Returns the default value for this property
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public Object DefaultValue
        {
            get { return TypeUsage.Facets[DbProviderManifest.DefaultValueFacetName].Value; }
        }

        /// <summary>
        ///     cached dynamic method to get the property value from a CLR instance
        /// </summary>
        internal Func<object, object> ValueGetter
        {
            get { return _memberGetter; }
            set
            {
                Debug.Assert(null != value, "clearing ValueGetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberGetter, value, null);
            }
        }

        /// <summary>
        ///     cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        internal Action<object, object> ValueSetter
        {
            get { return _memberSetter; }
            set
            {
                Debug.Assert(null != value, "clearing ValueSetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberSetter, value, null);
            }
        }

        public bool IsCollectionType
        {
            get { return TypeUsage.EdmType is CollectionType; }
        }

        public bool IsComplexType
        {
            get { return TypeUsage.EdmType is ComplexType; }
        }

        public bool IsPrimitiveType
        {
            get { return TypeUsage.EdmType is PrimitiveType; }
        }

        public bool IsEnumType
        {
            get { return TypeUsage.EdmType is EnumType; }
        }

        public bool IsUnderlyingPrimitiveType
        {
            get { return IsPrimitiveType || IsEnumType; }
        }

        public ComplexType ComplexType
        {
            get { return TypeUsage.EdmType as ComplexType; }
        }

        public PrimitiveType PrimitiveType
        {
            get { return TypeUsage.EdmType as PrimitiveType; }
        }

        public EnumType EnumType
        {
            get { return TypeUsage.EdmType as EnumType; }
        }

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

        public ConcurrencyMode ConcurrencyMode
        {
            get { return MetadataHelper.GetConcurrencyMode(TypeUsage); }
            set
            {
                Util.ThrowIfReadOnly(this);

                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(Converter.ConcurrencyModeFacet, value));
            }
        }

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

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            MaxLength = value
                        });
            }
        }

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

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            FixedLength = value
                        });
            }
        }

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

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            Unicode = value
                        });
            }
        }

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

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            Precision = value
                        });
            }
        }

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

                TypeUsage = TypeUsage.ShallowCopy(
                    new FacetValues
                        {
                            Scale = value
                        });
            }
        }
    }
}
