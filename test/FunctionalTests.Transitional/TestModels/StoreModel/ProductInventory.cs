// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ProductInventory
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

        public virtual short LocationID
        {
            get { return _locationID; }
            set
            {
                if (_locationID != value)
                {
                    if (Location != null
                        && Location.LocationID != value)
                    {
                        Location = null;
                    }
                    _locationID = value;
                }
            }
        }

        private short _locationID;

        public virtual string Shelf { get; set; }

        public virtual byte Bin { get; set; }

        public virtual short Quantity { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Location Location
        {
            get { return _location; }
            set
            {
                if (!ReferenceEquals(_location, value))
                {
                    var previousValue = _location;
                    _location = value;
                    FixupLocation(previousValue);
                }
            }
        }

        private Location _location;

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

        private void FixupLocation(Location previousValue)
        {
            if (previousValue != null
                && previousValue.ProductInventories.Contains(this))
            {
                previousValue.ProductInventories.Remove(this);
            }

            if (Location != null)
            {
                if (!Location.ProductInventories.Contains(this))
                {
                    Location.ProductInventories.Add(this);
                }
                if (LocationID != Location.LocationID)
                {
                    LocationID = Location.LocationID.Value;
                }
            }
        }

        private void FixupProduct(Product previousValue)
        {
            if (previousValue != null
                && previousValue.ProductInventories.Contains(this))
            {
                previousValue.ProductInventories.Remove(this);
            }

            if (Product != null)
            {
                if (!Product.ProductInventories.Contains(this))
                {
                    Product.ProductInventories.Add(this);
                }
                if (ProductID != Product.ProductID)
                {
                    ProductID = Product.ProductID;
                }
            }
        }
    }
}
