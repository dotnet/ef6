// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;

    public class CustomerAddress
    {
        public virtual int CustomerID
        {
            get { return _customerID; }
            set
            {
                if (_customerID != value)
                {
                    if (Customer != null && Customer.CustomerID != value)
                    {
                        Customer = null;
                    }
                    _customerID = value;
                }
            }
        }
        private int _customerID;

        public virtual int AddressID
        {
            get { return _addressID; }
            set
            {
                if (_addressID != value)
                {
                    if (Address != null && Address.AddressID != value)
                    {
                        Address = null;
                    }
                    _addressID = value;
                }
            }
        }
        private int _addressID;

        public virtual int AddressTypeID
        {
            get { return _addressTypeID; }
            set
            {
                if (_addressTypeID != value)
                {
                    if (AddressType != null && AddressType.AddressTypeID != value)
                    {
                        AddressType = null;
                    }
                    _addressTypeID = value;
                }
            }
        }
        private int _addressTypeID;

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

        public virtual AddressType AddressType
        {
            get { return _addressType; }
            set
            {
                if (!ReferenceEquals(_addressType, value))
                {
                    var previousValue = _addressType;
                    _addressType = value;
                    FixupAddressType(previousValue);
                }
            }
        }
        private AddressType _addressType;

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

        private void FixupAddress(Address previousValue)
        {
            if (previousValue != null && previousValue.CustomerAddresses.Contains(this))
            {
                previousValue.CustomerAddresses.Remove(this);
            }

            if (Address != null)
            {
                if (!Address.CustomerAddresses.Contains(this))
                {
                    Address.CustomerAddresses.Add(this);
                }
                if (AddressID != Address.AddressID)
                {
                    AddressID = Address.AddressID;
                }
            }
        }

        private void FixupAddressType(AddressType previousValue)
        {
            if (previousValue != null && previousValue.CustomerAddresses.Contains(this))
            {
                previousValue.CustomerAddresses.Remove(this);
            }

            if (AddressType != null)
            {
                if (!AddressType.CustomerAddresses.Contains(this))
                {
                    AddressType.CustomerAddresses.Add(this);
                }
                if (AddressTypeID != AddressType.AddressTypeID)
                {
                    AddressTypeID = AddressType.AddressTypeID;
                }
            }
        }

        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null && previousValue.CustomerAddresses.Contains(this))
            {
                previousValue.CustomerAddresses.Remove(this);
            }

            if (Customer != null)
            {
                if (!Customer.CustomerAddresses.Contains(this))
                {
                    Customer.CustomerAddresses.Add(this);
                }
                if (CustomerID != Customer.CustomerID)
                {
                    CustomerID = Customer.CustomerID;
                }
            }
        }
    }
}