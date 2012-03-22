namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using System.Data.Metadata.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Contains metadata for a property of a complex object or entity.
    /// </summary>
    internal class PropertyEntryMetadata : MemberEntryMetadata
    {
        #region Fields, constructors, and factory methods

        private readonly bool _isMapped;
        private readonly bool _isComplex;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "PropertyEntryMetadata" /> class.
        /// </summary>
        /// <param name = "declaringType">The type that the property is declared on.</param>
        /// <param name = "propertyType">Type of the property.</param>
        /// <param name = "propertyName">The property name.</param>
        /// <param name = "isMapped">if set to <c>true</c> the property is mapped in the EDM.</param>
        /// <param name = "isComplex">if set to <c>true</c> the property is a complex property.</param>
        public PropertyEntryMetadata(
            Type declaringType, Type propertyType, string propertyName, bool isMapped, bool isComplex)
            : base(declaringType, propertyType, propertyName)
        {
            _isMapped = isMapped;
            _isComplex = isComplex;
        }

        /// <summary>
        ///     Validates that the given name is a property of the declaring type (either on the CLR type or in the EDM)
        ///     and that it is a complex or scalar property rather than a nav property and then returns metadata about
        ///     the property.
        /// </summary>
        /// <param name = "internalContext">The internal context.</param>
        /// <param name = "declaringType">The type that the property is declared on.</param>
        /// <param name = "requestedType">The type of property requested, which may be 'object' if any type can be accepted.</param>
        /// <param name = "propertyName">Name of the property.</param>
        /// <returns>Metadata about the property, or null if the property does not exist or is a navigation property.</returns>
        public static PropertyEntryMetadata ValidateNameAndGetMetadata(
            InternalContext internalContext, Type declaringType, Type requestedType, string propertyName)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(declaringType != null);
            Contract.Requires(requestedType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            Type propertyType;
            DbHelpers.GetPropertyTypes(declaringType).TryGetValue(propertyName, out propertyType);

            var metadataWorkspace = internalContext.ObjectContext.MetadataWorkspace;
            var edmType = metadataWorkspace.GetItem<StructuralType>(declaringType.FullName, DataSpace.OSpace);

            var isMapped = false;
            var isComplex = false;

            EdmMember member;
            edmType.Members.TryGetValue(propertyName, false, out member);
            if (member != null)
            {
                // If the property is in the model, then it must be a scalar or complex property, not a nav prop
                var edmProperty = member as EdmProperty;
                if (edmProperty == null)
                {
                    return null;
                }

                if (propertyType == null)
                {
                    var asPrimitive = edmProperty.TypeUsage.EdmType as PrimitiveType;
                    if (asPrimitive != null)
                    {
                        propertyType = asPrimitive.ClrEquivalentType;
                    }
                    else
                    {
                        Contract.Assert(
                            edmProperty.TypeUsage.EdmType is StructuralType,
                            "Expected a structural type if property type is not primitive.");

                        var objectItemCollection =
                            (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);
                        propertyType = objectItemCollection.GetClrType((StructuralType)edmProperty.TypeUsage.EdmType);
                    }
                }

                isMapped = true;
                isComplex = edmProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType;
            }
            else
            {
                // If the prop is not in the model, then it must have a getter or a setter
                var propertyGetters = DbHelpers.GetPropertyGetters(declaringType);
                var propertySetters = DbHelpers.GetPropertySetters(declaringType);
                if (!(propertyGetters.ContainsKey(propertyName) || propertySetters.ContainsKey(propertyName)))
                {
                    return null;
                }

                Contract.Assert(
                    propertyType != null, "If the property has a getter or setter, then it must exist and have a type.");
            }

            if (!requestedType.IsAssignableFrom(propertyType))
            {
                throw Error.DbEntityEntry_WrongGenericForProp(
                    propertyName, declaringType.Name, requestedType.Name, propertyType.Name);
            }

            return new PropertyEntryMetadata(declaringType, propertyType, propertyName, isMapped, isComplex);
        }

        #endregion

        #region Entry factory methods

        /// <summary>
        ///     Creates a new <see cref = "InternalMemberEntry" /> the runtime type of which will be
        ///     determined by the metadata.
        /// </summary>
        /// <param name = "internalEntityEntry">The entity entry to which the member belongs.</param>
        /// <param name = "parentPropertyEntry">The parent property entry if the new entry is nested, otherwise null.</param>
        /// <returns>The new entry.</returns>
        public override InternalMemberEntry CreateMemberEntry(
            InternalEntityEntry internalEntityEntry, InternalPropertyEntry parentPropertyEntry)
        {
            return parentPropertyEntry == null
                       ? (InternalMemberEntry)new InternalEntityPropertyEntry(internalEntityEntry, this)
                       : new InternalNestedPropertyEntry(parentPropertyEntry, this);
        }

        #endregion

        #region Metadata access

        /// <summary>
        ///     Gets a value indicating whether this is a complex property.
        ///     That is, not whether or not this is a property on a complex object, but rather if the
        ///     property itself is a complex property.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is complex; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplex
        {
            get { return _isComplex; }
        }

        /// <summary>
        ///     Gets the type of the member for which this is metadata.
        /// </summary>
        /// <value>The type of the member entry.</value>
        public override MemberEntryType MemberEntryType
        {
            get { return _isComplex ? MemberEntryType.ComplexProperty : MemberEntryType.ScalarProperty; }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is mapped in the EDM.
        /// </summary>
        /// <value><c>true</c> if this instance is mapped; otherwise, <c>false</c>.</value>
        public bool IsMapped
        {
            get { return _isMapped; }
        }

        /// <summary>
        ///     Gets the type of the member, which for collection properties is the type
        ///     of the collection rather than the type in the collection.
        /// </summary>
        /// <value>The type of the member.</value>
        public override Type MemberType
        {
            get { return ElementType; }
        }

        #endregion
    }
}
