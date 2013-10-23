// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.TextTemplating
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Remoting.Messaging;
    using System.Text.RegularExpressions;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TextTemplating.VSHost;

    /// <summary>
    ///     TemplateActivity that allows the transformation of a T4 template within a WF workflow.
    ///     NOTE that this class should avoid any dependencies on any instance types (especially types instantiated by the
    ///     Entity Designer) in the Microsoft.Data.Entity.Design.* namespace except for
    ///     Microsoft.Data.Entity.Design.CreateDatabase.
    ///     This class exists in this project because of VS dependencies.
    /// </summary>
    public abstract class TemplateActivity : NativeActivity
    {
        private static readonly Regex _assemblyDirectiveRegex = new Regex(@"<#@\s*assembly\s+name=""(.*\$\(.*\).*)""\s*#>");
        private const string AssemblyDirectiveFormat = @"<#@ assembly name=""{0}"" #>";
        private string _edmxPath;

        /// <summary>
        ///     The output of the template that is specified by the <see cref="TemplatePath" /> property.
        /// </summary>
        protected string TemplateOutput { get; set; }

        /// <summary>
        ///     The path of the text template being processed.
        /// </summary>
        public InArgument<string> TemplatePath { get; set; }

        /// <summary>
        ///     Populates an <see cref="System.Collections.IDictionary" /> that is used to provide inputs to a text template. This method can be overridden in derived classes to provide custom inputs.
        ///     These inputs are placed into <see cref="CallContext" /> for use by the text template.
        /// </summary>
        /// <param name="context">The state of the current activity.</param>
        /// <param name="inputs">A dictionary that relates input names to input values for use by a text template.</param>
        protected abstract void OnGetTemplateInputs(NativeActivityContext context, IDictionary<string, object> inputs);

        /// <summary>
        ///     Transforms a text template that is specified in the <see cref="TemplatePath" /> property by calling the Visual Studio STextTemplatingService.
        /// </summary>
        /// <param name="context">The state of the current activity.</param>
        protected override void Execute(NativeActivityContext context)
        {
            var templateInputs = new Dictionary<string, object>();

            var symbolResolver = context.GetExtension<SymbolResolver>();
            var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
            if (edmParameterBag == null)
            {
                throw new InvalidOperationException(Resources.DatabaseCreation_ErrorNoEdmParameterBag);
            }

            // Add the EDMX path as a template input if it was populated as a parameter to the workflow
            _edmxPath = edmParameterBag.GetParameter<string>(EdmParameterBag.ParameterName.EdmxPath);
            if (_edmxPath != null)
            {
                templateInputs.Add(EdmParameterBag.ParameterName.EdmxPath.ToString(), _edmxPath);
            }

            // Call OnGetTemplateInputs, which will ask any derived class for inputs they would like the
            // templates to have access to
            OnGetTemplateInputs(context, templateInputs);

            // Retrieve the template name specified on the workflow file
            var unresolvedTemplatePath = TemplatePath.Get(context);
            if (String.IsNullOrEmpty(unresolvedTemplatePath))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplatePathNotSet, DisplayName));
            }

            // Add the template inputs to CallContext
            foreach (var inputName in templateInputs.Keys)
            {
                CallContext.LogicalSetData(inputName, templateInputs[inputName]);
            }

            try
            {
                TemplateOutput = ProcessTemplate(unresolvedTemplatePath);
            }
            finally
            {
                // We have to make sure we clear the CallContext data slots we set
                foreach (var inputName in templateInputs.Keys)
                {
                    CallContext.FreeNamedDataSlot(inputName);
                }
            }
        }

        /// <summary>
        ///     Process a T4 template using Visual Studio's text templating service, given a path that could contain macros (i.e. "$(DevEnvDir)\...").
        ///     NOTE: Template paths that are not files or are UNC paths are not allowed
        /// </summary>
        /// <param name="templatePath">Template's file path which may contain project-based macros</param>
        /// <returns>The output of processing the template.</returns>
        protected string ProcessTemplate(string templatePath)
        {
            // Attempt to resolve the full template path if it contains any macros, using
            // the edmx path to get the project and using the project's defined macros.
            Project project = null;
            if (!String.IsNullOrEmpty(_edmxPath))
            {
                project = VSHelpers.GetProjectForDocument(_edmxPath);
            }

            // Resolve and validate the template file path
            var errorMessages = new DatabaseGenerationEngine.PathValidationErrorMessages
                {
                    NullFile = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplatePathNotSet, DisplayName),
                    NonValid = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplatePathNotValid, DisplayName),
                    ParseError = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ExceptionParsingTemplateFilePath, DisplayName),
                    NonFile = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplatePathNonFile, DisplayName),
                    NotInProject = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTemplateFileNotInProject, DisplayName),
                    NonExistant = String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_TemplateFileNotExists, DisplayName)
                };

            var templateFileInfo = DatabaseGenerationEngine.ResolveAndValidatePath(
                project,
                templatePath,
                errorMessages);

            var resolvedTemplatePath = templateFileInfo.FullName;

            // The workflow will catch any IO exceptions and wrap them in a friendly message
            var templateContents = File.ReadAllText(resolvedTemplatePath);

            // Since we are leveraging the VS T4 Host, we will have to ask the environment how to resolve any other assemblies.
            // The VS T4 Host only resolves: (a) rooted paths (b) GAC'd dlls and (c) Referenced dlls in the project, if a hierarchy is provided
            // This is a little risky here, but we will use a strict regular expression to replace the assembly references with resolved paths
            templateContents = Regex.Replace(
                templateContents, _assemblyDirectiveRegex.ToString(), match =>
                    {
                        Debug.Assert(
                            match.Groups.Count == 2,
                            "If we have a match, we should only ever have two groups, the last of which is the assembly path");
                        if (match.Groups.Count == 2)
                        {
                            var resolvedAssemblyPath = match.Groups[1].Value;

                            // project can be null if we are running via tests. In this case, the custom macros will
                            // be used
                            resolvedAssemblyPath = VsUtils.ResolvePathWithMacro(
                                project, resolvedAssemblyPath,
                                new Dictionary<string, string>
                                    {
                                        { VsUtils.DevEnvDirMacroName, VsUtils.GetVisualStudioInstallDir() },
                                        { ExtensibleFileManager.EFTOOLS_USER_MACRONAME, ExtensibleFileManager.UserEFToolsDir.FullName },
                                        { ExtensibleFileManager.EFTOOLS_VS_MACRONAME, ExtensibleFileManager.VSEFToolsDir.FullName }
                                    });
                            return String.Format(CultureInfo.InvariantCulture, AssemblyDirectiveFormat, resolvedAssemblyPath);
                        }
                        return match.Value;
                    });

            var textTemplatingService = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
            Debug.Assert(textTemplatingService != null, "ITextTemplating could not be found from the IServiceProvider");
            if (textTemplatingService == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorTextTemplatingServiceNotFound, resolvedTemplatePath));
            }
            // Process the template, keeping track of errors
            var templateCallback = new TemplateCallback();
            var templateOutput = String.Empty;

            textTemplatingService.BeginErrorSession();
            templateOutput = textTemplatingService.ProcessTemplate(resolvedTemplatePath, templateContents, templateCallback, null);
            if (textTemplatingService.EndErrorSession())
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, Resources.TemplateErrorsEncountered, resolvedTemplatePath,
                        templateCallback.ErrorStringBuilder));
            }
            return templateOutput;
        }
    }
}
