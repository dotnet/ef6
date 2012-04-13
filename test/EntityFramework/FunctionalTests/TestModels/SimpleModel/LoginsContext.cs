namespace SimpleModel
{
    using System.Data.Entity;

    public class LoginsContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
    }
}
