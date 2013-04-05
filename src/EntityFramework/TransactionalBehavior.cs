// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    /// <summary>
    ///     Controls the transaction creation behavior
    /// </summary>
    public enum TransactionalBehavior
    {
        EnsureTransaction,
        DoNotEnsureTransaction
    }
}
