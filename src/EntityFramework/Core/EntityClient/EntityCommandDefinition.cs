namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// An aggregate Command Definition used by the EntityClient layers.  This is an aggregator
    /// object that represent information from multiple underlying provider commands.
    /// </summary>
    internal sealed class EntityCommandDefinition : DbCommandDefinition
    {
        private InternalEntityCommandDefinition _internalEntityCommandDefinition;

        /// <summary>
        /// don't let this be constructed publicly;
        /// </summary>
        /// <exception cref="EntityCommandCompilationException">Cannot prepare the command definition for execution; consult the InnerException for more information.</exception>
        /// <exception cref="NotSupportedException">The ADO.NET Data Provider you are using does not support CommandTrees.</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EntityCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree)
            : this(new InternalEntityCommandDefinition(storeProviderFactory, commandTree))
        {
        }

        internal EntityCommandDefinition(InternalEntityCommandDefinition internalEntityCommandDefinition)
        {
            _internalEntityCommandDefinition = internalEntityCommandDefinition;
            _internalEntityCommandDefinition.EntityCommandDefinitionWrapper = this;
        }

        /// <summary>
        /// Property to expose the known parameters for the query, so the Command objects 
        /// constructor can poplulate it's parameter collection from.
        /// </summary>
        internal IEnumerable<EntityParameter> Parameters
        {
            get { return _internalEntityCommandDefinition.Parameters; }
        }

        /// <summary>
        /// Set of entity sets exposed in the command.
        /// </summary>
        internal Set<EntitySet> EntitySets
        {
            get { return _internalEntityCommandDefinition.EntitySets; }
        }

        /// <summary>
        /// Create a DbCommand object from the definition, that can be executed
        /// </summary>
        /// <returns></returns>
        public override DbCommand CreateCommand()
        {
            return _internalEntityCommandDefinition.CreateCommand();
        }

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader.
        /// </summary>
        internal ColumnMap CreateColumnMap(DbDataReader storeDataReader)
        {
            return CreateColumnMap(storeDataReader, 0);
        }

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader's resultSetIndexth result set.
        /// </summary>
        internal ColumnMap CreateColumnMap(DbDataReader storeDataReader, int resultSetIndex)
        {
            return _internalEntityCommandDefinition.CreateColumnMap(storeDataReader, resultSetIndex);
        }

        internal static void PopulateParameterFromTypeUsage(EntityParameter parameter, TypeUsage type, bool isOutParam)
        {
            // type can be null here if the type provided by the user is not a known model type
            if (type != null)
            {
                PrimitiveTypeKind primitiveTypeKind;

                if (Helper.IsEnumType(type.EdmType))
                {
                    type = TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(type.EdmType));
                }
                else if (Helper.IsSpatialType(type, out primitiveTypeKind))
                {
                    parameter.EdmType = EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
                }
            }

            DbCommandDefinition.PopulateParameterFromTypeUsage(parameter, type, isOutParam);
        }

        /// <summary>
        /// Internal execute method -- copies command information from the map command 
        /// to the command objects, executes them, and builds the result assembly 
        /// structures needed to return the data reader
        /// </summary>
        /// <param name="entityCommand"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">behavior must specify CommandBehavior.SequentialAccess</exception>
        /// <exception cref="InvalidOperationException">input parameters in the entityCommand.Parameters collection must have non-null values.</exception>
        internal DbDataReader Execute(EntityCommand entityCommand, CommandBehavior behavior)
        {
            return _internalEntityCommandDefinition.Execute(entityCommand, behavior);
        }

        /// <summary>
        /// Execute the store commands, and return IteratorSources for each one
        /// </summary>
        /// <param name="entityCommand"></param>
        /// <param name="behavior"></param>
        internal DbDataReader ExecuteStoreCommands(EntityCommand entityCommand, CommandBehavior behavior)
        {
            return _internalEntityCommandDefinition.ExecuteStoreCommands(entityCommand, behavior);
        }

        /// <summary>
        /// Return the string used by EntityCommand and ObjectQuery<T> ToTraceString"/>
        /// </summary>
        /// <returns></returns>
        internal string ToTraceString()
        {
            return _internalEntityCommandDefinition.ToTraceString();
        }
    }
}
