// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System.Collections.Generic;

    public class ProductDescription
    {
        public virtual int ProductDescriptionID { get; set; }
        public virtual string Description { get; set; }

        public virtual RowDetails RowDetails { get; set; }

        public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; }
    }
}
