// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class VendorAddress
    {
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

        public virtual int AddressID
        {
            get { return _addressID; }
            set
            {
                if (_addressID != value)
                {
                    if (Address != null
                        && Address.AddressID != value)
                    {
                        Address = null;
                    }
                    _addressID = value;
                }
            }
        }

        private int _addressID;

        public virtual int AddressTypeID
        {
            get { return _addressTypeID; }
            set
            {
                if (_addressTypeID != value)
                {
                    if (AddressType != null
                        && AddressType.AddressTypeID != value)
                    {
                        AddressType = null;
                    }
                    _addressTypeID = value;
                }
            }
        }

        private int _addressTypeID;

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Address Address
        {
            get { return _address; }
            set
            {
                if (!ReferenceEquals(_address, value))
                {
                    var previousValue = _address;
                    _address = value;
                    FixupAddress(previousValue);
                }
            }
        }

        private Address _address;

        public virtual AddressType AddressType
        {
            get { return _addressType; }
            set
            {
                if (!ReferenceEquals(_addressType, value))
                {
                    var previousValue = _addressType;
                    _addressType = value;
                    FixupAddressType(previousValue);
                }
            }
        }

        private AddressType _addressType;

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

        private void FixupAddress(Address previousValue)
        {
            if (previousValue != null
                && previousValue.VendorAddresses.Contains(this))
            {
                previousValue.VendorAddresses.Remove(this);
            }

            if (Address != null)
            {
                if (!Address.VendorAddresses.Contains(this))
                {
                    Address.VendorAddresses.Add(this);
                }
                if (AddressID != Address.AddressID)
                {
                    AddressID = Address.AddressID;
                }
            }
        }

        private void FixupAddressType(AddressType previousValue)
        {
            if (previousValue != null
                && previousValue.VendorAddresses.Contains(this))
            {
                previousValue.VendorAddresses.Remove(this);
            }

            if (AddressType != null)
            {
                if (!AddressType.VendorAddresses.Contains(this))
                {
                    AddressType.VendorAddresses.Add(this);
                }
                if (AddressTypeID != AddressType.AddressTypeID)
                {
                    AddressTypeID = AddressType.AddressTypeID;
                }
            }
        }

        private void FixupVendor(Vendor previousValue)
        {
            if (previousValue != null
                && previousValue.VendorAddresses.Contains(this))
            {
                previousValue.VendorAddresses.Remove(this);
            }

            if (Vendor != null)
            {
                if (!Vendor.VendorAddresses.Contains(this))
                {
                    Vendor.VendorAddresses.Add(this);
                }
                if (VendorID != Vendor.VendorID)
                {
                    VendorID = Vendor.VendorID;
                }
            }
        }
    }
}
