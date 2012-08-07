// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    /// <summary>
    ///     An internal interface implemented by <see cref="DbSet{T}" /> and <see cref="DbSet" /> that allows access to
    ///     the internal set without using reflection.
    /// </summary>
    internal interface IInternalSetAdapter
    {
        #region Underlying internal set

        /// <summary>
        ///     The underlying internal set.
        /// </summary>
        IInternalSet InternalSet { get; }

        #endregion
    }
}
