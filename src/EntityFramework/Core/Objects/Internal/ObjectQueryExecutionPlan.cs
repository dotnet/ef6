// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents the 'compiled' form of all elements (query + result assembly) required to execute a specific <see
    ///      cref="ObjectQuery" />
    /// </summary>
    internal class ObjectQueryExecutionPlan
    {
        internal readonly DbCommandDefinition CommandDefinition;
        internal readonly ShaperFactory ResultShaperFactory;
        internal readonly TypeUsage ResultType;
        internal readonly MergeOption MergeOption;
        internal readonly IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> CompiledQueryParameters;

        /// <summary>
        ///     If the query yields entities from a single entity set, the value is stored here.
        /// </summary>
        private readonly EntitySet _singleEntitySet;

        /// <summary>
        ///     For testing purposes only. For anything else call <see cref="ObjectQueryExecutionPlanFactory.Prepare" />.
        /// </summary>
        public ObjectQueryExecutionPlan(
            DbCommandDefinition commandDefinition, ShaperFactory resultShaperFactory, TypeUsage resultType, MergeOption mergeOption,
            EntitySet singleEntitySet, IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters)
        {
            CommandDefinition = commandDefinition;
            ResultShaperFactory = resultShaperFactory;
            ResultType = resultType;
            MergeOption = mergeOption;
            _singleEntitySet = singleEntitySet;
            CompiledQueryParameters = compiledQueryParameters;
        }

        internal string ToTraceString()
        {
            var traceString = string.Empty;
            var entityCommandDef = CommandDefinition as EntityCommandDefinition;
            if (entityCommandDef != null)
            {
                traceString = entityCommandDef.ToTraceString();
            }
            return traceString;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual ObjectResult<TResultType> Execute<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues)
        {
            DbDataReader storeReader = null;
            try
            {
                // create entity command (just do this to snarf store command)
                var commandDefinition = (EntityCommandDefinition)CommandDefinition;
                var entityCommand = new EntityCommand((EntityConnection)context.Connection, commandDefinition);

                // pass through parameters and timeout values
                if (context.CommandTimeout.HasValue)
                {
                    entityCommand.CommandTimeout = context.CommandTimeout.Value;
                }

                if (parameterValues != null)
                {
                    foreach (var parameter in parameterValues)
                    {
                        var index = entityCommand.Parameters.IndexOf(parameter.Name);

                        if (index != -1)
                        {
                            entityCommand.Parameters[index].Value = parameter.Value ?? DBNull.Value;
                        }
                    }
                }

                // acquire store reader
                storeReader = commandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.Default);

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                var shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, true);

                // create materializer delegate
                TypeUsage resultItemEdmType;

                if (ResultType.EdmType.BuiltInTypeKind
                    == BuiltInTypeKind.CollectionType)
                {
                    resultItemEdmType = ((CollectionType)ResultType.EdmType).TypeUsage;
                }
                else
                {
                    resultItemEdmType = ResultType;
                }

                return new ObjectResult<TResultType>(shaper, _singleEntitySet, resultItemEdmType);
            }
            catch (Exception)
            {
                if (null != storeReader)
                {
                    // Note: The caller is responsible for disposing reader if creating
                    // the enumerator fails.
                    storeReader.Dispose();
                }
                throw;
            }
        }

        internal virtual async Task<ObjectResult<TResultType>> ExecuteAsync<TResultType>(
            ObjectContext context, ObjectParameterCollection parameterValues,
            CancellationToken cancellationToken)
        {
            DbDataReader storeReader = null;
            try
            {
                // create entity command (just do this to snarf store command)
                var commandDefinition = (EntityCommandDefinition)CommandDefinition;
                var entityCommand = new EntityCommand((EntityConnection)context.Connection, commandDefinition);

                // pass through parameters and timeout values
                if (context.CommandTimeout.HasValue)
                {
                    entityCommand.CommandTimeout = context.CommandTimeout.Value;
                }

                if (parameterValues != null)
                {
                    foreach (var parameter in parameterValues)
                    {
                        var index = entityCommand.Parameters.IndexOf(parameter.Name);

                        if (index != -1)
                        {
                            entityCommand.Parameters[index].Value = parameter.Value ?? DBNull.Value;
                        }
                    }
                }

                // acquire store reader
                storeReader = await commandDefinition.ExecuteStoreCommandsAsync(entityCommand, CommandBehavior.Default, cancellationToken);

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                var shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, true);

                // create materializer delegate
                TypeUsage resultItemEdmType;

                if (ResultType.EdmType.BuiltInTypeKind
                    == BuiltInTypeKind.CollectionType)
                {
                    resultItemEdmType = ((CollectionType)ResultType.EdmType).TypeUsage;
                }
                else
                {
                    resultItemEdmType = ResultType;
                }

                return new ObjectResult<TResultType>(shaper, _singleEntitySet, resultItemEdmType);
            }
            catch (Exception)
            {
                if (null != storeReader)
                {
                    // Note: The caller is responsible for disposing reader if creating
                    // the enumerator fails.
                    storeReader.Dispose();
                }
                throw;
            }
        }
    }
}
