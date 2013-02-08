// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductProductPhoto
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

        public virtual int ProductPhotoID
        {
            get { return _productPhotoID; }
            set
            {
                if (_productPhotoID != value)
                {
                    if (ProductPhoto != null
                        && ProductPhoto.ProductPhotoID != value)
                    {
                        ProductPhoto = null;
                    }
                    _productPhotoID = value;
                }
            }
        }

        private int _productPhotoID;

        public virtual bool Primary { get; set; }

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

        public virtual ProductPhoto ProductPhoto
        {
            get { return _productPhoto; }
            set
            {
                if (!ReferenceEquals(_productPhoto, value))
                {
                    var previousValue = _productPhoto;
                    _productPhoto = value;
                    FixupProductPhoto(previousValue);
                }
            }
        }

        private ProductPhoto _productPhoto;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.ProductProductPhotoes.Contains(this))
            {
                previousValue.ProductProductPhotoes.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductProductPhotoes.Contains(this))
                {
                    Product.ProductProductPhotoes.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }

        private void FixupProductPhoto(ProductPhoto previousValue)
        {
            if (previousValue != null
                && previousValue.ProductProductPhotoes.Contains(this))
            {
                previousValue.ProductProductPhotoes.Remove(this);
            }

            if (ProductPhoto != null)
            {
                if (!ProductPhoto.ProductProductPhotoes.Contains(this))
                {
                    ProductPhoto.ProductProductPhotoes.Add(this);
                }
                if (ProductPhotoID != ProductPhoto.ProductPhotoID)
                {
                    ProductPhotoID = ProductPhoto.ProductPhotoID;
                }
            }
        }
    }
}
