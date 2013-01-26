// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class SalesPerson
    {
        public virtual int SalesPersonID
        {
            get { return _salesPersonID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_salesPersonID != value)
                    {
                        if (Employee != null
                            && Employee.EmployeeID != value)
                        {
                            Employee = null;
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

        private int _salesPersonID;

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

        public virtual decimal? SalesQuota { get; set; }

        public virtual decimal Bonus { get; set; }

        public virtual decimal CommissionPct { get; set; }

        public virtual decimal SalesYTD { get; set; }

        public virtual decimal SalesLastYear { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Employee Employee
        {
            get { return _employee; }
            set
            {
                if (!ReferenceEquals(_employee, value))
                {
                    var previousValue = _employee;
                    _employee = value;
                    FixupEmployee(previousValue);
                }
            }
        }

        private Employee _employee;

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

        public virtual ICollection<SalesPersonQuotaHistory> SalesPersonQuotaHistories
        {
            get
            {
                if (_salesPersonQuotaHistories == null)
                {
                    var newCollection = new FixupCollection<SalesPersonQuotaHistory>();
                    newCollection.CollectionChanged += FixupSalesPersonQuotaHistories;
                    _salesPersonQuotaHistories = newCollection;
                }
                return _salesPersonQuotaHistories;
            }
            set
            {
                if (!ReferenceEquals(_salesPersonQuotaHistories, value))
                {
                    var previousValue = _salesPersonQuotaHistories as FixupCollection<SalesPersonQuotaHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesPersonQuotaHistories;
                    }
                    _salesPersonQuotaHistories = value;
                    var newValue = value as FixupCollection<SalesPersonQuotaHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesPersonQuotaHistories;
                    }
                }
            }
        }

        private ICollection<SalesPersonQuotaHistory> _salesPersonQuotaHistories;

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

        public virtual ICollection<Store> Stores
        {
            get
            {
                if (_stores == null)
                {
                    var newCollection = new FixupCollection<Store>();
                    newCollection.CollectionChanged += FixupStores;
                    _stores = newCollection;
                }
                return _stores;
            }
            set
            {
                if (!ReferenceEquals(_stores, value))
                {
                    var previousValue = _stores as FixupCollection<Store>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupStores;
                    }
                    _stores = value;
                    var newValue = value as FixupCollection<Store>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupStores;
                    }
                }
            }
        }

        private ICollection<Store> _stores;

        private bool _settingFK;

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.SalesPerson, this))
            {
                previousValue.SalesPerson = null;
            }

            if (Employee != null)
            {
                Employee.SalesPerson = this;
                if (SalesPersonID != Employee.EmployeeID)
                {
                    SalesPersonID = Employee.EmployeeID;
                }
            }
        }

        private void FixupSalesTerritory(SalesTerritory previousValue)
        {
            if (previousValue != null
                && previousValue.SalesPersons.Contains(this))
            {
                previousValue.SalesPersons.Remove(this);
            }

            if (SalesTerritory != null)
            {
                if (!SalesTerritory.SalesPersons.Contains(this))
                {
                    SalesTerritory.SalesPersons.Add(this);
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

        private void FixupSalesOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderHeader item in e.NewItems)
                {
                    item.SalesPerson = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesPerson, this))
                    {
                        item.SalesPerson = null;
                    }
                }
            }
        }

        private void FixupSalesPersonQuotaHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesPersonQuotaHistory item in e.NewItems)
                {
                    item.SalesPerson = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesPersonQuotaHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesPerson, this))
                    {
                        item.SalesPerson = null;
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
                    item.SalesPerson = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesTerritoryHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesPerson, this))
                    {
                        item.SalesPerson = null;
                    }
                }
            }
        }

        private void FixupStores(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Store item in e.NewItems)
                {
                    item.SalesPerson = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Store item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesPerson, this))
                    {
                        item.SalesPerson = null;
                    }
                }
            }
        }
    }
}
