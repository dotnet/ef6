// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    public class Product : ProductBase
    {
        public string CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
