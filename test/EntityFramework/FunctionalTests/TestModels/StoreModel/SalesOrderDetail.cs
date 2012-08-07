// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class SalesOrderDetail
    {
        public virtual int SalesOrderID
        {
            get { return _salesOrderID; }
            set
            {
                if (_salesOrderID != value)
                {
                    if (SalesOrderHeader != null
                        && SalesOrderHeader.SalesOrderID != value)
                    {
                        SalesOrderHeader = null;
                    }
                    _salesOrderID = value;
                }
            }
        }

        private int _salesOrderID;

        public virtual int SalesOrderDetailID { get; set; }

        public virtual string CarrierTrackingNumber { get; set; }

        public virtual short OrderQty { get; set; }

        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
                if (_productID != value)
                {
                    if (SpecialOfferProduct != null
                        && SpecialOfferProduct.ProductID != value)
                    {
                        SpecialOfferProduct = null;
                    }
                    _productID = value;
                }
            }
        }

        private int _productID;

        public virtual int SpecialOfferID
        {
            get { return _specialOfferID; }
            set
            {
                if (_specialOfferID != value)
                {
                    if (SpecialOfferProduct != null
                        && SpecialOfferProduct.SpecialOfferID != value)
                    {
                        SpecialOfferProduct = null;
                    }
                    _specialOfferID = value;
                }
            }
        }

        private int _specialOfferID;

        public virtual decimal UnitPrice { get; set; }

        public virtual decimal UnitPriceDiscount { get; set; }

        public virtual decimal LineTotal { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual SalesOrderHeader SalesOrderHeader
        {
            get { return _salesOrderHeader; }
            set
            {
                if (!ReferenceEquals(_salesOrderHeader, value))
                {
                    var previousValue = _salesOrderHeader;
                    _salesOrderHeader = value;
                    FixupSalesOrderHeader(previousValue);
                }
            }
        }

        private SalesOrderHeader _salesOrderHeader;

        public virtual SpecialOfferProduct SpecialOfferProduct
        {
            get { return _specialOfferProduct; }
            set
            {
                if (!ReferenceEquals(_specialOfferProduct, value))
                {
                    var previousValue = _specialOfferProduct;
                    _specialOfferProduct = value;
                    FixupSpecialOfferProduct(previousValue);
                }
            }
        }

        private SpecialOfferProduct _specialOfferProduct;

        private void FixupSalesOrderHeader(SalesOrderHeader previousValue)
        {
            if (previousValue != null
                && previousValue.SalesOrderDetails.Contains(this))
            {
                previousValue.SalesOrderDetails.Remove(this);
            }

            if (SalesOrderHeader != null)
            {
                if (!SalesOrderHeader.SalesOrderDetails.Contains(this))
                {
                    SalesOrderHeader.SalesOrderDetails.Add(this);
                }
                if (SalesOrderID != SalesOrderHeader.SalesOrderID)
                {
                    SalesOrderID = SalesOrderHeader.SalesOrderID;
                }
            }
        }

        private void FixupSpecialOfferProduct(SpecialOfferProduct previousValue)
        {
            if (previousValue != null
                && previousValue.SalesOrderDetails.Contains(this))
            {
                previousValue.SalesOrderDetails.Remove(this);
            }

            if (SpecialOfferProduct != null)
            {
                if (!SpecialOfferProduct.SalesOrderDetails.Contains(this))
                {
                    SpecialOfferProduct.SalesOrderDetails.Add(this);
                }
                if (SpecialOfferID != SpecialOfferProduct.SpecialOfferID)
                {
                    SpecialOfferID = SpecialOfferProduct.SpecialOfferID;
                }
                if (ProductID != SpecialOfferProduct.ProductID)
                {
                    ProductID = SpecialOfferProduct.ProductID;
                }
            }
        }
    }
}
