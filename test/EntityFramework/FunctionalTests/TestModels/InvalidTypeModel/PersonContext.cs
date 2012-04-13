namespace InvalidTypeModel
{
    using System.Data.Entity;

    public class PersonContext : DbContext
    {
        public DbSet<Person> People { get; set; }
    }
}
