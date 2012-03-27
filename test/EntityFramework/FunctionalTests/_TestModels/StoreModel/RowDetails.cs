namespace FunctionalTests.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ComplexType]
    public class RowDetails
    {
        public Guid rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}