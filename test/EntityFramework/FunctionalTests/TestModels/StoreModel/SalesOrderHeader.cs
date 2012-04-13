namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;

    public class SalesOrderHeader
    {
        [Key]
        public virtual int SalesOrderID { get; set; }

        public virtual byte RevisionNumber { get; set; }

        public virtual DateTime OrderDate { get; set; }

        public virtual DateTime DueDate { get; set; }

        public virtual DateTime? ShipDate { get; set; }

        public virtual byte Status { get; set; }

        public virtual bool OnlineOrderFlag { get; set; }

        public virtual string SalesOrderNumber { get; set; }

        public virtual string PurchaseOrderNumber { get; set; }

        public virtual string AccountNumber { get; set; }

        public virtual int CustomerID
        {
            get { return _customerID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_customerID != value)
                    {
                        if (Customer != null && Customer.CustomerID != value)
                        {
                            Customer = null;
                        }
                        _customerID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _customerID;

        public virtual int ContactID
        {
            get { return _contactID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_contactID != value)
                    {
                        if (Contact != null && Contact.ContactID != value)
                        {
                            Contact = null;
                        }
                        _contactID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _contactID;

        public virtual int? SalesPersonID
        {
            get { return _salesPersonID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_salesPersonID != value)
                    {
                        if (SalesPerson != null && SalesPerson.SalesPersonID != value)
                        {
                            SalesPerson = null;
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
        private int? _salesPersonID;

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
                        if (SalesTerritory != null && SalesTerritory.TerritoryID != value)
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

        public virtual int BillToAddressID
        {
            get { return _billToAddressID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_billToAddressID != value)
                    {
                        if (Address != null && Address.AddressID != value)
                        {
                            Address = null;
                        }
                        _billToAddressID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _billToAddressID;

        public virtual int ShipToAddressID
        {
            get { return _shipToAddressID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_shipToAddressID != value)
                    {
                        if (Address1 != null && Address1.AddressID != value)
                        {
                            Address1 = null;
                        }
                        _shipToAddressID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _shipToAddressID;

        public virtual int ShipMethodID
        {
            get { return _shipMethodID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_shipMethodID != value)
                    {
                        if (ShipMethod != null && ShipMethod.ShipMethodID != value)
                        {
                            ShipMethod = null;
                        }
                        _shipMethodID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _shipMethodID;

        public virtual int? CreditCardID
        {
            get { return _creditCardID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_creditCardID != value)
                    {
                        if (CreditCard != null && CreditCard.CreditCardID != value)
                        {
                            CreditCard = null;
                        }
                        _creditCardID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int? _creditCardID;

        public virtual string CreditCardApprovalCode { get; set; }

        public virtual int? CurrencyRateID
        {
            get { return _currencyRateID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_currencyRateID != value)
                    {
                        if (CurrencyRate != null && CurrencyRate.CurrencyRateID != value)
                        {
                            CurrencyRate = null;
                        }
                        _currencyRateID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int? _currencyRateID;

        public virtual decimal SubTotal { get; set; }

        public virtual decimal TaxAmt { get; set; }

        public virtual decimal Freight { get; set; }

        public virtual decimal TotalDue { get; set; }

        public virtual string Comment { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Address Address
        {
            get { return _address; }
            set
            {
                if (!ReferenceEquals(_address, value))
                {
                    var previousValue = _address;
                    _address = value;
                    FixupAddress(previousValue);
                }
            }
        }
        private Address _address;

        public virtual Address Address1
        {
            get { return _address1; }
            set
            {
                if (!ReferenceEquals(_address1, value))
                {
                    var previousValue = _address1;
                    _address1 = value;
                    FixupAddress1(previousValue);
                }
            }
        }
        private Address _address1;

        public virtual Contact Contact
        {
            get { return _contact; }
            set
            {
                if (!ReferenceEquals(_contact, value))
                {
                    var previousValue = _contact;
                    _contact = value;
                    FixupContact(previousValue);
                }
            }
        }
        private Contact _contact;

        public virtual ShipMethod ShipMethod
        {
            get { return _shipMethod; }
            set
            {
                if (!ReferenceEquals(_shipMethod, value))
                {
                    var previousValue = _shipMethod;
                    _shipMethod = value;
                    FixupShipMethod(previousValue);
                }
            }
        }
        private ShipMethod _shipMethod;

        public virtual CreditCard CreditCard
        {
            get { return _creditCard; }
            set
            {
                if (!ReferenceEquals(_creditCard, value))
                {
                    var previousValue = _creditCard;
                    _creditCard = value;
                    FixupCreditCard(previousValue);
                }
            }
        }
        private CreditCard _creditCard;

        public virtual CurrencyRate CurrencyRate
        {
            get { return _currencyRate; }
            set
            {
                if (!ReferenceEquals(_currencyRate, value))
                {
                    var previousValue = _currencyRate;
                    _currencyRate = value;
                    FixupCurrencyRate(previousValue);
                }
            }
        }
        private CurrencyRate _currencyRate;

        public virtual Customer Customer
        {
            get { return _customer; }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer;
                    _customer = value;
                    FixupCustomer(previousValue);
                }
            }
        }
        private Customer _customer;

        public virtual ICollection<SalesOrderDetail> SalesOrderDetails
        {
            get
            {
                if (_salesOrderDetails == null)
                {
                    var newCollection = new FixupCollection<SalesOrderDetail>();
                    newCollection.CollectionChanged += FixupSalesOrderDetails;
                    _salesOrderDetails = newCollection;
                }
                return _salesOrderDetails;
            }
            set
            {
                if (!ReferenceEquals(_salesOrderDetails, value))
                {
                    var previousValue = _salesOrderDetails as FixupCollection<SalesOrderDetail>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesOrderDetails;
                    }
                    _salesOrderDetails = value;
                    var newValue = value as FixupCollection<SalesOrderDetail>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesOrderDetails;
                    }
                }
            }
        }
        private ICollection<SalesOrderDetail> _salesOrderDetails;

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

        public virtual ICollection<SalesReason> SalesReasons { get; set; }

        private bool _settingFK;

        private void FixupAddress(Address previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (Address != null)
            {
                if (!Address.SalesOrderHeaders.Contains(this))
                {
                    Address.SalesOrderHeaders.Add(this);
                }
                if (BillToAddressID != Address.AddressID)
                {
                    BillToAddressID = Address.AddressID;
                }
            }
        }

        private void FixupAddress1(Address previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders1.Contains(this))
            {
                previousValue.SalesOrderHeaders1.Remove(this);
            }

            if (Address1 != null)
            {
                if (!Address1.SalesOrderHeaders1.Contains(this))
                {
                    Address1.SalesOrderHeaders1.Add(this);
                }
                if (ShipToAddressID != Address1.AddressID)
                {
                    ShipToAddressID = Address1.AddressID;
                }
            }
        }

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.SalesOrderHeaders.Contains(this))
                {
                    Contact.SalesOrderHeaders.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupShipMethod(ShipMethod previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (ShipMethod != null)
            {
                if (!ShipMethod.SalesOrderHeaders.Contains(this))
                {
                    ShipMethod.SalesOrderHeaders.Add(this);
                }
                if (ShipMethodID != ShipMethod.ShipMethodID)
                {
                    ShipMethodID = ShipMethod.ShipMethodID;
                }
            }
        }

        private void FixupCreditCard(CreditCard previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (CreditCard != null)
            {
                if (!CreditCard.SalesOrderHeaders.Contains(this))
                {
                    CreditCard.SalesOrderHeaders.Add(this);
                }
                if (CreditCardID != CreditCard.CreditCardID)
                {
                    CreditCardID = CreditCard.CreditCardID;
                }
            }
            else if (!_settingFK)
            {
                CreditCardID = null;
            }
        }

        private void FixupCurrencyRate(CurrencyRate previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (CurrencyRate != null)
            {
                if (!CurrencyRate.SalesOrderHeaders.Contains(this))
                {
                    CurrencyRate.SalesOrderHeaders.Add(this);
                }
                if (CurrencyRateID != CurrencyRate.CurrencyRateID)
                {
                    CurrencyRateID = CurrencyRate.CurrencyRateID;
                }
            }
            else if (!_settingFK)
            {
                CurrencyRateID = null;
            }
        }

        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (Customer != null)
            {
                if (!Customer.SalesOrderHeaders.Contains(this))
                {
                    Customer.SalesOrderHeaders.Add(this);
                }
                if (CustomerID != Customer.CustomerID)
                {
                    CustomerID = Customer.CustomerID;
                }
            }
        }

        private void FixupSalesPerson(SalesPerson previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (SalesPerson != null)
            {
                if (!SalesPerson.SalesOrderHeaders.Contains(this))
                {
                    SalesPerson.SalesOrderHeaders.Add(this);
                }
                if (SalesPersonID != SalesPerson.SalesPersonID)
                {
                    SalesPersonID = SalesPerson.SalesPersonID;
                }
            }
            else if (!_settingFK)
            {
                SalesPersonID = null;
            }
        }

        private void FixupSalesTerritory(SalesTerritory previousValue)
        {
            if (previousValue != null && previousValue.SalesOrderHeaders.Contains(this))
            {
                previousValue.SalesOrderHeaders.Remove(this);
            }

            if (SalesTerritory != null)
            {
                if (!SalesTerritory.SalesOrderHeaders.Contains(this))
                {
                    SalesTerritory.SalesOrderHeaders.Add(this);
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

        private void FixupSalesOrderDetails(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderDetail item in e.NewItems)
                {
                    item.SalesOrderHeader = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderDetail item in e.OldItems)
                {
                    if (ReferenceEquals(item.SalesOrderHeader, this))
                    {
                        item.SalesOrderHeader = null;
                    }
                }
            }
        }
    }
}