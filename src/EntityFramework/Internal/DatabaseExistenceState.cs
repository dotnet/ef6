// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    internal enum DatabaseExistenceState
    {
        Unknown,
        DoesNotExist,
        ExistsConsideredEmpty,
        Exists
    }
}
