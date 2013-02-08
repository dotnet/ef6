// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Currency
    {
        public virtual string CurrencyCode { get; set; }

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

        public virtual ICollection<CurrencyRate> CurrencyRates
        {
            get
            {
                if (_currencyRates == null)
                {
                    var newCollection = new FixupCollection<CurrencyRate>();
                    newCollection.CollectionChanged += FixupCurrencyRates;
                    _currencyRates = newCollection;
                }
                return _currencyRates;
            }
            set
            {
                if (!ReferenceEquals(_currencyRates, value))
                {
                    var previousValue = _currencyRates as FixupCollection<CurrencyRate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCurrencyRates;
                    }
                    _currencyRates = value;
                    var newValue = value as FixupCollection<CurrencyRate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCurrencyRates;
                    }
                }
            }
        }

        private ICollection<CurrencyRate> _currencyRates;

        public virtual ICollection<CurrencyRate> CurrencyRates1
        {
            get
            {
                if (_currencyRates1 == null)
                {
                    var newCollection = new FixupCollection<CurrencyRate>();
                    newCollection.CollectionChanged += FixupCurrencyRates1;
                    _currencyRates1 = newCollection;
                }
                return _currencyRates1;
            }
            set
            {
                if (!ReferenceEquals(_currencyRates1, value))
                {
                    var previousValue = _currencyRates1 as FixupCollection<CurrencyRate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCurrencyRates1;
                    }
                    _currencyRates1 = value;
                    var newValue = value as FixupCollection<CurrencyRate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCurrencyRates1;
                    }
                }
            }
        }

        private ICollection<CurrencyRate> _currencyRates1;

        private void FixupCountryRegionCurrencies(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CountryRegionCurrency item in e.NewItems)
                {
                    item.Currency = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CountryRegionCurrency item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }

        private void FixupCurrencyRates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CurrencyRate item in e.NewItems)
                {
                    item.Currency = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CurrencyRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }

        private void FixupCurrencyRates1(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CurrencyRate item in e.NewItems)
                {
                    item.Currency1 = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CurrencyRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency1, this))
                    {
                        item.Currency1 = null;
                    }
                }
            }
        }
    }
}
