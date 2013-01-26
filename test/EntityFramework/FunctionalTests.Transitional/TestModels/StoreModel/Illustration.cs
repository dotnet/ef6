// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Illustration
    {
        public virtual int IllustrationID { get; set; }

        public virtual string Diagram { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductModelIllustration> ProductModelIllustrations
        {
            get
            {
                if (_productModelIllustrations == null)
                {
                    var newCollection = new FixupCollection<ProductModelIllustration>();
                    newCollection.CollectionChanged += FixupProductModelIllustrations;
                    _productModelIllustrations = newCollection;
                }
                return _productModelIllustrations;
            }
            set
            {
                if (!ReferenceEquals(_productModelIllustrations, value))
                {
                    var previousValue = _productModelIllustrations as FixupCollection<ProductModelIllustration>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductModelIllustrations;
                    }
                    _productModelIllustrations = value;
                    var newValue = value as FixupCollection<ProductModelIllustration>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductModelIllustrations;
                    }
                }
            }
        }

        private ICollection<ProductModelIllustration> _productModelIllustrations;

        private void FixupProductModelIllustrations(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductModelIllustration item in e.NewItems)
                {
                    item.Illustration = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductModelIllustration item in e.OldItems)
                {
                    if (ReferenceEquals(item.Illustration, this))
                    {
                        item.Illustration = null;
                    }
                }
            }
        }
    }
}
