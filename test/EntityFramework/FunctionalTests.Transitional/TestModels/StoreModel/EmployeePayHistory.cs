// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class EmployeePayHistory
    {
        public virtual int EmployeeID
        {
            get { return _employeeID; }
            set
            {
                if (_employeeID != value)
                {
                    if (Employee != null
                        && Employee.EmployeeID != value)
                    {
                        Employee = null;
                    }
                    _employeeID = value;
                }
            }
        }

        private int _employeeID;

        public virtual DateTime RateChangeDate { get; set; }

        public virtual decimal Rate { get; set; }

        public virtual byte PayFrequency { get; set; }

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

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null
                && previousValue.EmployeePayHistories.Contains(this))
            {
                previousValue.EmployeePayHistories.Remove(this);
            }

            if (Employee != null)
            {
                if (!Employee.EmployeePayHistories.Contains(this))
                {
                    Employee.EmployeePayHistories.Add(this);
                }
                if (EmployeeID != Employee.EmployeeID)
                {
                    EmployeeID = Employee.EmployeeID;
                }
            }
        }
    }
}
