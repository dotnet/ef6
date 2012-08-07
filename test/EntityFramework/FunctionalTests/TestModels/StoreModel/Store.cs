// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Store
    {
        public virtual int CustomerID
        {
            get { return _customerID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_customerID != value)
                    {
                        if (Customer != null
                            && Customer.CustomerID != value)
                        {
                            Customer = null;
                        }
                        _customerID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int _customerID;

        public virtual string Name { get; set; }

        public virtual int? SalesPersonID
        {
            get { return _salesPersonID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_salesPersonID != value)
                    {
                        if (SalesPerson != null
                            && SalesPerson.SalesPersonID != value)
                        {
                            SalesPerson = null;
                        }
                        _salesPersonID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int? _salesPersonID;

        public virtual string Demographics { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Customer Customer
        {
            get { return _customer; }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer;
                    _customer = value;
                    FixupCustomer(previousValue);
                }
            }
        }

        private Customer _customer;

        public virtual SalesPerson SalesPerson
        {
            get { return _salesPerson; }
            set
            {
                if (!ReferenceEquals(_salesPerson, value))
                {
                    var previousValue = _salesPerson;
                    _salesPerson = value;
                    FixupSalesPerson(previousValue);
                }
            }
        }

        private SalesPerson _salesPerson;

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

        private bool _settingFK;

        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.Store, this))
            {
                previousValue.Store = null;
            }

            if (Customer != null)
            {
                Customer.Store = this;
                if (CustomerID != Customer.CustomerID)
                {
                    CustomerID = Customer.CustomerID;
                }
            }
        }

        private void FixupSalesPerson(SalesPerson previousValue)
        {
            if (previousValue != null
                && previousValue.Stores.Contains(this))
            {
                previousValue.Stores.Remove(this);
            }

            if (SalesPerson != null)
            {
                if (!SalesPerson.Stores.Contains(this))
                {
                    SalesPerson.Stores.Add(this);
                }
                if (SalesPersonID != SalesPerson.SalesPersonID)
                {
                    SalesPersonID = SalesPerson.SalesPersonID;
                }
            }
            else if (!_settingFK)
            {
                SalesPersonID = null;
            }
        }

        private void FixupStoreContacts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StoreContact item in e.NewItems)
                {
                    item.Store = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StoreContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.Store, this))
                    {
                        item.Store = null;
                    }
                }
            }
        }
    }
}
