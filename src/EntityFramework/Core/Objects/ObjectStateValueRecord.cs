// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    internal enum ObjectStateValueRecord
    {
        OriginalReadonly = 0,
        CurrentUpdatable = 1,
        OriginalUpdatableInternal = 2,
        OriginalUpdatablePublic = 3,
    }
}
