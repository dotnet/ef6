// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Employee
    {
        public virtual int EmployeeID { get; set; }

        public virtual string NationalIDNumber { get; set; }

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
                        if (Contact != null
                            && Contact.ContactID != value)
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

        public virtual string LoginID { get; set; }

        public virtual int? ManagerID
        {
            get { return _managerID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_managerID != value)
                    {
                        if (Manager != null
                            && Manager.EmployeeID != value)
                        {
                            Manager = null;
                        }
                        _managerID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int? _managerID;

        public virtual string Title { get; set; }

        public virtual DateTime BirthDate { get; set; }

        public virtual string MaritalStatus { get; set; }

        public virtual string Gender { get; set; }

        public virtual DateTime HireDate { get; set; }

        public virtual bool SalariedFlag { get; set; }

        public virtual short VacationHours { get; set; }

        public virtual short SickLeaveHours { get; set; }

        public virtual bool CurrentFlag { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

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

        public virtual ICollection<Employee> Employees
        {
            get
            {
                if (_employees == null)
                {
                    var newCollection = new FixupCollection<Employee>();
                    newCollection.CollectionChanged += FixupEmployee1;
                    _employees = newCollection;
                }
                return _employees;
            }
            set
            {
                if (!ReferenceEquals(_employees, value))
                {
                    var previousValue = _employees as FixupCollection<Employee>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmployee1;
                    }
                    _employees = value;
                    var newValue = value as FixupCollection<Employee>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmployee1;
                    }
                }
            }
        }

        private ICollection<Employee> _employees;

        [ForeignKey("ManagerID")]
        public virtual Employee Manager
        {
            get { return _manager; }
            set
            {
                if (!ReferenceEquals(_manager, value))
                {
                    var previousValue = _manager;
                    _manager = value;
                    FixupEmployee2(previousValue);
                }
            }
        }

        private Employee _manager;

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

        public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories
        {
            get
            {
                if (_employeeDepartmentHistories == null)
                {
                    var newCollection = new FixupCollection<EmployeeDepartmentHistory>();
                    newCollection.CollectionChanged += FixupEmployeeDepartmentHistories;
                    _employeeDepartmentHistories = newCollection;
                }
                return _employeeDepartmentHistories;
            }
            set
            {
                if (!ReferenceEquals(_employeeDepartmentHistories, value))
                {
                    var previousValue = _employeeDepartmentHistories as FixupCollection<EmployeeDepartmentHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmployeeDepartmentHistories;
                    }
                    _employeeDepartmentHistories = value;
                    var newValue = value as FixupCollection<EmployeeDepartmentHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmployeeDepartmentHistories;
                    }
                }
            }
        }

        private ICollection<EmployeeDepartmentHistory> _employeeDepartmentHistories;

        public virtual ICollection<EmployeePayHistory> EmployeePayHistories
        {
            get
            {
                if (_employeePayHistories == null)
                {
                    var newCollection = new FixupCollection<EmployeePayHistory>();
                    newCollection.CollectionChanged += FixupEmployeePayHistories;
                    _employeePayHistories = newCollection;
                }
                return _employeePayHistories;
            }
            set
            {
                if (!ReferenceEquals(_employeePayHistories, value))
                {
                    var previousValue = _employeePayHistories as FixupCollection<EmployeePayHistory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmployeePayHistories;
                    }
                    _employeePayHistories = value;
                    var newValue = value as FixupCollection<EmployeePayHistory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmployeePayHistories;
                    }
                }
            }
        }

        private ICollection<EmployeePayHistory> _employeePayHistories;

        public virtual ICollection<JobCandidate> JobCandidates
        {
            get
            {
                if (_jobCandidates == null)
                {
                    var newCollection = new FixupCollection<JobCandidate>();
                    newCollection.CollectionChanged += FixupJobCandidates;
                    _jobCandidates = newCollection;
                }
                return _jobCandidates;
            }
            set
            {
                if (!ReferenceEquals(_jobCandidates, value))
                {
                    var previousValue = _jobCandidates as FixupCollection<JobCandidate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupJobCandidates;
                    }
                    _jobCandidates = value;
                    var newValue = value as FixupCollection<JobCandidate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupJobCandidates;
                    }
                }
            }
        }

        private ICollection<JobCandidate> _jobCandidates;

        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders
        {
            get
            {
                if (_purchaseOrderHeaders == null)
                {
                    var newCollection = new FixupCollection<PurchaseOrderHeader>();
                    newCollection.CollectionChanged += FixupPurchaseOrderHeaders;
                    _purchaseOrderHeaders = newCollection;
                }
                return _purchaseOrderHeaders;
            }
            set
            {
                if (!ReferenceEquals(_purchaseOrderHeaders, value))
                {
                    var previousValue = _purchaseOrderHeaders as FixupCollection<PurchaseOrderHeader>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupPurchaseOrderHeaders;
                    }
                    _purchaseOrderHeaders = value;
                    var newValue = value as FixupCollection<PurchaseOrderHeader>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupPurchaseOrderHeaders;
                    }
                }
            }
        }

        private ICollection<PurchaseOrderHeader> _purchaseOrderHeaders;

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

        private bool _settingFK;

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null
                && previousValue.Employees.Contains(this))
            {
                previousValue.Employees.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.Employees.Contains(this))
                {
                    Contact.Employees.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupEmployee2(Employee previousValue)
        {
            if (previousValue != null
                && previousValue.Employees.Contains(this))
            {
                previousValue.Employees.Remove(this);
            }

            if (Manager != null)
            {
                if (!Manager.Employees.Contains(this))
                {
                    Manager.Employees.Add(this);
                }
                if (ManagerID != Manager.EmployeeID)
                {
                    ManagerID = Manager.EmployeeID;
                }
            }
            else if (!_settingFK)
            {
                ManagerID = null;
            }
        }

        private void FixupSalesPerson(SalesPerson previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.Employee, this))
            {
                previousValue.Employee = null;
            }

            if (SalesPerson != null)
            {
                SalesPerson.Employee = this;
            }
        }

        private void FixupEmployee1(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Employee item in e.NewItems)
                {
                    item.Manager = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Employee item in e.OldItems)
                {
                    if (ReferenceEquals(item.Manager, this))
                    {
                        item.Manager = null;
                    }
                }
            }
        }

        private void FixupEmployeeAddresses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeeAddress item in e.NewItems)
                {
                    item.Employee = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EmployeeAddress item in e.OldItems)
                {
                    if (ReferenceEquals(item.Employee, this))
                    {
                        item.Employee = null;
                    }
                }
            }
        }

        private void FixupEmployeeDepartmentHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeeDepartmentHistory item in e.NewItems)
                {
                    item.Employee = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EmployeeDepartmentHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Employee, this))
                    {
                        item.Employee = null;
                    }
                }
            }
        }

        private void FixupEmployeePayHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeePayHistory item in e.NewItems)
                {
                    item.Employee = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EmployeePayHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Employee, this))
                    {
                        item.Employee = null;
                    }
                }
            }
        }

        private void FixupJobCandidates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (JobCandidate item in e.NewItems)
                {
                    item.Employee = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (JobCandidate item in e.OldItems)
                {
                    if (ReferenceEquals(item.Employee, this))
                    {
                        item.Employee = null;
                    }
                }
            }
        }

        private void FixupPurchaseOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PurchaseOrderHeader item in e.NewItems)
                {
                    item.Employee = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PurchaseOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Employee, this))
                    {
                        item.Employee = null;
                    }
                }
            }
        }
    }
}
