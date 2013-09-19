// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class ProductCategory
    {
        public virtual int ProductCategoryID { get; set; }

        public virtual string Name { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductSubcategory> ProductSubcategories
        {
            get
            {
                if (_productSubcategories == null)
                {
                    var newCollection = new FixupCollection<ProductSubcategory>();
                    newCollection.CollectionChanged += FixupProductSubcategories;
                    _productSubcategories = newCollection;
                }
                return _productSubcategories;
            }
            set
            {
                if (!ReferenceEquals(_productSubcategories, value))
                {
                    var previousValue = _productSubcategories as FixupCollection<ProductSubcategory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductSubcategories;
                    }
                    _productSubcategories = value;
                    var newValue = value as FixupCollection<ProductSubcategory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductSubcategories;
                    }
                }
            }
        }

        private ICollection<ProductSubcategory> _productSubcategories;

        private void FixupProductSubcategories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductSubcategory item in e.NewItems)
                {
                    item.ProductCategory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductSubcategory item in e.OldItems)
                {
                    if (ReferenceEquals(item.ProductCategory, this))
                    {
                        item.ProductCategory = null;
                    }
                }
            }
        }
    }
}
