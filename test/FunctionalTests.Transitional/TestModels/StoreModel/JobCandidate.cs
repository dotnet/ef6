// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class JobCandidate
    {
        public virtual int JobCandidateID { get; set; }

        public virtual int? EmployeeID
        {
            get { return _employeeID; }
            set
            {
                try
                {
                    _settingFK = true;
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
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int? _employeeID;

        public virtual string Resume { get; set; }

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

        private bool _settingFK;

        private void FixupEmployee(Employee previousValue)
        {
            if (previousValue != null
                && previousValue.JobCandidates.Contains(this))
            {
                previousValue.JobCandidates.Remove(this);
            }

            if (Employee != null)
            {
                if (!Employee.JobCandidates.Contains(this))
                {
                    Employee.JobCandidates.Add(this);
                }
                if (EmployeeID != Employee.EmployeeID)
                {
                    EmployeeID = Employee.EmployeeID;
                }
            }
            else if (!_settingFK)
            {
                EmployeeID = null;
            }
        }
    }
}
