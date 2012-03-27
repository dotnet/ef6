namespace SimpleModel
{
    using System.Data.Entity;

    public class CeLoginsContext : LoginsContext
    {
        public CeLoginsContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CeLoginsContext>());
        }
    }
}
