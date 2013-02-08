// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class StateProvince
    {
        public virtual int StateProvinceID { get; set; }

        public virtual string StateProvinceCode { get; set; }

        public virtual string CountryRegionCode
        {
            get { return _countryRegionCode; }
            set
            {
                if (_countryRegionCode != value)
                {
                    if (CountryRegion != null
                        && CountryRegion.CountryRegionCode != value)
                    {
                        CountryRegion = null;
                    }
                    _countryRegionCode = value;
                }
            }
        }

        private string _countryRegionCode;

        public virtual bool IsOnlyStateProvinceFlag { get; set; }

        public virtual string Name { get; set; }

        public virtual int TerritoryID
        {
            get { return _territoryID; }
            set
            {
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
        }

        private int _territoryID;

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<Address> Addresses
        {
            get
            {
                if (_addresses == null)
                {
                    var newCollection = new FixupCollection<Address>();
                    newCollection.CollectionChanged += FixupAddresses;
                    _addresses = newCollection;
                }
                return _addresses;
            }
            set
            {
                if (!ReferenceEquals(_addresses, value))
                {
                    var previousValue = _addresses as FixupCollection<Address>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupAddresses;
                    }
                    _addresses = value;
                    var newValue = value as FixupCollection<Address>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupAddresses;
                    }
                }
            }
        }

        private ICollection<Address> _addresses;

        public virtual CountryRegion CountryRegion
        {
            get { return _countryRegion; }
            set
            {
                if (!ReferenceEquals(_countryRegion, value))
                {
                    var previousValue = _countryRegion;
                    _countryRegion = value;
                    FixupCountryRegion(previousValue);
                }
            }
        }

        private CountryRegion _countryRegion;

        public virtual ICollection<SalesTaxRate> SalesTaxRates
        {
            get
            {
                if (_salesTaxRates == null)
                {
                    var newCollection = new FixupCollection<SalesTaxRate>();
                    newCollection.CollectionChanged += FixupSalesTaxRates;
                    _salesTaxRates = newCollection;
                }
                return _salesTaxRates;
            }
            set
            {
                if (!ReferenceEquals(_salesTaxRates, value))
                {
                    var previousValue = _salesTaxRates as FixupCollection<SalesTaxRate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesTaxRates;
                    }
                    _salesTaxRates = value;
                    var newValue = value as FixupCollection<SalesTaxRate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesTaxRates;
                    }
                }
            }
        }

        private ICollection<SalesTaxRate> _salesTaxRates;

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

        private void FixupCountryRegion(CountryRegion previousValue)
        {
            if (previousValue != null
                && previousValue.StateProvinces.Contains(this))
            {
                previousValue.StateProvinces.Remove(this);
            }

            if (CountryRegion != null)
            {
                if (!CountryRegion.StateProvinces.Contains(this))
                {
                    CountryRegion.StateProvinces.Add(this);
                }
                if (CountryRegionCode != CountryRegion.CountryRegionCode)
                {
                    CountryRegionCode = CountryRegion.CountryRegionCode;
                }
            }
        }

        private void FixupSalesTerritory(SalesTerritory previousValue)
        {
            if (previousValue != null
                && previousValue.StateProvinces.Contains(this))
            {
                previousValue.StateProvinces.Remove(this);
            }

            if (SalesTerritory != null)
            {
                if (!SalesTerritory.StateProvinces.Contains(this))
                {
                    SalesTerritory.StateProvinces.Add(this);
                }
                if (TerritoryID != SalesTerritory.TerritoryID)
                {
                    TerritoryID = SalesTerritory.TerritoryID;
                }
            }
        }

        private void FixupAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Address item in e.NewItems)
                {
                    item.StateProvince = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Address item in e.OldItems)
                {
                    if (ReferenceEquals(item.StateProvince, this))
                    {
                        item.StateProvince = null;
                    }
                }
            }
        }

        private void FixupSalesTaxRates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesTaxRate item in e.NewItems)
                {
                    item.StateProvince = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesTaxRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.StateProvince, this))
                    {
                        item.StateProvince = null;
                    }
                }
            }
        }
    }
}
