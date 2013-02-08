// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class ContactCreditCard
    {
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

        public virtual int CreditCardID
        {
            get { return _creditCardID; }
            set
            {
                if (_creditCardID != value)
                {
                    if (CreditCard != null
                        && CreditCard.CreditCardID != value)
                    {
                        CreditCard = null;
                    }
                    _creditCardID = value;
                }
            }
        }

        private int _creditCardID;

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

        public virtual CreditCard CreditCard
        {
            get { return _creditCard; }
            set
            {
                if (!ReferenceEquals(_creditCard, value))
                {
                    var previousValue = _creditCard;
                    _creditCard = value;
                    FixupCreditCard(previousValue);
                }
            }
        }

        private CreditCard _creditCard;

        private void FixupContact(Contact previousValue)
        {
            if (previousValue != null
                && previousValue.ContactCreditCards.Contains(this))
            {
                previousValue.ContactCreditCards.Remove(this);
            }

            if (Contact != null)
            {
                if (!Contact.ContactCreditCards.Contains(this))
                {
                    Contact.ContactCreditCards.Add(this);
                }
                if (ContactID != Contact.ContactID)
                {
                    ContactID = Contact.ContactID;
                }
            }
        }

        private void FixupCreditCard(CreditCard previousValue)
        {
            if (previousValue != null
                && previousValue.ContactCreditCards.Contains(this))
            {
                previousValue.ContactCreditCards.Remove(this);
            }

            if (CreditCard != null)
            {
                if (!CreditCard.ContactCreditCards.Contains(this))
                {
                    CreditCard.ContactCreditCards.Add(this);
                }
                if (CreditCardID != CreditCard.CreditCardID)
                {
                    CreditCardID = CreditCard.CreditCardID;
                }
            }
        }
    }
}
