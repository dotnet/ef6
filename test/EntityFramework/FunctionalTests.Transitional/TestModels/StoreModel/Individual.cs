// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class Individual
    {
        public virtual int CustomerID
        {
            get { return _customerID; }
            set
            {
                if (_customerID != value)
                {
                    if (Customer != null
                        && Customer.CustomerID != value)
                    {
                        Customer = null;
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

        public virtual string Demographics { get; set; }

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

        public virtual Customer Customer
        {
            get { return _customer; }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer;
                    _customer = value;
                    FixupCustomer(previousValue);
                }
            }
        }

        private Customer _customer;

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null
                && previousValue.Individuals.Contains(this))
            {
                previousValue.Individuals.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.Individuals.Contains(this))
                {
                    Contact.Individuals.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null
                && ReferenceEquals(previousValue.Individual, this))
            {
                previousValue.Individual = null;
            }

            if (Customer != null)
            {
                Customer.Individual = this;
                if (CustomerID != Customer.CustomerID)
                {
                    CustomerID = Customer.CustomerID;
                }
            }
        }
    }
}
