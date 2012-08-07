// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class ContactType
    {
        public virtual int ContactTypeID { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<StoreContact> StoreContacts
        {
            get
            {
                if (_storeContacts == null)
                {
                    var newCollection = new FixupCollection<StoreContact>();
                    newCollection.CollectionChanged += FixupStoreContacts;
                    _storeContacts = newCollection;
                }
                return _storeContacts;
            }
            set
            {
                if (!ReferenceEquals(_storeContacts, value))
                {
                    var previousValue = _storeContacts as FixupCollection<StoreContact>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupStoreContacts;
                    }
                    _storeContacts = value;
                    var newValue = value as FixupCollection<StoreContact>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupStoreContacts;
                    }
                }
            }
        }

        private ICollection<StoreContact> _storeContacts;

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

        private void FixupStoreContacts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StoreContact item in e.NewItems)
                {
                    item.ContactType = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StoreContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.ContactType, this))
                    {
                        item.ContactType = null;
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
                    item.ContactType = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.ContactType, this))
                    {
                        item.ContactType = null;
                    }
                }
            }
        }
    }
}
