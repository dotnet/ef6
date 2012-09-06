// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     This attribute can be placed on a subclass of <see cref="DbContext" /> to indicate that the subclass of 
    ///     <see cref="DbConfiguration" /> representing the code-based configuration for the application is in a different
    ///     assembly than the context type.
    /// </summary>
    /// <remarks>
    ///     Normally a subclass of <see cref="DbConfiguration" /> should be placed in the same assembly as
    ///     the subclass of <see cref="DbContext" /> used by the application. It will then be discovered automatically.
    ///     However, if this is not possible or if the application contains multiple context types in different
    ///     assemblies, then this attribute can be used to direct DbConfiguration discovery to the appropriate type.
    ///     An alternative to using this attribute is to specify the DbConfiguration type to use in the application's
    ///     config file. See http://go.microsoft.com/fwlink/?LinkId=260883 for more information.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DbConfigurationTypeAttribute : Attribute
    {
        private readonly Type _configurationType;

        /// <summary>
        ///     Indicates that the given subclass of <see cref="DbConfiguration" /> should be used for code-based configuration
        ///     for this application.
        /// </summary>
        /// <param name="configurationType"> The <see cref="DbConfiguration" /> type to use. </param>
        public DbConfigurationTypeAttribute(Type configurationType)
        {
            Contract.Requires(configurationType != null);

            _configurationType = configurationType;
        }

        /// <summary>
        ///     Indicates that the subclass of <see cref="DbConfiguration" /> represented by the given assembly-qualified
        ///     name should be used for code-based configuration for this application.
        /// </summary>
        /// <param name="configurationTypeName"> The <see cref="DbConfiguration" /> type to use. </param>
        public DbConfigurationTypeAttribute(string configurationTypeName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(configurationTypeName));

            try
            {
                _configurationType = Type.GetType(configurationTypeName, throwOnError: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.DbConfigurationTypeInAttributeNotFound(configurationTypeName), ex);
            }
        }

        /// <summary>
        ///     Gets the subclass of <see cref="DbConfiguration" /> that should be used for code-based configuration
        ///     for this application.
        /// </summary>
        public Type ConfigurationType
        {
            get { return _configurationType; }
        }
    }
}
