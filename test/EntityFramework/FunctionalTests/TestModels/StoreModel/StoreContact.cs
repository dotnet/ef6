namespace FunctionalTests.Model
{
    using System;

    public class StoreContact
    {
        public virtual int CustomerID
        {
            get { return _customerID; }
            set
            {
                if (_customerID != value)
                {
                    if (Store != null && Store.CustomerID != value)
                    {
                        Store = null;
                    }
                    _customerID = value;
                }
            }
        }
        private int _customerID;

        public virtual int ContactID
        {
            get { return _contactID; }
            set
            {
                if (_contactID != value)
                {
                    if (Contact != null && Contact.ContactID != value)
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
                    if (ContactType != null && ContactType.ContactTypeID != value)
                    {
                        ContactType = null;
                    }
                    _contactTypeID = value;
                }
            }
        }
        private int _contactTypeID;

        public virtual Guid rowguid { get; set; }

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

        public virtual Store Store
        {
            get { return _store; }
            set
            {
                if (!ReferenceEquals(_store, value))
                {
                    var previousValue = _store;
                    _store = value;
                    FixupStore(previousValue);
                }
            }
        }
        private Store _store;

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null && previousValue.StoreContacts.Contains(this))
            {
                previousValue.StoreContacts.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.StoreContacts.Contains(this))
                {
                    Contact.StoreContacts.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupContactType(ContactType previousValue)
        {
            if (previousValue != null && previousValue.StoreContacts.Contains(this))
            {
                previousValue.StoreContacts.Remove(this);
            }

            if (ContactType != null)
            {
                if (!ContactType.StoreContacts.Contains(this))
                {
                    ContactType.StoreContacts.Add(this);
                }
                if (ContactTypeID != ContactType.ContactTypeID)
                {
                    ContactTypeID = ContactType.ContactTypeID;
                }
            }
        }

        private void FixupStore(Store previousValue)
        {
            if (previousValue != null && previousValue.StoreContacts.Contains(this))
            {
                previousValue.StoreContacts.Remove(this);
            }

            if (Store != null)
            {
                if (!Store.StoreContacts.Contains(this))
                {
                    Store.StoreContacts.Add(this);
                }
                if (CustomerID != Store.CustomerID)
                {
                    CustomerID = Store.CustomerID;
                }
            }
        }
    }
}