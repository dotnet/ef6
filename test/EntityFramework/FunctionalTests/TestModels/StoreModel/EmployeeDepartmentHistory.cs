// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;

    public class EmployeeDepartmentHistory
    {
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

        public virtual short DepartmentID
        {
            get { return _departmentID; }
            set
            {
                if (_departmentID != value)
                {
                    if (Department != null && Department.DepartmentID != value)
                    {
                        Department = null;
                    }
                    _departmentID = value;
                }
            }
        }
        private short _departmentID;

        public virtual byte ShiftID
        {
            get { return _shiftID; }
            set
            {
                if (_shiftID != value)
                {
                    if (Shift != null && Shift.ShiftID != value)
                    {
                        Shift = null;
                    }
                    _shiftID = value;
                }
            }
        }
        private byte _shiftID;

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Department Department
        {
            get { return _department; }
            set
            {
                if (!ReferenceEquals(_department, value))
                {
                    var previousValue = _department;
                    _department = value;
                    FixupDepartment(previousValue);
                }
            }
        }
        private Department _department;

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

        public virtual Shift Shift
        {
            get { return _shift; }
            set
            {
                if (!ReferenceEquals(_shift, value))
                {
                    var previousValue = _shift;
                    _shift = value;
                    FixupShift(previousValue);
                }
            }
        }
        private Shift _shift;

        private void FixupDepartment(Department previousValue)
        {
            if (previousValue != null && previousValue.EmployeeDepartmentHistories.Contains(this))
            {
                previousValue.EmployeeDepartmentHistories.Remove(this);
            }

            if (Department != null)
            {
                if (!Department.EmployeeDepartmentHistories.Contains(this))
                {
                    Department.EmployeeDepartmentHistories.Add(this);
                }
                if (DepartmentID != Department.DepartmentID)
                {
                    DepartmentID = Department.DepartmentID;
                }
            }
        }

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null && previousValue.EmployeeDepartmentHistories.Contains(this))
            {
                previousValue.EmployeeDepartmentHistories.Remove(this);
            }

            if (Employee != null)
            {
                if (!Employee.EmployeeDepartmentHistories.Contains(this))
                {
                    Employee.EmployeeDepartmentHistories.Add(this);
                }
                if (EmployeeID != Employee.EmployeeID)
                {
                    EmployeeID = Employee.EmployeeID;
                }
            }
        }

        private void FixupShift(Shift previousValue)
        {
            if (previousValue != null && previousValue.EmployeeDepartmentHistories.Contains(this))
            {
                previousValue.EmployeeDepartmentHistories.Remove(this);
            }

            if (Shift != null)
            {
                if (!Shift.EmployeeDepartmentHistories.Contains(this))
                {
                    Shift.EmployeeDepartmentHistories.Add(this);
                }
                if (ShiftID != Shift.ShiftID)
                {
                    ShiftID = Shift.ShiftID;
                }
            }
        }
    }
}