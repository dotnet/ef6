// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class DiscontinuedProduct : Product
    {
        public virtual DateTime DiscontinuedDate { get; set; }
    }
}
