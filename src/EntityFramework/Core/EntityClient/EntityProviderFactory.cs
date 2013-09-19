// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// Class representing a provider factory for the entity client provider
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2302", Justification = "We don't expect serviceType to be an Embedded Interop Types.")]
    public sealed class EntityProviderFactory : DbProviderFactory, IServiceProvider
    {
        /// <summary>
        /// A singleton object for the entity client provider factory object.
        /// This remains a public field (not property) because DbProviderFactory expects a field.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification =
                "EntityProviderFactory implements the singleton pattern and it's stateless.  This is needed in order to work with DbProviderFactories."
            )]
        public static readonly EntityProviderFactory Instance = new EntityProviderFactory();

        /// <summary>
        /// Constructs the EntityProviderFactory object, this is private as users shouldn't create it directly
        /// </summary>
        private EntityProviderFactory()
        {
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityCommand" />
        /// class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Entity.Core.EntityClient.EntityCommand" />.
        /// </returns>
        public override DbCommand CreateCommand()
        {
            return new EntityCommand();
        }

        /// <summary>
        /// Throws a <see cref="T:System.NotSupportedException" />. This method is currently not supported.
        /// </summary>
        /// <returns>This method is currently not supported.</returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" />
        /// class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnection" />.
        /// </returns>
        public override DbConnection CreateConnection()
        {
            return new EntityConnection();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />.
        /// </returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new EntityConnectionStringBuilder();
        }

        /// <summary>
        /// Throws a <see cref="T:System.NotSupportedException" />. This method is currently not supported.
        /// </summary>
        /// <returns>This method is currently not supported.</returns>

        public override DbDataAdapter CreateDataAdapter()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />
        /// class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />.
        /// </returns>
        public override DbParameter CreateParameter()
        {
            return new EntityParameter();
        }

        /// <summary>
        /// Throws a <see cref="T:System.NotSupportedException" />. This method is currently not supported.
        /// </summary>
        /// <returns>This method is currently not supported.</returns>

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the requested <see cref="T:System.IServiceProvider" /> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.IServiceProvider" />. The supported types are
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.DbProviderServices" />
        /// ,
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.DbCommandDefinitionBuilder" />
        /// , and
        /// <see
        ///     cref="T:System.Data.IEntityAdapter" />
        /// . Returns null (or Nothing in Visual Basic) for every other type.
        /// </returns>
        /// <param name="serviceType">
        /// The <see cref="T:System.Type" /> to return.
        /// </param>
        object IServiceProvider.GetService(Type serviceType)
        {
            return serviceType == typeof(DbProviderServices) ? EntityProviderServices.Instance : null;
        }
    }
}
