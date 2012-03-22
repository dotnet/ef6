namespace System.Data.Entity
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     A value from this enumeration can be provided directly to the <see cref = "DbModelBuilder" />
    ///     class or can be used in the <see cref = "DbModelBuilderVersionAttribute" /> applied to
    ///     a class derived from <see cref = "DbContext" />. The value used defines which version of
    ///     the DbContext and DbModelBuilder conventions should be used when building a model from
    ///     code--also know as "Code First".
    /// </summary>
    /// <remarks>
    ///     Using DbModelBuilderVersion.Latest ensures that all the latest functionality is available
    ///     when upgrading to a new release of the Entity Framework. However, it may result in an
    ///     application behaving differently with the new release than it did with a previous release.
    ///     This can be avoided by using a specific version of the conventions, but if a version
    ///     other than the latest is set then not all the latest functionality will be available.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public enum DbModelBuilderVersion
    {
        /// <summary>
        ///     Indicates that the latest version of the <see cref = "DbModelBuilder" /> and 
        ///     <see cref = "DbContext" /> conventions should be used.
        /// </summary>
        Latest,

        /// <summary>
        ///     Indicates that the version of the <see cref = "DbModelBuilder" /> and 
        ///     <see cref = "DbContext" /> conventions shipped with Entity Framework v4.1
        ///     should be used.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        V4_1,

        /// <summary>
        ///     Indicates that the version of the <see cref = "DbModelBuilder" /> and 
        ///     <see cref = "DbContext" /> conventions shipped with Entity Framework v5.0
        ///     should be used.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        V5_0
    }
}
