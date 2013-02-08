// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class SpecialOffer
    {
        public virtual int SpecialOfferID { get; set; }

        public virtual string Description { get; set; }

        public virtual decimal DiscountPct { get; set; }

        public virtual string Type { get; set; }

        public virtual string Category { get; set; }

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime EndDate { get; set; }

        public virtual int MinQty { get; set; }

        public virtual int? MaxQty { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<SpecialOfferProduct> SpecialOfferProducts
        {
            get
            {
                if (_specialOfferProducts == null)
                {
                    var newCollection = new FixupCollection<SpecialOfferProduct>();
                    newCollection.CollectionChanged += FixupSpecialOfferProducts;
                    _specialOfferProducts = newCollection;
                }
                return _specialOfferProducts;
            }
            set
            {
                if (!ReferenceEquals(_specialOfferProducts, value))
                {
                    var previousValue = _specialOfferProducts as FixupCollection<SpecialOfferProduct>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSpecialOfferProducts;
                    }
                    _specialOfferProducts = value;
                    var newValue = value as FixupCollection<SpecialOfferProduct>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSpecialOfferProducts;
                    }
                }
            }
        }

        private ICollection<SpecialOfferProduct> _specialOfferProducts;

        private void FixupSpecialOfferProducts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SpecialOfferProduct item in e.NewItems)
                {
                    item.SpecialOffer = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SpecialOfferProduct item in e.OldItems)
                {
                    if (ReferenceEquals(item.SpecialOffer, this))
                    {
                        item.SpecialOffer = null;
                    }
                }
            }
        }
    }
}
