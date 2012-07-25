// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Location
    {
        public virtual short? LocationID { get; set; }

        public virtual string Name { get; set; }

        public virtual decimal CostRate { get; set; }

        public virtual decimal Availability { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ProductInventory> ProductInventories
        {
            get
            {
                if (_productInventories == null)
                {
                    var newCollection = new FixupCollection<ProductInventory>();
                    newCollection.CollectionChanged += FixupProductInventories;
                    _productInventories = newCollection;
                }
                return _productInventories;
            }
            set
            {
                if (!ReferenceEquals(_productInventories, value))
                {
                    var previousValue = _productInventories as FixupCollection<ProductInventory>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProductInventories;
                    }
                    _productInventories = value;
                    var newValue = value as FixupCollection<ProductInventory>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProductInventories;
                    }
                }
            }
        }
        private ICollection<ProductInventory> _productInventories;

        public virtual ICollection<WorkOrderRouting> WorkOrderRoutings
        {
            get
            {
                if (_workOrderRoutings == null)
                {
                    var newCollection = new FixupCollection<WorkOrderRouting>();
                    newCollection.CollectionChanged += FixupWorkOrderRoutings;
                    _workOrderRoutings = newCollection;
                }
                return _workOrderRoutings;
            }
            set
            {
                if (!ReferenceEquals(_workOrderRoutings, value))
                {
                    var previousValue = _workOrderRoutings as FixupCollection<WorkOrderRouting>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupWorkOrderRoutings;
                    }
                    _workOrderRoutings = value;
                    var newValue = value as FixupCollection<WorkOrderRouting>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupWorkOrderRoutings;
                    }
                }
            }
        }
        private ICollection<WorkOrderRouting> _workOrderRoutings;

        private void FixupProductInventories(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductInventory item in e.NewItems)
                {
                    item.Location = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductInventory item in e.OldItems)
                {
                    if (ReferenceEquals(item.Location, this))
                    {
                        item.Location = null;
                    }
                }
            }
        }

        private void FixupWorkOrderRoutings(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WorkOrderRouting item in e.NewItems)
                {
                    item.Location = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (WorkOrderRouting item in e.OldItems)
                {
                    if (ReferenceEquals(item.Location, this))
                    {
                        item.Location = null;
                    }
                }
            }
        }
    }
}