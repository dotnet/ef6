// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;

    /// <summary>
    /// Represents the base item class for all the metadata
    /// </summary>
    public abstract class GlobalItem : MetadataItem
    {
        #region Constructors

        /// <summary>
        /// Implementing this internal constructor so that this class can't be derived
        /// outside this assembly
        /// </summary>
        internal GlobalItem()
        {
        }

        internal GlobalItem(MetadataFlags flags)
            : base(flags)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the DataSpace in which this type belongs to
        /// </summary>
        [MetadataProperty(typeof(DataSpace), false)]
        internal virtual DataSpace DataSpace
        {
            get
            {
                // Since there can be row types that span across spaces and we can have collections to such row types, we need to exclude RowType and collection type in this assert check
                Debug.Assert(
                    GetDataSpace() != (DataSpace)(-1) || BuiltInTypeKind == BuiltInTypeKind.RowType
                    || BuiltInTypeKind == BuiltInTypeKind.CollectionType, "DataSpace must have some valid value");
                return GetDataSpace();
            }
            set
            {
                // Whenever you assign the data space value, it must be unassigned or re-assigned to the same value.
                // The only exception being we sometimes need to create row types that contains types from various spaces
                Debug.Assert(
                    GetDataSpace() == (DataSpace)(-1) || GetDataSpace() == value || BuiltInTypeKind == BuiltInTypeKind.RowType
                    || BuiltInTypeKind == BuiltInTypeKind.CollectionType, "Invalid Value being set for DataSpace");
                SetDataSpace(value);
            }
        }

        #endregion
    }
}
