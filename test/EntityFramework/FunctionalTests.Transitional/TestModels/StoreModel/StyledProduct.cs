// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System.ComponentModel.DataAnnotations;

    public class StyledProduct : Product
    {
        [StringLength(150)]
        public virtual string Style { get; set; }
    }
}
