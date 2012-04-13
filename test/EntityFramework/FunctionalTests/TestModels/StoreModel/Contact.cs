namespace FunctionalTests.Model
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class Contact
    {
        public virtual int ContactID { get; set; }

        public virtual bool NameStyle { get; set; }

        public virtual string Title { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string MiddleName { get; set; }

        public virtual string LastName { get; set; }

        public virtual string Suffix { get; set; }

        public virtual string EmailAddress { get; set; }

        public virtual int EmailPromotion { get; set; }

        public virtual string Phone { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual string PasswordSalt { get; set; }

        public virtual string AdditionalContactInfo { get; set; }

        public virtual RowDetails RowDetails { get; set; }

        public virtual ICollection<Employee> Employees
        {
            get
            {
                if (_employees == null)
                {
                    var newCollection = new FixupCollection<Employee>();
                    newCollection.CollectionChanged += FixupEmployees;
                    _employees = newCollection;
                }
                return _employees;
            }
            set
            {
                if (!ReferenceEquals(_employees, value))
                {
                    var previousValue = _employees as FixupCollection<Employee>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmployees;
                    }
                    _employees = value;
                    var newValue = value as FixupCollection<Employee>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmployees;
                    }
                }
            }
        }
        private ICollection<Employee> _employees;

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

        public virtual ICollection<Individual> Individuals
        {
            get
            {
                if (_individuals == null)
                {
                    var newCollection = new FixupCollection<Individual>();
                    newCollection.CollectionChanged += FixupIndividuals;
                    _individuals = newCollection;
                }
                return _individuals;
            }
            set
            {
                if (!ReferenceEquals(_individuals, value))
                {
                    var previousValue = _individuals as FixupCollection<Individual>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupIndividuals;
                    }
                    _individuals = value;
                    var newValue = value as FixupCollection<Individual>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupIndividuals;
                    }
                }
            }
        }
        private ICollection<Individual> _individuals;

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

        public virtual ICollection<StoreContact> StoreContacts
        {
            get
            {
                if (_storeContacts == null)
                {
                    var newCollection = new FixupCollection<StoreContact>();
                    newCollection.CollectionChanged += FixupStoreContacts;
                    _storeContacts = newCollection;
                }
                return _storeContacts;
            }
            set
            {
                if (!ReferenceEquals(_storeContacts, value))
                {
                    var previousValue = _storeContacts as FixupCollection<StoreContact>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupStoreContacts;
                    }
                    _storeContacts = value;
                    var newValue = value as FixupCollection<StoreContact>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupStoreContacts;
                    }
                }
            }
        }
        private ICollection<StoreContact> _storeContacts;

        public virtual ICollection<VendorContact> VendorContacts
        {
            get
            {
                if (_vendorContacts == null)
                {
                    var newCollection = new FixupCollection<VendorContact>();
                    newCollection.CollectionChanged += FixupVendorContacts;
                    _vendorContacts = newCollection;
                }
                return _vendorContacts;
            }
            set
            {
                if (!ReferenceEquals(_vendorContacts, value))
                {
                    var previousValue = _vendorContacts as FixupCollection<VendorContact>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupVendorContacts;
                    }
                    _vendorContacts = value;
                    var newValue = value as FixupCollection<VendorContact>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupVendorContacts;
                    }
                }
            }
        }
        private ICollection<VendorContact> _vendorContacts;

        private void FixupEmployees(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Employee item in e.NewItems)
                {
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Employee item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
                    }
                }
            }
        }

        private void FixupContactCreditCards(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ContactCreditCard item in e.NewItems)
                {
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ContactCreditCard item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
                    }
                }
            }
        }

        private void FixupIndividuals(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Individual item in e.NewItems)
                {
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Individual item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
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
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SalesOrderHeader item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
                    }
                }
            }
        }

        private void FixupStoreContacts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StoreContact item in e.NewItems)
                {
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (StoreContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
                    }
                }
            }
        }

        private void FixupVendorContacts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (VendorContact item in e.NewItems)
                {
                    item.Contact = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (VendorContact item in e.OldItems)
                {
                    if (ReferenceEquals(item.Contact, this))
                    {
                        item.Contact = null;
                    }
                }
            }
        }
    }
}