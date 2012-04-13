namespace AdvancedPatternsModel
{
    using System.Data.Entity;

    public class AdvancedPatternsEmployeeContext : DbContext
    {
        public AdvancedPatternsEmployeeContext()
            : base("AdvancedPatternsDatabase")
        {}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            AdvancedPatternsMasterContext.SetupModel(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Employee> AllEmployees { get; set; }
        public DbSet<PastEmployee> PastEmployees { get; set; }
        public DbSet<CurrentEmployee> CurrentEmployees { get; set; }
    }
}
