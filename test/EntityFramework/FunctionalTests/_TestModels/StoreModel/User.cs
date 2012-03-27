namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User
    {
        public virtual int UserID { get; set; }

        [InverseProperty("Following")]
        public virtual ICollection<User> Followers { get; set; }
        public virtual ICollection<User> Following { get; set; }

        public virtual Guid rowguid { get; set; }

        public virtual DateTime ModifiedDate { get; set; }
    }
}