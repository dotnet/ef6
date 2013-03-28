// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    ///     Class representing a ref type
    /// </summary>
    public class RefType : EdmType
    {
        internal RefType()
        {
        }

        /// <summary>
        ///     The constructor for constructing a RefType object with the entity type it references
        /// </summary>
        /// <param name="entityType"> The entity type that this ref type references </param>
        /// <exception cref="System.ArgumentNullException">Thrown if entityType argument is null</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal RefType(EntityType entityType)
            : base(GetIdentity(Check.NotNull(entityType, "entityType")),
                EdmConstants.TransientNamespace, entityType.DataSpace)
        {
            _elementType = entityType;
            SetReadOnly();
        }

        private readonly EntityTypeBase _elementType;

        /// <summary>
        ///     Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" />
        ///     .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.RefType; }
        }

        /// <summary>
        ///     Gets the entity type referenced by this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityTypeBase" /> object that represents the entity type referenced by this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" />
        ///     .
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EntityTypeBase, false)]
        public virtual EntityTypeBase ElementType
        {
            get { return _elementType; }
        }

        /// <summary>
        ///     Constructs the name of the collection type
        /// </summary>
        /// <param name="entityTypeBase"> The entity type base that this ref type refers to </param>
        /// <returns> The identity of the resulting ref type </returns>
        private static string GetIdentity(EntityTypeBase entityTypeBase)
        {
            var builder = new StringBuilder(50);
            builder.Append("reference[");
            entityTypeBase.BuildIdentity(builder);
            builder.Append("]");
            return builder.ToString();
        }
    }
}
