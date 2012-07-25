// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// The class for provider services of the entity client
    /// </summary>
    internal sealed class EntityProviderServices : DbProviderServices
    {
        /// <summary>
        /// Singleton object;
        /// </summary>
        internal static readonly EntityProviderServices Instance = new EntityProviderServices();

        /// <summary>
        /// Create a Command Definition object, given the connection and command tree
        /// </summary>
        /// <param name="connection">connection to the underlying provider</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an executable command definition object</returns>
        /// <exception cref="ArgumentNullException">connection and commandTree arguments must not be null</exception>
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            var storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            return CreateCommandDefinition(storeMetadata.StoreProviderFactory, commandTree);
        }

        internal static EntityCommandDefinition CreateCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree)
        {
            Contract.Requires(storeProviderFactory != null);
            Contract.Requires(commandTree != null);

            return new EntityCommandDefinition(storeProviderFactory, commandTree);
        }

        /// <summary>
        /// Ensures that the data space of the specified command tree is the model (C-) space
        /// </summary>
        /// <param name="commandTree">The command tree for which the data space should be validated</param>
        internal override void ValidateDataSpace(DbCommandTree commandTree)
        {
            if (commandTree.DataSpace
                != DataSpace.CSpace)
            {
                throw new ProviderIncompatibleException(Strings.EntityClient_RequiresNonStoreCommandTree);
            }
        }

        /// <summary>
        /// Create a EntityCommandDefinition object based on the prototype command
        /// This method is intended for provider writers to build a default command definition
        /// from a command. 
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">prototype argument must not be null</exception>
        /// <exception cref="InvalidCastException">prototype argument must be a EntityCommand</exception>
        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return ((EntityCommand)prototype).GetCommandDefinition();
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            if (connection.GetType()
                != typeof(EntityConnection))
            {
                throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(EntityConnection)));
            }

            return MetadataItem.EdmProviderManifest.Token;
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return MetadataItem.EdmProviderManifest;
        }
    }
}
