// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class CountryRegion
    {
        public virtual string CountryRegionCode { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<CountryRegionCurrency> CountryRegionCurrencies
        {
            get
            {
                if (_countryRegionCurrencies == null)
                {
                    var newCollection = new FixupCollection<CountryRegionCurrency>();
                    newCollection.CollectionChanged += FixupCountryRegionCurrencies;
                    _countryRegionCurrencies = newCollection;
                }
                return _countryRegionCurrencies;
            }
            set
            {
                if (!ReferenceEquals(_countryRegionCurrencies, value))
                {
                    var previousValue = _countryRegionCurrencies as FixupCollection<CountryRegionCurrency>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCountryRegionCurrencies;
                    }
                    _countryRegionCurrencies = value;
                    var newValue = value as FixupCollection<CountryRegionCurrency>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCountryRegionCurrencies;
                    }
                }
            }
        }
        private ICollection<CountryRegionCurrency> _countryRegionCurrencies;

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

        private void FixupCountryRegionCurrencies(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CountryRegionCurrency item in e.NewItems)
                {
                    item.CountryRegion = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CountryRegionCurrency item in e.OldItems)
                {
                    if (ReferenceEquals(item.CountryRegion, this))
                    {
                        item.CountryRegion = null;
                    }
                }
            }
        }

        private void FixupStateProvinces(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StateProvince item in e.NewItems)
                {
                    item.CountryRegion = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StateProvince item in e.OldItems)
                {
                    if (ReferenceEquals(item.CountryRegion, this))
                    {
                        item.CountryRegion = null;
                    }
                }
            }
        }
    }
}