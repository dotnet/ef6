namespace FunctionalTests.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class EmployeeAddress
    {
        [Key]
        public virtual int EmployeeID
        {
            get { return _employeeID; }
            set
            {
                if (_employeeID != value)
                {
//                    if (Employee != null && Employee.EmployeeID != value)
//                    {
//                        Employee = null;
//                    }
                    _employeeID = value;
                }
            }
        }
        private int _employeeID;

        [Key]
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

        public virtual Guid rowguid { get; set; }

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

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null && previousValue.EmployeeAddresses.Contains(this))
            {
                previousValue.EmployeeAddresses.Remove(this);
            }

            if (Employee != null)
            {
                if (!Employee.EmployeeAddresses.Contains(this))
                {
                    Employee.EmployeeAddresses.Add(this);
                }
                if (EmployeeID != Employee.EmployeeID)
                {
                    EmployeeID = Employee.EmployeeID;
                }
            }
        }

        private void FixupAddress(Address previousValue)
        {
            if (previousValue != null && previousValue.EmployeeAddresses.Contains(this))
            {
                previousValue.EmployeeAddresses.Remove(this);
            }

            if (Address != null)
            {
                if (!Address.EmployeeAddresses.Contains(this))
                {
                    Address.EmployeeAddresses.Add(this);
                }
                if (AddressID != Address.AddressID)
                {
                    AddressID = Address.AddressID;
                }
            }
        }
    }
}