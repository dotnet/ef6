// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Shift
    {
        public virtual byte ShiftID { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime StartTime { get; set; }

        public virtual DateTime EndTime { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

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

        private void FixupEmployeeDepartmentHistories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmployeeDepartmentHistory item in e.NewItems)
                {
                    item.Shift = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EmployeeDepartmentHistory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Shift, this))
                    {
                        item.Shift = null;
                    }
                }
            }
        }
    }
}