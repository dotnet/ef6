// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Customer
    {
        public virtual int CustomerID { get; set; }

        public virtual int? TerritoryID
        {
            get { return _territoryID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_territoryID != value)
                    {
                        if (SalesTerritory != null
                            && SalesTerritory.TerritoryID != value)
                        {
                            SalesTerritory = null;
                        }
                        _territoryID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int? _territoryID;

        public virtual string AccountNumber { get; set; }

        public virtual string CustomerType { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual CustomerDiscount CustomerDiscount { get; set; }

        public virtual SalesTerritory SalesTerritory
        {
            get { return _salesTerritory; }
            set
            {
                if (!ReferenceEquals(_salesTerritory, value))
                {
                    var previousValue = _salesTerritory;
                    _salesTerritory = value;
                    FixupSalesTerritory(previousValue);
                }
            }
        }

        private SalesTerritory _salesTerritory;

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

        public virtual Individual Individual
        {
            get { return _individual; }
            set
            {
                if (!ReferenceEquals(_individual, value))
                {
                    var previousValue = _individual;
                    _individual = value;
                    FixupIndividual(previousValue);
                }
            }
        }

        private Individual _individual;

        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders
        {
            get
            {
                if (_salesOrderHeaders == null)
                {
                    var newCollection = new FixupCollection<SalesOrderHeader>();
                    newCollection.CollectionChanged += FixupSalesOrderHeaders;
                    _salesOrderHeaders = newCollection;
                }
                return _salesOrderHeaders;
            }
            set
            {
                if (!ReferenceEquals(_salesOrderHeaders, value))
                {
                    var previousValue = _salesOrderHeaders as FixupCollection<SalesOrderHeader>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesOrderHeaders;
                    }
                    _salesOrderHeaders = value;
                    var newValue = value as FixupCollection<SalesOrderHeader>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesOrderHeaders;
                    }
                }
            }
        }

        private ICollection<SalesOrderHeader> _salesOrderHeaders;

        public virtual Store Store
        {
            get { return _store; }
            set
            {
                if (!ReferenceEquals(_store, value))
                {
                    var previousValue = _store;
                    _store = value;
                    FixupStore(previousValue);
                }
            }
        }

        private Store _store;

        private bool _settingFK;

        private void FixupSalesTerritory(SalesTerritory previousValue)
        {
            if (previousValue != null
                && previousValue.Customers.Contains(this))
            {
                previousValue.Customers.Remove(this);
            }

            if (SalesTerritory != null)
            {
                if (!SalesTerritory.Customers.Contains(this))
                {
                    SalesTerritory.Customers.Add(this);
                }
                if (TerritoryID != SalesTerritory.TerritoryID)
                {
                    TerritoryID = SalesTerritory.TerritoryID;
                }
            }
            else if (!_settingFK)
            {
                TerritoryID = null;
            }
        }

        private void FixupIndividual(Individual previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.Customer, this))
            {
                previousValue.Customer = null;
            }

            if (Individual != null)
            {
                Individual.Customer = this;
            }
        }

        private void FixupStore(Store previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.Customer, this))
            {
                previousValue.Customer = null;
            }

            if (Store != null)
            {
                Store.Customer = this;
            }
        }

        private void FixupCustomerAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CustomerAddress item in e.NewItems)
                {
                    item.Customer = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CustomerAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Customer, this))
                    {
                        item.Customer = null;
                    }
                }
            }
        }

        private void FixupSalesOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderHeader item in e.NewItems)
                {
                    item.Customer = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Customer, this))
                    {
                        item.Customer = null;
                    }
                }
            }
        }
    }
}
