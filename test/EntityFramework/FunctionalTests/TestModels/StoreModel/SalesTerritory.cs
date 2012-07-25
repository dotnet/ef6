// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;

    public class SalesTerritory
    {
        [Key]
        public virtual int TerritoryID { get; set; }

        public virtual string Name { get; set; }

        public virtual string CountryRegionCode { get; set; }

        public virtual string Group { get; set; }

        public virtual decimal SalesYTD { get; set; }

        public virtual decimal SalesLastYear { get; set; }

        public virtual decimal CostYTD { get; set; }

        public virtual decimal CostLastYear { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<StateProvince> StateProvinces
        {
            get
            {
                if (_stateProvinces == null)
                {
                    var newCollection = new FixupCollection<StateProvince>();
                    newCollection.CollectionChanged += FixupStateProvinces;
                    _stateProvinces = newCollection;
                }
                return _stateProvinces;
            }
            set
            {
                if (!ReferenceEquals(_stateProvinces, value))
                {
                    var previousValue = _stateProvinces as FixupCollection<StateProvince>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupStateProvinces;
                    }
                    _stateProvinces = value;
                    var newValue = value as FixupCollection<StateProvince>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupStateProvinces;
                    }
                }
            }
        }
        private ICollection<StateProvince> _stateProvinces;

        public virtual ICollection<Customer> Customers
        {
            get
            {
                if (_customers == null)
                {
                    var newCollection = new FixupCollection<Customer>();
                    newCollection.CollectionChanged += FixupCustomers;
                    _customers = newCollection;
                }
                return _customers;
            }
            set
            {
                if (!ReferenceEquals(_customers, value))
                {
                    var previousValue = _customers as FixupCollection<Customer>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCustomers;
                    }
                    _customers = value;
                    var newValue = value as FixupCollection<Customer>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCustomers;
                    }
                }
            }
        }
        private ICollection<Customer> _customers;

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

        public virtual ICollection<SalesPerson> SalesPersons
        {
            get
            {
                if (_salesPersons == null)
                {
                    var newCollection = new FixupCollection<SalesPerson>();
                    newCollection.CollectionChanged += FixupSalesPersons;
                    _salesPersons = newCollection;
                }
                return _salesPersons;
            }
            set
            {
                if (!ReferenceEquals(_salesPersons, value))
                {
                    var previousValue = _salesPersons as FixupCollection<SalesPerson>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesPersons;
                    }
                    _salesPersons = value;
                    var newValue = value as FixupCollection<SalesPerson>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesPersons;
                    }
                }
            }
        }
        private ICollection<SalesPerson> _salesPersons;

        public virtual ICollection<SalesTerritoryHistory> SalesTerritoryHistories
        {
            get
            {
                if (_salesTerritoryHistories == null)
                {
                    var newCollection = new FixupCollection<SalesTerritoryHistory>();
                    newCollection.CollectionChanged += FixupSalesTerritoryHistories;
                    _salesTerritoryHistories = newCollection;
                }
                return _salesTerritoryHistories;
            }
            set
            {
                if (!ReferenceEquals(_salesTerritoryHistories, value))
                {
                    var previousValue = _salesTerritoryHistories as FixupCollection<SalesTerritoryHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesTerritoryHistories;
                    }
                    _salesTerritoryHistories = value;
                    var newValue = value as FixupCollection<SalesTerritoryHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesTerritoryHistories;
                    }
                }
            }
        }
        private ICollection<SalesTerritoryHistory> _salesTerritoryHistories;

        private void FixupStateProvinces(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StateProvince item in e.NewItems)
                {
                    item.SalesTerritory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StateProvince item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesTerritory, this))
                    {
                        item.SalesTerritory = null;
                    }
                }
            }
        }

        private void FixupCustomers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Customer item in e.NewItems)
                {
                    item.SalesTerritory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Customer item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesTerritory, this))
                    {
                        item.SalesTerritory = null;
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
                    item.SalesTerritory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesTerritory, this))
                    {
                        item.SalesTerritory = null;
                    }
                }
            }
        }

        private void FixupSalesPersons(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesPerson item in e.NewItems)
                {
                    item.SalesTerritory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesPerson item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesTerritory, this))
                    {
                        item.SalesTerritory = null;
                    }
                }
            }
        }

        private void FixupSalesTerritoryHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesTerritoryHistory item in e.NewItems)
                {
                    item.SalesTerritory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesTerritoryHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesTerritory, this))
                    {
                        item.SalesTerritory = null;
                    }
                }
            }
        }
    }
}