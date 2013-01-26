// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class SalesTerritoryHistory
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

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

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

        private void FixupSalesPerson(SalesPerson previousValue)
        {
            if (previousValue != null
                && previousValue.SalesTerritoryHistories.Contains(this))
            {
                previousValue.SalesTerritoryHistories.Remove(this);
            }

            if (SalesPerson != null)
            {
                if (!SalesPerson.SalesTerritoryHistories.Contains(this))
                {
                    SalesPerson.SalesTerritoryHistories.Add(this);
                }
                if (SalesPersonID != SalesPerson.SalesPersonID)
                {
                    SalesPersonID = SalesPerson.SalesPersonID;
                }
            }
        }

        private void FixupSalesTerritory(SalesTerritory previousValue)
        {
            if (previousValue != null
                && previousValue.SalesTerritoryHistories.Contains(this))
            {
                previousValue.SalesTerritoryHistories.Remove(this);
            }

            if (SalesTerritory != null)
            {
                if (!SalesTerritory.SalesTerritoryHistories.Contains(this))
                {
                    SalesTerritory.SalesTerritoryHistories.Add(this);
                }
                if (TerritoryID != SalesTerritory.TerritoryID)
                {
                    TerritoryID = SalesTerritory.TerritoryID;
                }
            }
        }
    }
}
