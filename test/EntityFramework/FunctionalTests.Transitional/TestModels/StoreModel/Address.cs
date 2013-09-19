// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Address
    {
        public virtual int AddressID { get; set; }

        public virtual string AddressLine1 { get; set; }

        public virtual string AddressLine2 { get; set; }

        public virtual string City { get; set; }

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

        public virtual string PostalCode { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<EmployeeAddress> EmployeeAddresses
        {
            get
            {
                if (_employeeAddresses == null)
                {
                    var newCollection = new FixupCollection<EmployeeAddress>();
                    newCollection.CollectionChanged += FixupEmployeeAddresses;
                    _employeeAddresses = newCollection;
                }
                return _employeeAddresses;
            }
            set
            {
                if (!ReferenceEquals(_employeeAddresses, value))
                {
                    var previousValue = _employeeAddresses as FixupCollection<EmployeeAddress>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmployeeAddresses;
                    }
                    _employeeAddresses = value;
                    var newValue = value as FixupCollection<EmployeeAddress>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmployeeAddresses;
                    }
                }
            }
        }

        private ICollection<EmployeeAddress> _employeeAddresses;

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

        public virtual ICollection<CustomerAddress> CustomerAddresses
        {
            get
            {
                if (_customerAddresses == null)
                {
                    var newCollection = new FixupCollection<CustomerAddress>();
                    newCollection.CollectionChanged += FixupCustomerAddresses;
                    _customerAddresses = newCollection;
                }
                return _customerAddresses;
            }
            set
            {
                if (!ReferenceEquals(_customerAddresses, value))
                {
                    var previousValue = _customerAddresses as FixupCollection<CustomerAddress>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCustomerAddresses;
                    }
                    _customerAddresses = value;
                    var newValue = value as FixupCollection<CustomerAddress>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCustomerAddresses;
                    }
                }
            }
        }

        private ICollection<CustomerAddress> _customerAddresses;

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

        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders1
        {
            get
            {
                if (_salesOrderHeaders1 == null)
                {
                    var newCollection = new FixupCollection<SalesOrderHeader>();
                    newCollection.CollectionChanged += FixupSalesOrderHeaders1;
                    _salesOrderHeaders1 = newCollection;
                }
                return _salesOrderHeaders1;
            }
            set
            {
                if (!ReferenceEquals(_salesOrderHeaders1, value))
                {
                    var previousValue = _salesOrderHeaders1 as FixupCollection<SalesOrderHeader>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesOrderHeaders1;
                    }
                    _salesOrderHeaders1 = value;
                    var newValue = value as FixupCollection<SalesOrderHeader>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesOrderHeaders1;
                    }
                }
            }
        }

        private ICollection<SalesOrderHeader> _salesOrderHeaders1;

        public virtual ICollection<VendorAddress> VendorAddresses
        {
            get
            {
                if (_vendorAddresses == null)
                {
                    var newCollection = new FixupCollection<VendorAddress>();
                    newCollection.CollectionChanged += FixupVendorAddresses;
                    _vendorAddresses = newCollection;
                }
                return _vendorAddresses;
            }
            set
            {
                if (!ReferenceEquals(_vendorAddresses, value))
                {
                    var previousValue = _vendorAddresses as FixupCollection<VendorAddress>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupVendorAddresses;
                    }
                    _vendorAddresses = value;
                    var newValue = value as FixupCollection<VendorAddress>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupVendorAddresses;
                    }
                }
            }
        }

        private ICollection<VendorAddress> _vendorAddresses;

        private void FixupStateProvince(StateProvince previousValue)
        {
            if (previousValue != null
                && previousValue.Addresses.Contains(this))
            {
                previousValue.Addresses.Remove(this);
            }

            if (StateProvince != null)
            {
                if (!StateProvince.Addresses.Contains(this))
                {
                    StateProvince.Addresses.Add(this);
                }
                if (StateProvinceID != StateProvince.StateProvinceID)
                {
                    StateProvinceID = StateProvince.StateProvinceID;
                }
            }
        }

        private void FixupEmployeeAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeeAddress item in e.NewItems)
                {
                    item.Address = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EmployeeAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Address, this))
                    {
                        item.Address = null;
                    }
                }
            }
        }

        private void FixupCustomerAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CustomerAddress item in e.NewItems)
                {
                    item.Address = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CustomerAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Address, this))
                    {
                        item.Address = null;
                    }
                }
            }
        }

        private void FixupSalesOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderHeader item in e.NewItems)
                {
                    item.Address = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Address, this))
                    {
                        item.Address = null;
                    }
                }
            }
        }

        private void FixupSalesOrderHeaders1(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderHeader item in e.NewItems)
                {
                    item.Address1 = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Address1, this))
                    {
                        item.Address1 = null;
                    }
                }
            }
        }

        private void FixupVendorAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (VendorAddress item in e.NewItems)
                {
                    item.Address = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Address, this))
                    {
                        item.Address = null;
                    }
                }
            }
        }
    }
}
