// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Vendor
    {
        public virtual int VendorID { get; set; }

        public virtual string AccountNumber { get; set; }

        public virtual string Name { get; set; }

        public virtual byte CreditRating { get; set; }

        public virtual bool PreferredVendorStatus { get; set; }

        public virtual bool ActiveFlag { get; set; }

        public virtual string PurchasingWebServiceURL { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductVendor> ProductVendors
        {
            get
            {
                if (_productVendors == null)
                {
                    var newCollection = new FixupCollection<ProductVendor>();
                    newCollection.CollectionChanged += FixupProductVendors;
                    _productVendors = newCollection;
                }
                return _productVendors;
            }
            set
            {
                if (!ReferenceEquals(_productVendors, value))
                {
                    var previousValue = _productVendors as FixupCollection<ProductVendor>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductVendors;
                    }
                    _productVendors = value;
                    var newValue = value as FixupCollection<ProductVendor>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductVendors;
                    }
                }
            }
        }
        private ICollection<ProductVendor> _productVendors;

        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders
        {
            get
            {
                if (_purchaseOrderHeaders == null)
                {
                    var newCollection = new FixupCollection<PurchaseOrderHeader>();
                    newCollection.CollectionChanged += FixupPurchaseOrderHeaders;
                    _purchaseOrderHeaders = newCollection;
                }
                return _purchaseOrderHeaders;
            }
            set
            {
                if (!ReferenceEquals(_purchaseOrderHeaders, value))
                {
                    var previousValue = _purchaseOrderHeaders as FixupCollection<PurchaseOrderHeader>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupPurchaseOrderHeaders;
                    }
                    _purchaseOrderHeaders = value;
                    var newValue = value as FixupCollection<PurchaseOrderHeader>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupPurchaseOrderHeaders;
                    }
                }
            }
        }
        private ICollection<PurchaseOrderHeader> _purchaseOrderHeaders;

        public virtual ICollection<VendorAddress> VendorAddresses
        {
            get
            {
                if (_vendorAddresses == null)
                {
                    var newCollection = new FixupCollection<VendorAddress>();
                    newCollection.CollectionChanged += FixupVendorAddresses;
                    _vendorAddresses = newCollection;
                }
                return _vendorAddresses;
            }
            set
            {
                if (!ReferenceEquals(_vendorAddresses, value))
                {
                    var previousValue = _vendorAddresses as FixupCollection<VendorAddress>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupVendorAddresses;
                    }
                    _vendorAddresses = value;
                    var newValue = value as FixupCollection<VendorAddress>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupVendorAddresses;
                    }
                }
            }
        }
        private ICollection<VendorAddress> _vendorAddresses;

        public virtual ICollection<VendorContact> VendorContacts
        {
            get
            {
                if (_vendorContacts == null)
                {
                    var newCollection = new FixupCollection<VendorContact>();
                    newCollection.CollectionChanged += FixupVendorContacts;
                    _vendorContacts = newCollection;
                }
                return _vendorContacts;
            }
            set
            {
                if (!ReferenceEquals(_vendorContacts, value))
                {
                    var previousValue = _vendorContacts as FixupCollection<VendorContact>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupVendorContacts;
                    }
                    _vendorContacts = value;
                    var newValue = value as FixupCollection<VendorContact>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupVendorContacts;
                    }
                }
            }
        }
        private ICollection<VendorContact> _vendorContacts;

        private void FixupProductVendors(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductVendor item in e.NewItems)
                {
                    item.Vendor = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductVendor item in e.OldItems)
                {
                    if (ReferenceEquals(item.Vendor, this))
                    {
                        item.Vendor = null;
                    }
                }
            }
        }

        private void FixupPurchaseOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PurchaseOrderHeader item in e.NewItems)
                {
                    item.Vendor = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PurchaseOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Vendor, this))
                    {
                        item.Vendor = null;
                    }
                }
            }
        }

        private void FixupVendorAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (VendorAddress item in e.NewItems)
                {
                    item.Vendor = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Vendor, this))
                    {
                        item.Vendor = null;
                    }
                }
            }
        }

        private void FixupVendorContacts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (VendorContact item in e.NewItems)
                {
                    item.Vendor = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.Vendor, this))
                    {
                        item.Vendor = null;
                    }
                }
            }
        }
    }
}