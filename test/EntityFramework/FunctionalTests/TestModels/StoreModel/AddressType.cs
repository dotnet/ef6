// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class AddressType
    {
        public virtual int AddressTypeID { get; set; }

        public virtual string Name { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<CustomerAddress> CustomerAddresses
        {
            get
            {
                if (_customerAddresses == null)
                {
                    var newCollection = new FixupCollection<CustomerAddress>();
                    newCollection.CollectionChanged += FixupCustomerAddresses;
                    _customerAddresses = newCollection;
                }
                return _customerAddresses;
            }
            set
            {
                if (!ReferenceEquals(_customerAddresses, value))
                {
                    var previousValue = _customerAddresses as FixupCollection<CustomerAddress>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCustomerAddresses;
                    }
                    _customerAddresses = value;
                    var newValue = value as FixupCollection<CustomerAddress>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCustomerAddresses;
                    }
                }
            }
        }

        private ICollection<CustomerAddress> _customerAddresses;

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

        private void FixupCustomerAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CustomerAddress item in e.NewItems)
                {
                    item.AddressType = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CustomerAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.AddressType, this))
                    {
                        item.AddressType = null;
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
                    item.AddressType = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.AddressType, this))
                    {
                        item.AddressType = null;
                    }
                }
            }
        }
    }
}
