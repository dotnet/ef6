// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class ShipMethod
    {
        public virtual int ShipMethodID { get; set; }

        public virtual string Name { get; set; }

        public virtual decimal ShipBase { get; set; }

        public virtual decimal ShipRate { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

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

        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders
        {
            get
            {
                if (_salesOrderHeaders == null)
                {
                    var newCollection = new FixupCollection<SalesOrderHeader>();
                    newCollection.CollectionChanged += FixupSalesOrderHeaders;
                    _salesOrderHeaders = newCollection;
                }
                return _salesOrderHeaders;
            }
            set
            {
                if (!ReferenceEquals(_salesOrderHeaders, value))
                {
                    var previousValue = _salesOrderHeaders as FixupCollection<SalesOrderHeader>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSalesOrderHeaders;
                    }
                    _salesOrderHeaders = value;
                    var newValue = value as FixupCollection<SalesOrderHeader>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSalesOrderHeaders;
                    }
                }
            }
        }

        private ICollection<SalesOrderHeader> _salesOrderHeaders;

        private void FixupPurchaseOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PurchaseOrderHeader item in e.NewItems)
                {
                    item.ShipMethod = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PurchaseOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.ShipMethod, this))
                    {
                        item.ShipMethod = null;
                    }
                }
            }
        }

        private void FixupSalesOrderHeaders(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SalesOrderHeader item in e.NewItems)
                {
                    item.ShipMethod = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.ShipMethod, this))
                    {
                        item.ShipMethod = null;
                    }
                }
            }
        }
    }
}
