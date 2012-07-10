namespace System.Data.Entity.Config
{
    /// <summary>
    /// A type derived from this type should be placed in an assembly that contains
    /// a context type derived from <see cref="DbContext"/> to indicate that the code-based configuration
    /// for that context is contained in a different assembly. The type of the code-based configuration
    /// to use must be returned from the ConfigurationToUse method and must be a type derived from
    /// <see cref="DbConfiguration"/>.
    /// </summary>
    public abstract class DbConfigurationProxy : DbConfiguration
    {
        /// <summary>
        /// Called to get the type of <see cref="DbConfiguration"/> to use for code-based configuration
        /// in this assembly when that DbConfiguration type is contained in another assembly.
        /// </summary>
        /// <returns>The <see cref="DbConfiguration"/> type to use.</returns>
        public abstract Type ConfigurationToUse();
    }
}