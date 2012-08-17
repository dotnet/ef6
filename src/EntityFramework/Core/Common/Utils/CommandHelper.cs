// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Contains utility methods for construction of DB commands through generic
    ///     provider interfaces.
    /// </summary>
    internal static class CommandHelper
    {
        /// <summary>
        ///     Consumes all rows and result sets from the reader. This allows client to retrieve
        ///     parameter values and intercept any store exceptions.
        /// </summary>
        /// <param name="reader"> Reader to consume. </param>
        internal static void ConsumeReader(DbDataReader reader)
        {
            if (null != reader
                && !reader.IsClosed)
            {
                while (reader.NextResult())
                {
                    // Note that we only walk through the result sets. We don't need
                    // to walk through individual rows (though underlying provider
                    // implementation may do so)
                }
            }
        }


#if !NET40

        /// <summary>
        ///     Asynchronously consumes all rows and result sets from the reader. This allows client to retrieve
        ///     parameter values and intercept any store exceptions.
        /// </summary>
        internal static async Task ConsumeReaderAsync(DbDataReader reader, CancellationToken cancellationToken)
        {
            if (null != reader
                && !reader.IsClosed)
            {
                while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    // Note that we only walk through the result sets. We don't need
                    // to walk through individual rows (though underlying provider
                    // implementation may do so)
                }
            }
        }

#endif

        /// <summary>
        ///     requires: commandText must not be null
        ///     The command text must be in the form Container.FunctionImportName.
        /// </summary>
        internal static void ParseFunctionImportCommandText(
            string commandText, string defaultContainerName, out string containerName, out string functionImportName)
        {
            Debug.Assert(null != commandText);

            // Split the string
            var nameParts = commandText.Split('.');
            containerName = null;
            functionImportName = null;
            if (2 == nameParts.Length)
            {
                containerName = nameParts[0].Trim();
                functionImportName = nameParts[1].Trim();
            }
            else if (1 == nameParts.Length
                     && null != defaultContainerName)
            {
                containerName = defaultContainerName;
                functionImportName = nameParts[0].Trim();
            }
            if (string.IsNullOrEmpty(containerName)
                || string.IsNullOrEmpty(functionImportName))
            {
                throw new InvalidOperationException(Strings.EntityClient_InvalidStoredProcedureCommandText);
            }
        }

        /// <summary>
        ///     Given an entity command and entity transaction, passes through relevant state to store provider
        ///     command.
        /// </summary>
        /// <param name="entityCommand"> Entity command. Must not be null. </param>
        /// <param name="entityTransaction"> Entity transaction. Must not be null. </param>
        /// <param name="storeProviderCommand"> Store provider command that is being setup. Must not be null. </param>
        internal static void SetStoreProviderCommandState(
            EntityCommand entityCommand, EntityTransaction entityTransaction, DbCommand storeProviderCommand)
        {
            Debug.Assert(null != entityCommand);
            Debug.Assert(null != storeProviderCommand);

            storeProviderCommand.CommandTimeout = entityCommand.CommandTimeout;
            storeProviderCommand.Connection = (entityCommand.Connection).StoreConnection;
            storeProviderCommand.Transaction = (null != entityTransaction) ? entityTransaction.StoreTransaction : null;
            storeProviderCommand.UpdatedRowSource = entityCommand.UpdatedRowSource;
        }

        /// <summary>
        ///     Given an entity command, store provider command and a connection, sets all output parameter values on the entity command.
        ///     The connection is used to determine how to map spatial values.
        /// </summary>
        /// <param name="entityCommand"> Entity command on which to set parameter values. Must not be null. </param>
        /// <param name="storeProviderCommand"> Store provider command from which to retrieve parameter values. Must not be null. </param>
        /// <param name="connection"> The connection on which the command was run. Must not be null </param>
        internal static void SetEntityParameterValues(
            EntityCommand entityCommand, DbCommand storeProviderCommand, EntityConnection connection)
        {
            Debug.Assert(null != entityCommand);
            Debug.Assert(null != storeProviderCommand);
            Debug.Assert(null != connection);

            foreach (DbParameter storeParameter in storeProviderCommand.Parameters)
            {
                var direction = storeParameter.Direction;
                if (0 != (direction & ParameterDirection.Output))
                {
                    // if the entity command also defines the parameter, propagate store parameter value
                    // to entity parameter
                    var parameterOrdinal = entityCommand.Parameters.IndexOf(storeParameter.ParameterName);
                    if (0 <= parameterOrdinal)
                    {
                        var entityParameter = entityCommand.Parameters[parameterOrdinal];
                        var parameterValue = storeParameter.Value;
                        var parameterType = entityParameter.GetTypeUsage();
                        if (Helper.IsSpatialType(parameterType))
                        {
                            parameterValue = GetSpatialValueFromProviderValue(
                                parameterValue, (PrimitiveType)parameterType.EdmType, connection);
                        }
                        entityParameter.Value = parameterValue;
                    }
                }
            }
        }

        private static object GetSpatialValueFromProviderValue(
            object spatialValue, PrimitiveType parameterType, EntityConnection connection)
        {
            var providerServices = DbProviderServices.GetProviderServices(connection.StoreConnection);
            var storeItemCollection = (StoreItemCollection)connection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
            var spatialServices = providerServices.GetSpatialServices(storeItemCollection.StoreProviderManifestToken);
            if (Helper.IsGeographicType(parameterType))
            {
                return spatialServices.GeographyFromProviderValue(spatialValue);
            }
            else
            {
                Debug.Assert(Helper.IsGeometricType(parameterType));
                return spatialServices.GeometryFromProviderValue(spatialValue);
            }
        }

        // requires: all arguments must be given
        internal static EdmFunction FindFunctionImport(MetadataWorkspace workspace, string containerName, string functionImportName)
        {
            Debug.Assert(null != workspace && null != containerName && null != functionImportName);
            // find entity container
            EntityContainer entityContainer;
            if (!workspace.TryGetEntityContainer(containerName, DataSpace.CSpace, out entityContainer))
            {
                throw new InvalidOperationException(
                    Strings.EntityClient_UnableToFindFunctionImportContainer(
                        containerName));
            }

            // find function import
            EdmFunction functionImport = null;
            foreach (var candidate in entityContainer.FunctionImports)
            {
                if (candidate.Name == functionImportName)
                {
                    functionImport = candidate;
                    break;
                }
            }
            if (null == functionImport)
            {
                throw new InvalidOperationException(
                    Strings.EntityClient_UnableToFindFunctionImport(
                        containerName, functionImportName));
            }
            if (functionImport.IsComposableAttribute)
            {
                throw new InvalidOperationException(
                    Strings.EntityClient_FunctionImportMustBeNonComposable(containerName + "." + functionImportName));
            }
            return functionImport;
        }
    }
}
