namespace System.Data.Entity.Migrations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VariantAttribute : Attribute
    {
        public VariantAttribute(DatabaseProvider provider, ProgrammingLanguage language)
        {
            DatabaseProvider = provider;
            ProgrammingLanguage = language;
        }

        public DatabaseProvider DatabaseProvider { get; private set; }
        public ProgrammingLanguage ProgrammingLanguage { get; private set; }
    }
}