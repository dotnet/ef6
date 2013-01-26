// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ShoppingCartItem
    {
        public virtual int ShoppingCartItemID { get; set; }

        public virtual string ShoppingCartID { get; set; }

        public virtual int Quantity { get; set; }

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

        public virtual DateTime DateCreated { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        [InverseProperty("ShoppingCartItems")]
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
                && previousValue.ShoppingCartItems.Contains(this))
            {
                previousValue.ShoppingCartItems.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ShoppingCartItems.Contains(this))
                {
                    Product.ShoppingCartItems.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }
    }
}
