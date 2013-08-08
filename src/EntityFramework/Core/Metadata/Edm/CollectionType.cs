// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents the Edm Collection Type
    /// </summary>
    public class CollectionType : EdmType
    {
        // For testing only
        internal CollectionType()
        {
        }

        /// <summary>
        /// The constructor for constructing a CollectionType object with the element type it contains
        /// </summary>
        /// <param name="elementType"> The element type that this collection type contains </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument elementType is null</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal CollectionType(EdmType elementType)
            : this(TypeUsage.Create(elementType))
        {
            DataSpace = elementType.DataSpace;
        }

        /// <summary>
        /// The constructor for constructing a CollectionType object with the element type (as a TypeUsage) it contains
        /// </summary>
        /// <param name="elementType"> The element type that this collection type contains </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument elementType is null</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal CollectionType(TypeUsage elementType)
            : base(GetIdentity(Check.NotNull(elementType, "elementType")),
                EdmConstants.TransientNamespace, elementType.EdmType.DataSpace)
        {
            _typeUsage = elementType;
            SetReadOnly();
        }

        private readonly TypeUsage _typeUsage;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.CollectionType" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.CollectionType" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.CollectionType; }
        }

        /// <summary>
        /// Gets the instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> class that contains the type of the element that this current
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.CollectionType" />
        /// object includes and facets for that type.
        /// </summary>
        /// <returns>
        /// The instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> class that contains the type of the element that this current
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.CollectionType" />
        /// object includes and facets for that type.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
        public virtual TypeUsage TypeUsage
        {
            get { return _typeUsage; }
        }

        /// <summary>
        /// Constructs the name of the collection type
        /// </summary>
        /// <param name="typeUsage"> The typeusage for the element type that this collection type refers to </param>
        /// <returns> The identity of the resulting collection type </returns>
        private static string GetIdentity(TypeUsage typeUsage)
        {
            var builder = new StringBuilder(50);
            builder.Append("collection[");
            typeUsage.BuildIdentity(builder);
            builder.Append("]");
            return builder.ToString();
        }

        /// <summary>
        /// Override EdmEquals to support value comparison of TypeUsage property
        /// </summary>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (ReferenceEquals(this, item))
            {
                return true;
            }

            // check type of item
            if (null == item
                || BuiltInTypeKind.CollectionType != item.BuiltInTypeKind)
            {
                return false;
            }
            var other = (CollectionType)item;

            // compare type usage
            return TypeUsage.EdmEquals(other.TypeUsage);
        }
    }
}
