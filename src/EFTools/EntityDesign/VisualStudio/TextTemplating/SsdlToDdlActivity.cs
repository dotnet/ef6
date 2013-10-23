// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.TextTemplating;

    /// <summary>
    ///     SsdlToDdlActivity that allows the transformation of SSDL to DDL using a TemplateActivity.
    ///     NOTE that this class should avoid any dependencies on any instance types (especially types instantiated by the
    ///     Entity Designer) in the Microsoft.Data.Entity.Design.* namespace except for
    ///     Microsoft.Data.Entity.Design.CreateDatabase.
    ///     This class exists in this project because of VS dependencies.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ddl")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
    public sealed class SsdlToDdlActivity : TemplateActivity
    {
        /// <summary>
        ///     A Windows Workflow <see cref="OutArgument{T}" /> that specifies the data definition language (DDL)
        ///     that is generated from the store schema definition language (SSDL) in the
        ///     <see cref="SsdlInput" /> and <see cref="ExistingSsdlInput" /> properties.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ddl")]
        public OutArgument<string> DdlOutput { get; set; }

        /// <summary>
        ///     A Windows Workflow <see cref="InArgument{T}" /> that specifies the existing store schema definition language (SSDL)
        ///     from which the data definition language (DDL) for dropping existing database objects is generated.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
        public InArgument<string> ExistingSsdlInput { get; set; }

        /// <summary>
        ///     A Windows Workflow <see cref="InArgument{T}" /> that specifies the store schema definition language (SSDL) from which the data
        ///     definition language (DDL) for creating new database objects is generated.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ssdl")]
        public InArgument<string> SsdlInput { get; set; }

        /// <summary>
        ///     Populates an <see cref="IDictionary{TKey, TValue}" /> that is used to provide inputs to a text template. This method can be overridden in derived classes to provide custom inputs.
        ///     These inputs are placed into the CallContext for use by the text template.
        /// </summary>
        /// <param name="context">The state of the current activity.</param>
        /// <param name="inputs">A dictionary that relates input names to input values for use by a text template.</param>
        protected override void OnGetTemplateInputs(NativeActivityContext context, IDictionary<string, object> inputs)
        {
            inputs.Add(EdmConstants.ssdlOutputName, SsdlInput.Get(context));
            inputs.Add(EdmConstants.existingSsdlInputName, ExistingSsdlInput.Get(context));

            var symbolResolver = context.GetExtension<SymbolResolver>();
            var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
            if (edmParameterBag == null)
            {
                throw new InvalidOperationException(Resources.DatabaseCreation_ErrorNoEdmParameterBag);
            }

            // Find the TargetVersion parameter
            var targetFrameworkVersion = edmParameterBag.GetParameter<Version>(EdmParameterBag.ParameterName.TargetVersion);
            if (targetFrameworkVersion == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorNoParameterDefined,
                        EdmParameterBag.ParameterName.TargetVersion.ToString()));
            }

            // Validate the TargetVersion parameter and add it as an input
            if (false == EntityFrameworkVersion.IsValidVersion(targetFrameworkVersion))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorInvalidTargetVersion, targetFrameworkVersion));
            }
            inputs.Add(EdmParameterBag.ParameterName.TargetVersion.ToString(), targetFrameworkVersion);

            // Add the Provider invariant name
            var providerInvariantName = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.ProviderInvariantName);
            if (String.IsNullOrEmpty(providerInvariantName))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorNoParameterDefined,
                        EdmParameterBag.ParameterName.ProviderInvariantName.ToString()));
            }
            inputs.Add(EdmParameterBag.ParameterName.ProviderInvariantName.ToString(), providerInvariantName);

            // Add the Provider manifest token (optional)
            var providerManifestToken = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.ProviderManifestToken);
            inputs.Add(EdmParameterBag.ParameterName.ProviderManifestToken.ToString(), providerManifestToken);

            // Add the Database Schema Name
            var databaseSchemaName = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.DatabaseSchemaName);
            if (String.IsNullOrEmpty(databaseSchemaName))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorNoParameterDefined,
                        EdmParameterBag.ParameterName.DatabaseSchemaName.ToString()));
            }
            inputs.Add(EdmParameterBag.ParameterName.DatabaseSchemaName.ToString(), databaseSchemaName);

            // Add the Database Name (Note: it's OK for this to be null e.g. some providers do not provide this)
            var databaseName = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.DatabaseName);
            inputs.Add(EdmParameterBag.ParameterName.DatabaseName.ToString(), databaseName);

            // Add the DDL Template Path (optional)
            var ddlTemplatePath = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.DDLTemplatePath);
            inputs.Add(EdmParameterBag.ParameterName.DDLTemplatePath.ToString(), ddlTemplatePath);
        }

        /// <summary>
        ///     Transforms a text template that is specified in the TemplatePath property by calling the Visual Studio STextTemplatingService.
        /// </summary>
        /// <param name="context">The state of the current activity.</param>
        protected override void Execute(NativeActivityContext context)
        {
            // inject the template path at runtime if an explicit one doesn't exist.
            if (TemplatePath.Get(context) == null)
            {
                var symbolResolver = context.GetExtension<SymbolResolver>();
                var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
                var ddlTemplatePath = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.DDLTemplatePath);

                if (String.IsNullOrEmpty(ddlTemplatePath))
                {
                    // TemplatePath must either be specified at design-time in the workflow file or at runtime, passed in through EdmParameterBag.
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, Resources.DatabaseCreation_NoDDLTemplatePathSpecified, DisplayName));
                }

                TemplatePath.Set(context, ddlTemplatePath);
            }

            base.Execute(context);

            Debug.Assert(!String.IsNullOrEmpty(TemplateOutput), "DDL returned from SsdlToDdl template is null or empty...");
            if (TemplateOutput == null)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplateOutputNotSet, DisplayName));
            }

            DdlOutput.Set(context, TemplateOutput);
        }
    }
}
