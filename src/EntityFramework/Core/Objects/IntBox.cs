// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    // <summary>
    // This class is used in Referential Integrity Constraints feature.
    // It is used to get around the problem of enumerating dictionary contents,
    // but allowing update of the value without breaking the enumerator.
    // </summary>
    internal sealed class IntBox
    {
        internal IntBox(int val)
        {
            Value = val;
        }

        internal int Value { get; set; }
    }
}
