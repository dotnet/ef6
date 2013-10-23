// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Provides values for the parameters that are defined in the EdmParameterBag.ParameterName enumeration.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public class EdmParameterBag
    {
        /// <summary>
        ///     An enumeration of parameter names that are used by classes in the Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators namespace when generating a database from a conceptual model.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Changing this would affect the public .tt files")]
        public enum ParameterName
        {
            /// <summary>
            ///     The SynchronizationContext that represents the state of the user interface thread in Visual Studio.
            /// </summary>
            SynchronizationContext,

            /// <summary>
            ///     The name of the IAssemblyLoader parameter object used to resolve and load an assembly given its name.
            /// </summary>
            AssemblyLoader,

            /// <summary>
            ///     The targeted version of the Entity Framework.
            /// </summary>
            TargetVersion,

            /// <summary>
            ///     The invariant name of the provider.
            /// </summary>
            ProviderInvariantName,

            /// <summary>
            ///     The provider's manifest token.
            /// </summary>
            ProviderManifestToken,

            /// <summary>
            ///     The provider connection string.
            /// </summary>
            ProviderConnectionString,

            /// <summary>
            ///     The name of the schema of the generated database.
            /// </summary>
            DatabaseSchemaName,

            /// <summary>
            ///     The name of the generated database.
            /// </summary>
            DatabaseName,

            /// <summary>
            ///     The path to the text template used to generate data definition language (DDL).
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DDL")]
            DDLTemplatePath,

            /// <summary>
            ///     The path to the .edmx file from which the Generate Database Wizard was launched.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edmx")]
            EdmxPath
        }

        private readonly Dictionary<ParameterName, object> _parameterBag;

        /// <summary>
        ///     Constructor for EdmParameterBag
        /// </summary>
        /// <param name="syncContext">An optional SynchronizationContext that represents the state of the user interface thread in Visual Studio.</param>
        /// <param name="assemblyLoader">An optional IAssemblyLoader used to resolve and load an assembly given its name.</param>
        /// <param name="targetVersion">The targeted version of the Entity Framework.</param>
        /// <param name="providerInvariantName">The invariant name of the provider.</param>
        /// <param name="providerManifestToken">The provider's manifest token.</param>
        /// <param name="providerConnectionString">An optional provider connection string.</param>
        /// <param name="databaseSchemaName">The name of the schema of the generated database.</param>
        /// <param name="databaseName">The name of the generated database.</param>
        /// <param name="ddlTemplatePath">The path to the text template used to generate data definition language (DDL).</param>
        /// <param name="edmxPath">An optional path to the .edmx file from which the Generate Database Wizard was launched.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ddl")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edmx")]
        public EdmParameterBag(
            SynchronizationContext syncContext,
            IAssemblyLoader assemblyLoader,
            Version targetVersion,
            string providerInvariantName,
            string providerManifestToken,
            string providerConnectionString,
            string databaseSchemaName,
            string databaseName,
            string ddlTemplatePath,
            string edmxPath)
        {
            _parameterBag = new Dictionary<ParameterName, object>();
            _parameterBag.Add(ParameterName.SynchronizationContext, syncContext);
            _parameterBag.Add(ParameterName.AssemblyLoader, assemblyLoader);
            _parameterBag.Add(ParameterName.TargetVersion, targetVersion);
            _parameterBag.Add(ParameterName.ProviderInvariantName, providerInvariantName);
            _parameterBag.Add(ParameterName.ProviderManifestToken, providerManifestToken);
            _parameterBag.Add(ParameterName.ProviderConnectionString, providerConnectionString);
            _parameterBag.Add(ParameterName.DatabaseSchemaName, databaseSchemaName);
            _parameterBag.Add(ParameterName.DatabaseName, databaseName);
            _parameterBag.Add(ParameterName.DDLTemplatePath, ddlTemplatePath);
            _parameterBag.Add(ParameterName.EdmxPath, edmxPath);
        }

        /// <summary>
        ///     Returns the value of the parameter for the specified <see cref="EdmParameterBag.ParameterName" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter with the name parameterName.</typeparam>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>
        ///     The value of the parameter for the specified <see cref="EdmParameterBag.ParameterName" />.
        /// </returns>
        public T GetParameter<T>(ParameterName parameterName) where T : class
        {
            object paramValue = null;
            _parameterBag.TryGetValue(parameterName, out paramValue);
            return paramValue as T;
        }
    }
}
