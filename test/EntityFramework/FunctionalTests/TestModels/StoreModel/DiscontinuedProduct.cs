namespace FunctionalTests.Model
{
    using System;

    public class DiscontinuedProduct : Product
    {
        public virtual DateTime DiscontinuedDate { get; set; }
    }
}