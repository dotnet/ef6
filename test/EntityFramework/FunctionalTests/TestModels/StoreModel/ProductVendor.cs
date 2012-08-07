// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductVendor
    {
        public virtual int ProductID
        {
            get { return _productID; }
            set
            {
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
        }

        private int _productID;

        public virtual int VendorID
        {
            get { return _vendorID; }
            set
            {
                if (_vendorID != value)
                {
                    if (Vendor != null
                        && Vendor.VendorID != value)
                    {
                        Vendor = null;
                    }
                    _vendorID = value;
                }
            }
        }

        private int _vendorID;

        public virtual int AverageLeadTime { get; set; }

        public virtual decimal StandardPrice { get; set; }

        public virtual decimal? LastReceiptCost { get; set; }

        public virtual DateTime? LastReceiptDate { get; set; }

        public virtual int MinOrderQty { get; set; }

        public virtual int MaxOrderQty { get; set; }

        public virtual int? OnOrderQty { get; set; }

        public virtual string UnitMeasureCode
        {
            get { return _unitMeasureCode; }
            set
            {
                if (_unitMeasureCode != value)
                {
                    if (UnitMeasure != null
                        && UnitMeasure.UnitMeasureCode != value)
                    {
                        UnitMeasure = null;
                    }
                    _unitMeasureCode = value;
                }
            }
        }

        private string _unitMeasureCode;

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

        public virtual UnitMeasure UnitMeasure { get; set; }

        public virtual Vendor Vendor
        {
            get { return _vendor; }
            set
            {
                if (!ReferenceEquals(_vendor, value))
                {
                    var previousValue = _vendor;
                    _vendor = value;
                    FixupVendor(previousValue);
                }
            }
        }

        private Vendor _vendor;

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.ProductVendors.Contains(this))
            {
                previousValue.ProductVendors.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductVendors.Contains(this))
                {
                    Product.ProductVendors.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }

        private void FixupVendor(Vendor previousValue)
        {
            if (previousValue != null
                && previousValue.ProductVendors.Contains(this))
            {
                previousValue.ProductVendors.Remove(this);
            }

            if (Vendor != null)
            {
                if (!Vendor.ProductVendors.Contains(this))
                {
                    Vendor.ProductVendors.Add(this);
                }
                if (VendorID != Vendor.VendorID)
                {
                    VendorID = Vendor.VendorID;
                }
            }
        }
    }
}
