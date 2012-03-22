namespace System.Data.Entity
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This attribute can be applied to a class derived from <see cref = "DbContext" /> to set which
    ///     version of the DbContext and <see cref = "DbModelBuilder" /> conventions should be used when building
    ///     a model from code--also know as "Code First". See the <see cref = "DbModelBuilderVersion" />
    ///     enumeration for details about DbModelBuilder versions.
    /// </summary>
    /// <remarks>
    ///     If the attribute is missing from DbContextthen DbContext will always use the latest
    ///     version of the conventions.  This is equivalent to using DbModelBuilderVersion.Latest.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DbModelBuilderVersionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbModelBuilderVersionAttribute" /> class.
        /// </summary>
        /// <param name = "version">The <see cref = "DbModelBuilder" /> conventions version to use.</param>
        public DbModelBuilderVersionAttribute(DbModelBuilderVersion version)
        {
            if (!Enum.IsDefined(typeof(DbModelBuilderVersion), version))
            {
                throw new ArgumentOutOfRangeException("version");
            }

            Version = version;
        }

        /// <summary>
        ///     Gets the <see cref = "DbModelBuilder" /> conventions version.
        /// </summary>
        /// <value>The <see cref = "DbModelBuilder" /> conventions version.</value>
        public DbModelBuilderVersion Version { get; private set; }
    }
}
