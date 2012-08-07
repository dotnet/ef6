// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class VendorContact
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

        public virtual int ContactID
        {
            get { return _contactID; }
            set
            {
                if (_contactID != value)
                {
                    if (Contact != null
                        && Contact.ContactID != value)
                    {
                        Contact = null;
                    }
                    _contactID = value;
                }
            }
        }

        private int _contactID;

        public virtual int ContactTypeID
        {
            get { return _contactTypeID; }
            set
            {
                if (_contactTypeID != value)
                {
                    if (ContactType != null
                        && ContactType.ContactTypeID != value)
                    {
                        ContactType = null;
                    }
                    _contactTypeID = value;
                }
            }
        }

        private int _contactTypeID;

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Contact Contact
        {
            get { return _contact; }
            set
            {
                if (!ReferenceEquals(_contact, value))
                {
                    var previousValue = _contact;
                    _contact = value;
                    FixupContact(previousValue);
                }
            }
        }

        private Contact _contact;

        public virtual ContactType ContactType
        {
            get { return _contactType; }
            set
            {
                if (!ReferenceEquals(_contactType, value))
                {
                    var previousValue = _contactType;
                    _contactType = value;
                    FixupContactType(previousValue);
                }
            }
        }

        private ContactType _contactType;

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

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null
                && previousValue.VendorContacts.Contains(this))
            {
                previousValue.VendorContacts.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.VendorContacts.Contains(this))
                {
                    Contact.VendorContacts.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupContactType(ContactType previousValue)
        {
            if (previousValue != null
                && previousValue.VendorContacts.Contains(this))
            {
                previousValue.VendorContacts.Remove(this);
            }

            if (ContactType != null)
            {
                if (!ContactType.VendorContacts.Contains(this))
                {
                    ContactType.VendorContacts.Add(this);
                }
                if (ContactTypeID != ContactType.ContactTypeID)
                {
                    ContactTypeID = ContactType.ContactTypeID;
                }
            }
        }

        private void FixupVendor(Vendor previousValue)
        {
            if (previousValue != null
                && previousValue.VendorContacts.Contains(this))
            {
                previousValue.VendorContacts.Remove(this);
            }

            if (Vendor != null)
            {
                if (!Vendor.VendorContacts.Contains(this))
                {
                    Vendor.VendorContacts.Add(this);
                }
                if (VendorID != Vendor.VendorID)
                {
                    VendorID = Vendor.VendorID;
                }
            }
        }
    }
}
