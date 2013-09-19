// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Diagnostics;

    internal class NavigationEntryMetadata : MemberEntryMetadata
    {
        #region Fields and constructors

        private readonly bool _isCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationEntryMetadata" /> class.
        /// </summary>
        /// <param name="declaringType"> The type that the property is declared on. </param>
        /// <param name="propertyType"> Type of the property. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <param name="isCollection">
        /// if set to <c>true</c> this is a collection nav prop.
        /// </param>
        public NavigationEntryMetadata(Type declaringType, Type propertyType, string propertyName, bool isCollection)
            : base(declaringType, propertyType, propertyName)
        {
            _isCollection = isCollection;
        }

        #endregion

        #region Metadata access

        /// <summary>
        /// Gets the type of the member for which this is metadata.
        /// </summary>
        /// <value> The type of the member entry. </value>
        public override MemberEntryType MemberEntryType
        {
            get
            {
                return _isCollection
                           ? MemberEntryType.CollectionNavigationProperty
                           : MemberEntryType.ReferenceNavigationProperty;
            }
        }

        /// <summary>
        /// Gets the type of the member, which for collection properties is the type
        /// of the collection rather than the type in the collection.
        /// </summary>
        /// <value> The type of the member. </value>
        public override Type MemberType
        {
            get { return _isCollection ? DbHelpers.CollectionType(ElementType) : ElementType; }
        }

        #endregion

        #region Entry factory methods

        /// <summary>
        /// Creates a new <see cref="InternalMemberEntry" /> the runtime type of which will be
        /// determined by the metadata.
        /// </summary>
        /// <param name="internalEntityEntry"> The entity entry to which the member belongs. </param>
        /// <param name="parentPropertyEntry"> The parent property entry which will always be null for navigation entries. </param>
        /// <returns> The new entry. </returns>
        public override InternalMemberEntry CreateMemberEntry(
            InternalEntityEntry internalEntityEntry, InternalPropertyEntry parentPropertyEntry)
        {
            Debug.Assert(parentPropertyEntry == null, "Navigation entries cannot be nested; parentPropertyEntry must be null.");

            return _isCollection
                       ? (InternalMemberEntry)new InternalCollectionEntry(internalEntityEntry, this)
                       : new InternalReferenceEntry(internalEntityEntry, this);
        }

        #endregion
    }
}
