namespace FunctionalTests.Model
{
    using System;

    public class CustomerDiscount
    {
        public virtual int CustomerID { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual decimal Discount { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }
    }
}