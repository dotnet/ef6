// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Internal;

    public class FakeEntity
    {
        public int Id { get; set; }

        internal static readonly PropertyEntryMetadata FakeNamedFooPropertyMetadata = new PropertyEntryMetadata(
            typeof(FakeEntity), typeof(string), "Foo", isMapped: false, isComplex: false);
    }
}
