// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class SalesPersonQuotaHistory
    {
        public virtual int SalesPersonID
        {
            get { return _salesPersonID; }
            set
            {
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
        }

        private int _salesPersonID;

        public virtual DateTime QuotaDate { get; set; }

        public virtual decimal SalesQuota { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

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

        private void FixupSalesPerson(SalesPerson previousValue)
        {
            if (previousValue != null
                && previousValue.SalesPersonQuotaHistories.Contains(this))
            {
                previousValue.SalesPersonQuotaHistories.Remove(this);
            }

            if (SalesPerson != null)
            {
                if (!SalesPerson.SalesPersonQuotaHistories.Contains(this))
                {
                    SalesPerson.SalesPersonQuotaHistories.Add(this);
                }
                if (SalesPersonID != SalesPerson.SalesPersonID)
                {
                    SalesPersonID = SalesPerson.SalesPersonID;
                }
            }
        }
    }
}
