// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductListPriceHistory
    {
        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
                if (_productID != value)
                {
                    if (Product != null
                        && Product.ProductID != value)
                    {
                        Product = null;
                    }
                    _productID = value;
                }
            }
        }

        private int _productID;

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual decimal ListPrice { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Product Product
        {
            get { return _product; }
            set
            {
                if (!ReferenceEquals(_product, value))
                {
                    var previousValue = _product;
                    _product = value;
                    FixupProduct(previousValue);
                }
            }
        }

        private Product _product;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.ProductListPriceHistories.Contains(this))
            {
                previousValue.ProductListPriceHistories.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductListPriceHistories.Contains(this))
                {
                    Product.ProductListPriceHistories.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }
    }
}
