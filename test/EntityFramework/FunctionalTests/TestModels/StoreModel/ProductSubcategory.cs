// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class ProductSubcategory
    {
        public virtual int ProductSubcategoryID { get; set; }

        public virtual int ProductCategoryID
        {
            get { return _productCategoryID; }
            set
            {
                if (_productCategoryID != value)
                {
                    if (ProductCategory != null && ProductCategory.ProductCategoryID != value)
                    {
                        ProductCategory = null;
                    }
                    _productCategoryID = value;
                }
            }
        }
        private int _productCategoryID;

        public virtual string Name { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<Product> Products
        {
            get
            {
                if (_products == null)
                {
                    var newCollection = new FixupCollection<Product>();
                    newCollection.CollectionChanged += FixupProducts;
                    _products = newCollection;
                }
                return _products;
            }
            set
            {
                if (!ReferenceEquals(_products, value))
                {
                    var previousValue = _products as FixupCollection<Product>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProducts;
                    }
                    _products = value;
                    var newValue = value as FixupCollection<Product>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProducts;
                    }
                }
            }
        }
        private ICollection<Product> _products;

        public virtual ProductCategory ProductCategory
        {
            get { return _productCategory; }
            set
            {
                if (!ReferenceEquals(_productCategory, value))
                {
                    var previousValue = _productCategory;
                    _productCategory = value;
                    FixupProductCategory(previousValue);
                }
            }
        }
        private ProductCategory _productCategory;

        private void FixupProductCategory(ProductCategory previousValue)
        {
            if (previousValue != null && previousValue.ProductSubcategories.Contains(this))
            {
                previousValue.ProductSubcategories.Remove(this);
            }

            if (ProductCategory != null)
            {
                if (!ProductCategory.ProductSubcategories.Contains(this))
                {
                    ProductCategory.ProductSubcategories.Add(this);
                }
                if (ProductCategoryID != ProductCategory.ProductCategoryID)
                {
                    ProductCategoryID = ProductCategory.ProductCategoryID;
                }
            }
        }

        private void FixupProducts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Product item in e.NewItems)
                {
                    item.ProductSubcategory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Product item in e.OldItems)
                {
                    if (ReferenceEquals(item.ProductSubcategory, this))
                    {
                        item.ProductSubcategory = null;
                    }
                }
            }
        }
    }
}