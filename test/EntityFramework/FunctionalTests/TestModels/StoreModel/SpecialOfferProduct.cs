// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class SpecialOfferProduct
    {
        public virtual int SpecialOfferID
        {
            get { return _specialOfferID; }
            set
            {
                if (_specialOfferID != value)
                {
                    if (SpecialOffer != null && SpecialOffer.SpecialOfferID != value)
                    {
                        SpecialOffer = null;
                    }
                    _specialOfferID = value;
                }
            }
        }
        private int _specialOfferID;

        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
                if (_productID != value)
                {
                    if (Product != null && Product.ProductID != value)
                    {
                        Product = null;
                    }
                    _productID = value;
                }
            }
        }
        private int _productID;

        public virtual Guid rowguid { get; set; }

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

        public virtual ICollection<SalesOrderDetail> SalesOrderDetails
        {
            get
            {
                if (_salesOrderDetails == null)
                {
                    var newCollection = new FixupCollection<SalesOrderDetail>();
                    newCollection.CollectionChanged += FixupSalesOrderDetails;
                    _salesOrderDetails = newCollection;
                }
                return _salesOrderDetails;
            }
            set
            {
                if (!ReferenceEquals(_salesOrderDetails, value))
                {
                    var previousValue = _salesOrderDetails as FixupCollection<SalesOrderDetail>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesOrderDetails;
                    }
                    _salesOrderDetails = value;
                    var newValue = value as FixupCollection<SalesOrderDetail>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesOrderDetails;
                    }
                }
            }
        }
        private ICollection<SalesOrderDetail> _salesOrderDetails;

        public virtual SpecialOffer SpecialOffer
        {
            get { return _specialOffer; }
            set
            {
                if (!ReferenceEquals(_specialOffer, value))
                {
                    var previousValue = _specialOffer;
                    _specialOffer = value;
                    FixupSpecialOffer(previousValue);
                }
            }
        }
        private SpecialOffer _specialOffer;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null && previousValue.SpecialOfferProducts.Contains(this))
            {
                previousValue.SpecialOfferProducts.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.SpecialOfferProducts.Contains(this))
                {
                    Product.SpecialOfferProducts.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }

        private void FixupSpecialOffer(SpecialOffer previousValue)
        {
            if (previousValue != null && previousValue.SpecialOfferProducts.Contains(this))
            {
                previousValue.SpecialOfferProducts.Remove(this);
            }

            if (SpecialOffer != null)
            {
                if (!SpecialOffer.SpecialOfferProducts.Contains(this))
                {
                    SpecialOffer.SpecialOfferProducts.Add(this);
                }
                if (SpecialOfferID != SpecialOffer.SpecialOfferID)
                {
                    SpecialOfferID = SpecialOffer.SpecialOfferID;
                }
            }
        }

        private void FixupSalesOrderDetails(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderDetail item in e.NewItems)
                {
                    item.SpecialOfferProduct = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderDetail item in e.OldItems)
                {
                    if (ReferenceEquals(item.SpecialOfferProduct, this))
                    {
                        item.SpecialOfferProduct = null;
                    }
                }
            }
        }
    }
}