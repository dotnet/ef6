// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    public enum TransactionBehavior
    {
        Default,
        EnsureTransaction = Default,
        DoNotEnsureTransaction
    }
}
