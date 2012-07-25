// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace SimpleModel
{
    using System;
    using System.Globalization;

    public class ProductForLinq : BaseTypeForLinq
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "ID: {0}, Name: {1}, Category: {2}, UnitePrice: {3}, UnitsInStock: {4}", Id, ProductName, Category, UnitPrice, UnitsInStock);
        }
    }
}
