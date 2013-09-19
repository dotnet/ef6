// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    /// <summary>
    /// Controls the transaction creation behavior while executing a database command or query.
    /// </summary>
    public enum TransactionalBehavior
    {
        /// <summary>
        /// If no transaction is present then a new transaction will be used for the operation.
        /// </summary>
        EnsureTransaction,

        /// <summary>
        /// If an existing transaction is present then use it, otherwise execute the command or query without a transaction.
        /// </summary>
        DoNotEnsureTransaction
    }
}
