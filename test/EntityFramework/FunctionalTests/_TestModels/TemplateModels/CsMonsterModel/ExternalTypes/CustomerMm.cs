namespace Another.Place
{
    using System.Collections.Generic;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;

    public partial class CustomerMm
    {
        public CustomerMm()
        {
            Orders = new HashSet<OrderMm>();
            Logins = new HashSet<LoginMm>();
            ContactInfo = new ContactDetailsMm();
            Auditing = new AuditInfoMm();
        }

        public int CustomerId { get; set; }
        public string Name { get; set; }

        public ContactDetailsMm ContactInfo { get; set; }
        public AuditInfoMm Auditing { get; set; }

        public virtual ICollection<OrderMm> Orders { get; set; }
        public virtual ICollection<LoginMm> Logins { get; set; }
        public virtual CustomerMm Husband { get; set; }
        public virtual CustomerMm Wife { get; set; }
        public virtual CustomerInfoMm Info { get; set; }
    }
}