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
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // Represents the 'compiled' form of all elements (query + result assembly) required to execute a specific
    // <see cref="ObjectQuery" />
    // </summary>
    internal class ObjectQueryExecutionPlan
    {
        internal readonly DbCommandDefinition CommandDefinition;
        internal readonly bool Streaming;
        internal readonly ShaperFactory ResultShaperFactory;
        internal readonly TypeUsage ResultType;
        internal readonly MergeOption MergeOption;
        internal readonly IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> CompiledQueryParameters;

        // <summary>
        // If the query yields entities from a single entity set, the value is stored here.
        // </summary>
        private readonly EntitySet _singleEntitySet;

        // <summary>
        // For testing purposes only. For anything else call <see cref="ObjectQueryExecutionPlanFactory.Prepare" />.
        // </summary>
        public ObjectQueryExecutionPlan(
            DbCommandDefinition commandDefinition, ShaperFactory resultShaperFactory, TypeUsage resultType, MergeOption mergeOption,
            bool streaming, EntitySet singleEntitySet, IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters)
        {
            CommandDefinition = commandDefinition;
            ResultShaperFactory = resultShaperFactory;
            ResultType = resultType;
            MergeOption = mergeOption;
            Streaming = streaming;
            _singleEntitySet = singleEntitySet;
            CompiledQueryParameters = compiledQueryParameters;
        }

        internal string ToTraceString()
        {
            var entityCommandDef = CommandDefinition as EntityCommandDefinition;

            return 
                (entityCommandDef != null) 
                    ? entityCommandDef.ToTraceString() 
                    : string.Empty;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Buffer disposed by the returned ObjectResult")]
        internal virtual ObjectResult<TResultType> Execute<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues)
        {
            DbDataReader storeReader = null;
            BufferedDataReader bufferedReader = null;
            try
            {
                using (var entityCommand = PrepareEntityCommand(context, parameterValues))
                {
                    // acquire store reader
                    storeReader = entityCommand.GetCommandDefinition().ExecuteStoreCommands(
                        entityCommand,
                        Streaming
                            ? CommandBehavior.Default
                            : CommandBehavior.SequentialAccess);
                }

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                Shaper<TResultType> shaper;
                if (Streaming)
                {
                    shaper = shaperFactory.Create(
                        storeReader, context, context.MetadataWorkspace, MergeOption, true, Streaming);
                }
                else
                {
                    var storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);

                    bufferedReader = new BufferedDataReader(storeReader);
                    bufferedReader.Initialize(storeItemCollection.ProviderManifestToken, providerServices, shaperFactory.ColumnTypes, shaperFactory.NullableColumns);

                    shaper = shaperFactory.Create(
                        bufferedReader, context, context.MetadataWorkspace, MergeOption, true, Streaming);
                }

                // create materializer delegate
                TypeUsage resultItemEdmType;
                if (ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
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
                // Note: The ObjectResult is responsible for disposing the reader if creating
                // the enumerator fails.
                if (Streaming && storeReader != null)
                {
                    storeReader.Dispose();
                }

                if (!Streaming
                    && bufferedReader != null)
                {
                    bufferedReader.Dispose();
                }
                throw;
            }
        }

#if !NET40

        internal virtual async Task<ObjectResult<TResultType>> ExecuteAsync<TResultType>(
            ObjectContext context, ObjectParameterCollection parameterValues,
            CancellationToken cancellationToken)
        {
            DbDataReader storeReader = null;
            BufferedDataReader bufferedReader = null;
            try
            {
                using (var entityCommand = PrepareEntityCommand(context, parameterValues))
                {
                    // acquire store reader
                    storeReader = await
                                  entityCommand.GetCommandDefinition()
                                               .ExecuteStoreCommandsAsync(
                                                   entityCommand,
                                                   Streaming
                                                       ? CommandBehavior.Default
                                                       : CommandBehavior.SequentialAccess
                                      , cancellationToken)
                                               .ConfigureAwait(continueOnCapturedContext: false);
                }

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                Shaper<TResultType> shaper;
                if (Streaming)
                {
                    shaper = shaperFactory.Create(
                        storeReader, context, context.MetadataWorkspace, MergeOption, true, Streaming);
                }
                else
                {
                    var storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(storeItemCollection.ProviderInvariantName);

                    bufferedReader = new BufferedDataReader(storeReader);
                    await
                        bufferedReader.InitializeAsync(storeItemCollection.ProviderManifestToken, providerServices, shaperFactory.ColumnTypes, shaperFactory.NullableColumns, cancellationToken)
                                      .ConfigureAwait(continueOnCapturedContext: false);

                    shaper = shaperFactory.Create(
                        bufferedReader, context, context.MetadataWorkspace, MergeOption, true, Streaming);
                }

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
                // Note: The ObjectResult is responsible for disposing the reader if creating
                // the enumerator fails.
                if (Streaming && storeReader != null)
                {
                    storeReader.Dispose();
                }

                if (!Streaming
                    && bufferedReader != null)
                {
                    bufferedReader.Dispose();
                }
                throw;
            }
        }

#endif

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Disposed by caller")]
        private EntityCommand PrepareEntityCommand(ObjectContext context, ObjectParameterCollection parameterValues)
        {
            // create entity command (just do this to snarf store command)
            var commandDefinition = (EntityCommandDefinition)CommandDefinition;
            var entityCommand = new EntityCommand(
                (EntityConnection)context.Connection, commandDefinition, context.InterceptionContext);

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

            return entityCommand;
        }
    }
}
