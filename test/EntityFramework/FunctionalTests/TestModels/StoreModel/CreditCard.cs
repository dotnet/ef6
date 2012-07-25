// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.Entity.ModelConfiguration;

    public class CreditCard
    {
        public class CreditCardConfiguration : EntityTypeConfiguration<CreditCard>
        {
            public CreditCardConfiguration()
            {
                Property(cc => cc.CardNumber);
            }
        }

        public virtual int CreditCardID { get; set; }

        public virtual string CardType { get; set; }

        private string CardNumber { get; set; }

        public virtual byte ExpMonth { get; set; }

        public virtual short ExpYear { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<ContactCreditCard> ContactCreditCards
        {
            get
            {
                if (_contactCreditCards == null)
                {
                    var newCollection = new FixupCollection<ContactCreditCard>();
                    newCollection.CollectionChanged += FixupContactCreditCards;
                    _contactCreditCards = newCollection;
                }
                return _contactCreditCards;
            }
            set
            {
                if (!ReferenceEquals(_contactCreditCards, value))
                {
                    var previousValue = _contactCreditCards as FixupCollection<ContactCreditCard>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupContactCreditCards;
                    }
                    _contactCreditCards = value;
                    var newValue = value as FixupCollection<ContactCreditCard>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupContactCreditCards;
                    }
                }
            }
        }
        private ICollection<ContactCreditCard> _contactCreditCards;

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

        private void FixupContactCreditCards(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ContactCreditCard item in e.NewItems)
                {
                    item.CreditCard = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ContactCreditCard item in e.OldItems)
                {
                    if (ReferenceEquals(item.CreditCard, this))
                    {
                        item.CreditCard = null;
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
                    item.CreditCard = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.CreditCard, this))
                    {
                        item.CreditCard = null;
                    }
                }
            }
        }
    }
}