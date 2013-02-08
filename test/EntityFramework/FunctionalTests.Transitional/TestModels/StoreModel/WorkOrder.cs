// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class WorkOrder
    {
        public virtual int WorkOrderID { get; set; }

        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_productID != value)
                    {
                        if (Product != null
                            && Product.ProductID != value)
                        {
                            Product = null;
                        }
                        _productID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private int _productID;

        public virtual int OrderQty { get; set; }

        public virtual int StockedQty { get; set; }

        public virtual short ScrappedQty { get; set; }

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual DateTime DueDate { get; set; }

        public virtual short? ScrapReasonID
        {
            get { return _scrapReasonID; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_scrapReasonID != value)
                    {
                        if (ScrapReason != null
                            && ScrapReason.ScrapReasonID != value)
                        {
                            ScrapReason = null;
                        }
                        _scrapReasonID = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }

        private short? _scrapReasonID;

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Product Product
        {
            get { return _product; }
            set
            {
                if (!ReferenceEquals(_product, value))
                {
                    var previousValue = _product;
                    _product = value;
                    FixupProduct(previousValue);
                }
            }
        }

        private Product _product;

        public virtual ScrapReason ScrapReason
        {
            get { return _scrapReason; }
            set
            {
                if (!ReferenceEquals(_scrapReason, value))
                {
                    var previousValue = _scrapReason;
                    _scrapReason = value;
                    FixupScrapReason(previousValue);
                }
            }
        }

        private ScrapReason _scrapReason;

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

        private bool _settingFK;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.WorkOrders.Contains(this))
            {
                previousValue.WorkOrders.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.WorkOrders.Contains(this))
                {
                    Product.WorkOrders.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }

        private void FixupScrapReason(ScrapReason previousValue)
        {
            if (previousValue != null
                && previousValue.WorkOrders.Contains(this))
            {
                previousValue.WorkOrders.Remove(this);
            }

            if (ScrapReason != null)
            {
                if (!ScrapReason.WorkOrders.Contains(this))
                {
                    ScrapReason.WorkOrders.Add(this);
                }
                if (ScrapReasonID != ScrapReason.ScrapReasonID)
                {
                    ScrapReasonID = ScrapReason.ScrapReasonID;
                }
            }
            else if (!_settingFK)
            {
                ScrapReasonID = null;
            }
        }

        private void FixupWorkOrderRoutings(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WorkOrderRouting item in e.NewItems)
                {
                    item.WorkOrder = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (WorkOrderRouting item in e.OldItems)
                {
                    if (ReferenceEquals(item.WorkOrder, this))
                    {
                        item.WorkOrder = null;
                    }
                }
            }
        }
    }
}
