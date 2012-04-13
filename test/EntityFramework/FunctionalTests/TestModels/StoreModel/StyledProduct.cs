namespace FunctionalTests.Model
{
    using System.ComponentModel.DataAnnotations;

    public class StyledProduct : Product
    {
        [StringLength(150)]
        public virtual string Style { get; set; }
    }
}