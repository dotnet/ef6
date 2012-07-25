// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class PurchaseOrderHeader
    {
        public virtual int PurchaseOrderID { get; set; }

        public virtual byte RevisionNumber { get; set; }

        public virtual byte Status { get; set; }

        public virtual int EmployeeID
        {
            get { return _employeeID; }
            set
            {
                if (_employeeID != value)
                {
                    if (Employee != null && Employee.EmployeeID != value)
                    {
                        Employee = null;
                    }
                    _employeeID = value;
                }
            }
        }
        private int _employeeID;

        public virtual int VendorID
        {
            get { return _vendorID; }
            set
            {
                if (_vendorID != value)
                {
                    if (Vendor != null && Vendor.VendorID != value)
                    {
                        Vendor = null;
                    }
                    _vendorID = value;
                }
            }
        }
        private int _vendorID;

        public virtual int ShipMethodID
        {
            get { return _shipMethodID; }
            set
            {
                if (_shipMethodID != value)
                {
                    if (ShipMethod != null && ShipMethod.ShipMethodID != value)
                    {
                        ShipMethod = null;
                    }
                    _shipMethodID = value;
                }
            }
        }
        private int _shipMethodID;

        public virtual DateTime OrderDate { get; set; }

        public virtual DateTime? ShipDate { get; set; }

        public virtual decimal SubTotal { get; set; }

        public virtual decimal TaxAmt { get; set; }

        public virtual decimal Freight { get; set; }

        public virtual decimal TotalDue { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Employee Employee
        {
            get { return _employee; }
            set
            {
                if (!ReferenceEquals(_employee, value))
                {
                    var previousValue = _employee;
                    _employee = value;
                    FixupEmployee(previousValue);
                }
            }
        }
        private Employee _employee;

        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails
        {
            get
            {
                if (_purchaseOrderDetails == null)
                {
                    var newCollection = new FixupCollection<PurchaseOrderDetail>();
                    newCollection.CollectionChanged += FixupPurchaseOrderDetails;
                    _purchaseOrderDetails = newCollection;
                }
                return _purchaseOrderDetails;
            }
            set
            {
                if (!ReferenceEquals(_purchaseOrderDetails, value))
                {
                    var previousValue = _purchaseOrderDetails as FixupCollection<PurchaseOrderDetail>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupPurchaseOrderDetails;
                    }
                    _purchaseOrderDetails = value;
                    var newValue = value as FixupCollection<PurchaseOrderDetail>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupPurchaseOrderDetails;
                    }
                }
            }
        }
        private ICollection<PurchaseOrderDetail> _purchaseOrderDetails;

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

        public virtual Vendor Vendor
        {
            get { return _vendor; }
            set
            {
                if (!ReferenceEquals(_vendor, value))
                {
                    var previousValue = _vendor;
                    _vendor = value;
                    FixupVendor(previousValue);
                }
            }
        }
        private Vendor _vendor;

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null && previousValue.PurchaseOrderHeaders.Contains(this))
            {
                previousValue.PurchaseOrderHeaders.Remove(this);
            }

            if (Employee != null)
            {
                if (!Employee.PurchaseOrderHeaders.Contains(this))
                {
                    Employee.PurchaseOrderHeaders.Add(this);
                }
                if (EmployeeID != Employee.EmployeeID)
                {
                    EmployeeID = Employee.EmployeeID;
                }
            }
        }

        private void FixupShipMethod(ShipMethod previousValue)
        {
            if (previousValue != null && previousValue.PurchaseOrderHeaders.Contains(this))
            {
                previousValue.PurchaseOrderHeaders.Remove(this);
            }

            if (ShipMethod != null)
            {
                if (!ShipMethod.PurchaseOrderHeaders.Contains(this))
                {
                    ShipMethod.PurchaseOrderHeaders.Add(this);
                }
                if (ShipMethodID != ShipMethod.ShipMethodID)
                {
                    ShipMethodID = ShipMethod.ShipMethodID;
                }
            }
        }

        private void FixupVendor(Vendor previousValue)
        {
            if (previousValue != null && previousValue.PurchaseOrderHeaders.Contains(this))
            {
                previousValue.PurchaseOrderHeaders.Remove(this);
            }

            if (Vendor != null)
            {
                if (!Vendor.PurchaseOrderHeaders.Contains(this))
                {
                    Vendor.PurchaseOrderHeaders.Add(this);
                }
                if (VendorID != Vendor.VendorID)
                {
                    VendorID = Vendor.VendorID;
                }
            }
        }

        private void FixupPurchaseOrderDetails(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PurchaseOrderDetail item in e.NewItems)
                {
                    item.PurchaseOrderHeader = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PurchaseOrderDetail item in e.OldItems)
                {
                    if (ReferenceEquals(item.PurchaseOrderHeader, this))
                    {
                        item.PurchaseOrderHeader = null;
                    }
                }
            }
        }
    }
}