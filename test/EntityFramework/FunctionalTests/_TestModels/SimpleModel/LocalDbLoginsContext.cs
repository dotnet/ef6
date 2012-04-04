namespace SimpleModel
{
    using System.Data.Entity;

    public class LocalDbLoginsContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
    }
}