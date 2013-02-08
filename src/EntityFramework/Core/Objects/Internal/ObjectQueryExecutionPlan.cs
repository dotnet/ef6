// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    /// <summary>
    ///     Represents the 'compiled' form of all elements (query + result assembly) required to execute a specific
    ///     <see cref="ObjectQuery" />
    /// </summary>
    internal class ObjectQueryExecutionPlan
    {
        internal readonly DbCommandDefinition CommandDefinition;
        internal readonly bool Streaming;
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

        internal string ToTraceString(ObjectParameterCollection parameters = null)
        {
            var entityCommandDef = CommandDefinition as EntityCommandDefinition;

            if (entityCommandDef == null)
            {
                return String.Empty;
            }

            var traceString = entityCommandDef.ToTraceString();

            if (parameters == null
                || parameters.Count == 0)
            {
                return traceString;
            }

            var command = CommandDefinition.CreateCommand();
            var sb = new StringBuilder();

            sb.Append(traceString);
            sb.Append("\n\n/*\n");

            // append parameter information
            foreach (var objParam in parameters)
            {
                var index = command.Parameters.IndexOf(objParam.Name);

                if (index != -1)
                {
                    var dbParam = command.Parameters[index];
                    sb.Append(dbParam.DbType);
                }
                else
                {
                    Debug.Assert(
                        false,
                        "A parameter in the input ObjectParameterCollection does not have a match in the command definition's DbParameterCollection.");
                }

                sb.Append(" ");
                sb.Append(objParam.Name);
                sb.Append(" = ");

                var value = objParam.Value;
                var type = (value != null) ? value.GetType() : null;

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                        sb.Append("(null)");
                        break;

                    case TypeCode.Char:
                        sb.Append("'");
                        sb.Append(value);
                        sb.Append("'");
                        break;

                    case TypeCode.String:
                        sb.Append("\"");
                        sb.Append(value);
                        sb.Append("\"");
                        break;

                    default:
                        sb.Append(value);
                        break;
                }

                sb.Append("\n");
            }

            sb.Append("*/\n");

            return sb.ToString();
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
                    storeReader = entityCommand.GetCommandDefinition().ExecuteStoreCommands(entityCommand,
                        Streaming
                            ? CommandBehavior.Default
                            : CommandBehavior.SequentialAccess);
                }

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                Shaper<TResultType> shaper;
                if (Streaming)
                {
                    shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, true, useSpatialReader: true);
                }
                else
                {
                    var storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.GetService<DbProviderServices>(storeItemCollection.StoreProviderInvariantName);
                    
                    bufferedReader = new BufferedDataReader(storeReader);
                    bufferedReader.Initialize(storeItemCollection.StoreProviderManifestToken, providerServices);

                    shaper = shaperFactory.Create(bufferedReader, context, context.MetadataWorkspace, MergeOption, true, useSpatialReader: false);
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

                if (!Streaming && bufferedReader != null)
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
                                               .ExecuteStoreCommandsAsync(entityCommand,
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
                    shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, true, useSpatialReader: true);
                }
                else
                {
                    var storeItemCollection = (StoreItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                    var providerServices = DbConfiguration.GetService<DbProviderServices>(storeItemCollection.StoreProviderInvariantName);

                    bufferedReader = new BufferedDataReader(storeReader);
                    await bufferedReader.InitializeAsync(storeItemCollection.StoreProviderManifestToken, providerServices, cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);

                    shaper = shaperFactory.Create(bufferedReader, context, context.MetadataWorkspace, MergeOption, true, useSpatialReader: false);
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

                if (!Streaming && bufferedReader != null)
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

            return entityCommand;
        }
    }
}
