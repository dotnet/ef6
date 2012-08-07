// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductReview
    {
        public virtual int ProductReviewID { get; set; }

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

        public virtual string ReviewerName { get; set; }

        public virtual DateTime ReviewDate { get; set; }

        public virtual string EmailAddress { get; set; }

        public virtual int Rating { get; set; }

        public virtual string Comments { get; set; }

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
                && previousValue.ProductReviews.Contains(this))
            {
                previousValue.ProductReviews.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductReviews.Contains(this))
                {
                    Product.ProductReviews.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }
    }
}
