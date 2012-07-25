// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Class holding utility functions for metadata
    /// </summary>
    internal static class Util
    {
        #region Methods

        /// <summary>
        /// Throws an appropriate exception if the given item is a readonly, used when an attempt is made to change
        /// a property
        /// </summary>
        /// <param name="item">The item whose readonly is being tested</param>
        internal static void ThrowIfReadOnly(MetadataItem item)
        {
            Debug.Assert(item != null, "The given item is null");
            if (item.IsReadOnly)
            {
                throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
            }
        }

        /// <summary>
        /// Check to make sure the given item do have identity
        /// </summary>
        /// <param name="item">The item to check for valid identity</param>
        /// <param name="argumentName">The name of the argument</param>
        [Conditional("DEBUG")]
        internal static void AssertItemHasIdentity(MetadataItem item, string argumentName)
        {
            Debug.Assert(!string.IsNullOrEmpty(item.Identity), "Item has empty identity.");
            EntityUtil.GenericCheckArgumentNull(item, argumentName);
        }

        #endregion
    }
}
