// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Culture
    {
        public virtual string CultureID { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures
        {
            get
            {
                if (_productModelProductDescriptionCultures == null)
                {
                    var newCollection = new FixupCollection<ProductModelProductDescriptionCulture>();
                    newCollection.CollectionChanged += FixupProductModelProductDescriptionCultures;
                    _productModelProductDescriptionCultures = newCollection;
                }
                return _productModelProductDescriptionCultures;
            }
            set
            {
                if (!ReferenceEquals(_productModelProductDescriptionCultures, value))
                {
                    var previousValue = _productModelProductDescriptionCultures as FixupCollection<ProductModelProductDescriptionCulture>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductModelProductDescriptionCultures;
                    }
                    _productModelProductDescriptionCultures = value;
                    var newValue = value as FixupCollection<ProductModelProductDescriptionCulture>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductModelProductDescriptionCultures;
                    }
                }
            }
        }

        private ICollection<ProductModelProductDescriptionCulture> _productModelProductDescriptionCultures;

        private void FixupProductModelProductDescriptionCultures(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductModelProductDescriptionCulture item in e.NewItems)
                {
                    item.Culture = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductModelProductDescriptionCulture item in e.OldItems)
                {
                    if (ReferenceEquals(item.Culture, this))
                    {
                        item.Culture = null;
                    }
                }
            }
        }
    }
}
