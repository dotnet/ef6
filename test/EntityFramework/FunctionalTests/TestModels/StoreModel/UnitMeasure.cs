namespace FunctionalTests.Model
{
    using System.ComponentModel.DataAnnotations;

    public class UnitMeasure
    {
        public virtual string UnitMeasureCode { get; set; }

        [MaxLength(42)]
        public virtual string Name { get; set; }
    }
}