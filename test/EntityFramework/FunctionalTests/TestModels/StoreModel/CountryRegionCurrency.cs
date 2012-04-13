namespace FunctionalTests.Model
{
    using System;

    public class CountryRegionCurrency
    {
        public virtual string CountryRegionCode
        {
            get { return _countryRegionCode; }
            set
            {
                if (_countryRegionCode != value)
                {
                    if (CountryRegion != null && CountryRegion.CountryRegionCode != value)
                    {
                        CountryRegion = null;
                    }
                    _countryRegionCode = value;
                }
            }
        }
        private string _countryRegionCode;

        public virtual string CurrencyCode
        {
            get { return _currencyCode; }
            set
            {
                if (_currencyCode != value)
                {
                    if (Currency != null && Currency.CurrencyCode != value)
                    {
                        Currency = null;
                    }
                    _currencyCode = value;
                }
            }
        }
        private string _currencyCode;

        public virtual DateTime ModifiedDate { get; set; }

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

        public virtual Currency Currency
        {
            get { return _currency; }
            set
            {
                if (!ReferenceEquals(_currency, value))
                {
                    var previousValue = _currency;
                    _currency = value;
                    FixupCurrency(previousValue);
                }
            }
        }
        private Currency _currency;

        private void FixupCountryRegion(CountryRegion previousValue)
        {
            if (previousValue != null && previousValue.CountryRegionCurrencies.Contains(this))
            {
                previousValue.CountryRegionCurrencies.Remove(this);
            }

            if (CountryRegion != null)
            {
                if (!CountryRegion.CountryRegionCurrencies.Contains(this))
                {
                    CountryRegion.CountryRegionCurrencies.Add(this);
                }
                if (CountryRegionCode != CountryRegion.CountryRegionCode)
                {
                    CountryRegionCode = CountryRegion.CountryRegionCode;
                }
            }
        }

        private void FixupCurrency(Currency previousValue)
        {
            if (previousValue != null && previousValue.CountryRegionCurrencies.Contains(this))
            {
                previousValue.CountryRegionCurrencies.Remove(this);
            }

            if (Currency != null)
            {
                if (!Currency.CountryRegionCurrencies.Contains(this))
                {
                    Currency.CountryRegionCurrencies.Add(this);
                }
                if (CurrencyCode != Currency.CurrencyCode)
                {
                    CurrencyCode = Currency.CurrencyCode;
                }
            }
        }
    }
}