// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    // <summary>
    // The types of member entries supported.
    // </summary>
    internal enum MemberEntryType
    {
        ReferenceNavigationProperty,
        CollectionNavigationProperty,
        ScalarProperty,
        ComplexProperty,
    }
}
