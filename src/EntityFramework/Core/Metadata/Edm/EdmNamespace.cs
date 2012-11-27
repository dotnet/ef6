// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of a namespace in an <see cref="EdmModel" /> .
    /// </summary>
    public class EdmNamespace : MetadataItem, IQualifiedNameMetadataItem
    {
        private readonly List<AssociationType> associationTypesList = new List<AssociationType>();
        private readonly List<ComplexType> complexTypesList = new List<ComplexType>();
        private readonly List<EntityType> entityTypesList = new List<EntityType>();
        private readonly List<EnumType> enumTypesList = new List<EnumType>();

        /// <summary>
        ///     Gets all <see cref="EdmType" /> s declared within the namspace. Includes <see cref="AssociationType" /> s,
        ///     <see
        ///         cref="ComplexType" />
        ///     s, <see cref="EntityType" /> s.
        /// </summary>
        public IEnumerable<EdmType> NamespaceItems
        {
            get
            {
                return associationTypesList
                    .Concat<EdmType>(complexTypesList)
                    .Concat(entityTypesList)
                    .Concat(enumTypesList);
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="AssociationType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<AssociationType> AssociationTypes
        {
            get { return associationTypesList; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="ComplexType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<ComplexType> ComplexTypes
        {
            get { return complexTypesList; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="EntityType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EntityType> EntityTypes
        {
            get { return entityTypesList; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="EnumType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EnumType> EnumTypes
        {
            get { return enumTypesList; }
        }

        public virtual string Name { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { throw new NotImplementedException(); }
        }

        internal override string Identity
        {
            get { throw new NotImplementedException(); }
        }
    }
}
