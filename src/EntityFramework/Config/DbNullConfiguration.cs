namespace System.Data.Entity.Config
{
    /// <summary>
    /// A type derived from this type should be placed in an assembly that contains
    /// a context type derived from <see cref="DbContext"/> to indicate that the context does
    /// not participate in providing code-based configuration. This is usually used for
    /// contexts that are parts of framework or infrastructure code and hence not part of
    /// the application code.
    /// </summary>
    public class DbNullConfiguration : DbConfiguration
    {
    }
}