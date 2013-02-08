// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class SalesTaxRate
    {
        public virtual int SalesTaxRateID { get; set; }

        public virtual int StateProvinceID
        {
            get { return _stateProvinceID; }
            set
            {
                if (_stateProvinceID != value)
                {
                    if (StateProvince != null
                        && StateProvince.StateProvinceID != value)
                    {
                        StateProvince = null;
                    }
                    _stateProvinceID = value;
                }
            }
        }

        private int _stateProvinceID;

        public virtual byte TaxType { get; set; }

        public virtual decimal TaxRate { get; set; }

        public virtual string Name { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual StateProvince StateProvince
        {
            get { return _stateProvince; }
            set
            {
                if (!ReferenceEquals(_stateProvince, value))
                {
                    var previousValue = _stateProvince;
                    _stateProvince = value;
                    FixupStateProvince(previousValue);
                }
            }
        }

        private StateProvince _stateProvince;

        private void FixupStateProvince(StateProvince previousValue)
        {
            if (previousValue != null
                && previousValue.SalesTaxRates.Contains(this))
            {
                previousValue.SalesTaxRates.Remove(this);
            }

            if (StateProvince != null)
            {
                if (!StateProvince.SalesTaxRates.Contains(this))
                {
                    StateProvince.SalesTaxRates.Add(this);
                }
                if (StateProvinceID != StateProvince.StateProvinceID)
                {
                    StateProvinceID = StateProvince.StateProvinceID;
                }
            }
        }
    }
}
