namespace Another.Place
{
    using System;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;

    public partial class AuditInfoMm
    {
        public AuditInfoMm()
        {
            Concurrency = new ConcurrencyInfoMm();
        }

        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        public ConcurrencyInfoMm Concurrency { get; set; }
    }
}