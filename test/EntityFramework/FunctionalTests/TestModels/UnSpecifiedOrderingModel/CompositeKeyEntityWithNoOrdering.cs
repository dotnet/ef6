// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnSpecifiedOrderingModel
{
    using System.ComponentModel.DataAnnotations;

    public class CompositeKeyEntityWithNoOrdering
    {
        [Key]
        public int intKey { get; set; }

        [Key]
        public float floatKey { get; set; }

        [Key]
        public byte[] binaryKey { get; set; }
    }
}
