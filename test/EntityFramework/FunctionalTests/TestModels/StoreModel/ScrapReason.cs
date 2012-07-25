// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class ScrapReason
    {
        public virtual short ScrapReasonID { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<WorkOrder> WorkOrders
        {
            get
            {
                if (_workOrders == null)
                {
                    var newCollection = new FixupCollection<WorkOrder>();
                    newCollection.CollectionChanged += FixupWorkOrders;
                    _workOrders = newCollection;
                }
                return _workOrders;
            }
            set
            {
                if (!ReferenceEquals(_workOrders, value))
                {
                    var previousValue = _workOrders as FixupCollection<WorkOrder>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupWorkOrders;
                    }
                    _workOrders = value;
                    var newValue = value as FixupCollection<WorkOrder>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupWorkOrders;
                    }
                }
            }
        }
        private ICollection<WorkOrder> _workOrders;

        private void FixupWorkOrders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WorkOrder item in e.NewItems)
                {
                    item.ScrapReason = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (WorkOrder item in e.OldItems)
                {
                    if (ReferenceEquals(item.ScrapReason, this))
                    {
                        item.ScrapReason = null;
                    }
                }
            }
        }
    }
}