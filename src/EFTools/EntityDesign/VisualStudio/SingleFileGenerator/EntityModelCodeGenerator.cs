// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.SingleFileGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     Our SFG-based code generator works by:
    ///     1. Reading the CodeGenerationStrategy option in the EDMX file
    ///     2. If the CodeGenerationStrategy is set to 'Default' then we will proceed with codegen:
    ///     3. Call System.Data.Entity.Design CodeGen APIs
    /// </summary>
    [ComVisible(true)]
    [Guid("A58BFFCF-B9BD-4904-9248-0936C15D178A")]
    public class EntityModelCodeGenerator : BaseCodeGeneratorWithSite
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public const string CodeGenToolName = "EntityModelCodeGenerator";
        private const string ExcludeExtension = ".exclude";
        private static readonly Dictionary<uint, string> _oldInputNames = new Dictionary<uint, string>();

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="itemId"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        /// <param name="oldInputName"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        [SuppressMessage("Microsoft.Interoperability", "CA1407:AvoidStaticMembersInComVisibleTypes")]
        public static void AddNameOfItemToBeRenamed(uint itemId, string oldInputName)
        {
            _oldInputNames.Add(itemId, oldInputName);
        }

        /// <summary>
        ///     Gets the default extension of the output file from the CodeDomProvider
        /// </summary>
        /// <returns></returns>
        protected override string DefaultExtensionString
        {
            get
            {
                var extension = string.Empty;

                var codeDom = CodeProvider;

                // the vsmdCodeDomProvider will be null in some error situations (eg, if the user added an EDMX file to a web site, but didn't
                // put it in App_Code.  So Don't assert here. 
                if (codeDom != null)
                {
                    extension = codeDom.FileExtension;
                    if (extension != null
                        && extension.Length > 0)
                    {
                        extension = "." + extension.TrimStart(".".ToCharArray());
                    }
                }
                return ".Designer" + extension;
            }
        }

        /// <summary>
        ///     Calls the CodeGen API to generate the code file from the CSDL file
        /// </summary>
        /// <param name="inputFileName">The full path of the CSDL file; this is fed in from the ProjectItem</param>
        /// <param name="inputFileContent">The contents of the CSDL file - this should always be used so that we can gen off of in memory documents</param>
        /// <param name="defaultNamespace">The default namespace.</param>
        /// <returns>null implies error, else the contents of the file</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsErrorList.BringToFront")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent, string defaultNamespace)
        {
            byte[] generatedBytes = null;
            var generatedCode = string.Empty;

            var projectItem = SiteServiceProvider.GetService(typeof(ProjectItem)) as ProjectItem;

            // Check if this is a rename operation here. The SFG gets called during a rename on website projects
            // and attempts to rollback the code-behind file if the SFG fails, but before the rename of the
            // code-behind file. We don't want to hose the existing code-behind file so we return back the bytes
            // of the previous code-behind file.
            if (projectItem != null
                && IsAfterARename(projectItem))
            {
                var generatedCodeFile = GetCodeGenFilePathFromInputFile(inputFileName);
                if (File.Exists(generatedCodeFile))
                {
                    return GetBytesOfExistingCodeGenFile(generatedCodeFile);
                }
            }

            // init Language
            var languageOption = LanguageOption.GenerateCSharpCode;
            try
            {
                var defaultExtension = DefaultExtensionString.ToUpperInvariant();
                if (defaultExtension.Contains(".CS"))
                {
                    languageOption = LanguageOption.GenerateCSharpCode;
                }
                else if (defaultExtension.Contains(".VB"))
                {
                    languageOption = LanguageOption.GenerateVBCode;
                }
                else
                {
                    throw new NotSupportedException(Strings.UnknownLanguage);
                }
            }
            catch (Exception e)
            {
                string commentString;
                if (languageOption == LanguageOption.GenerateVBCode)
                {
                    commentString = "' " + e.Message;
                }
                else
                {
                    commentString = "// " + e.Message;
                }
                generatedBytes = Encoding.UTF8.GetBytes(commentString);
                return generatedBytes;
            }

            // ensure that our package is loaded
            try
            {
                PackageManager.LoadEDMPackage(SiteServiceProvider);
            }
            catch (Exception)
            {
                // It would be nice to add an error in the error list, but you need an IServiceProvider to do this
                // We use our package usually for this, but here, our pacakge failed to load.  Raise a message box
                VsUtils.ShowErrorDialog(Resources.LoadOurPackageError);

                string commentString;
                if (languageOption == LanguageOption.GenerateVBCode)
                {
                    commentString = "' " + Resources.LoadOurPackageError;
                }
                else
                {
                    commentString = "// " + Resources.LoadOurPackageError;
                }
                generatedBytes = Encoding.UTF8.GetBytes(commentString);
                return generatedBytes;
            }

            if (InputFileHasContent(inputFileContent))
            {
                var projectItemUri = Utils.FileName2Uri(inputFileName);

                ModelManager modelManager = PackageManager.Package.ModelManager;
                ModelManager tempModelManager = null;

                try
                {
                    // First check the ModelManager to see if the artifact was loaded by the designer or if it was persisted from an earlier code-gen attempt
                    var artifact = modelManager.GetArtifact(projectItemUri);
                    if (artifact == null)
                    {
                        tempModelManager = new EntityDesignModelManager(new EFArtifactFactory(), new VSArtifactSetFactory());
                        artifact = tempModelManager.GetNewOrExistingArtifact(
                            projectItemUri, new StandaloneXmlModelProvider(PackageManager.Package));
                        Debug.Assert(artifact != null, "We should have created the artifact from the temporary model manager");

                        // Since we created the artifact purely for code-gen, we need to mark it as such so the designer knows not to re-use this
                        // artifact later.
                        foreach (var codeGenArtifact in artifact.ArtifactSet.Artifacts)
                        {
                            codeGenArtifact.IsCodeGenArtifact = true;
                        }
                    }

                    // First check if the 'CodeGenerationStrategy' option is set to 'Default'. 
                    // If there's no or empty value, we assume 'Default'.
                    var codeGenStrategy = ModelHelper.GetDesignerPropertyValueFromArtifact(
                        OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeCodeGenerationStrategy, artifact);
                    if (String.IsNullOrEmpty(codeGenStrategy)
                        || codeGenStrategy.Equals(Resources.Default))
                    {
                        // navigate to the conceptual element
                        var cModel = artifact.ConceptualModel();
                        if (cModel != null)
                        {
                            IList<EdmSchemaError> generatorErrors;

                            using (var output = new StringWriter(CultureInfo.InvariantCulture))
                            {
                                // set up namespace to use for code-gen. defaultNamespace is computed by VS to account for
                                // folder path in the project, custom tool namespace, and differences between C#, VB, and web-site projects.
                                defaultNamespace = GetCodeNamespace(defaultNamespace, artifact);

                                // generate code
                                generatorErrors =
                                    new LegacyCodeGenerationDriver(languageOption, artifact.SchemaVersion)
                                        .GenerateCode(artifact, defaultNamespace, output);

                                generatedCode = output.ToString();
                            }

                            // TODO: pass on validation to our designer so that we validate the MSL and SSDL in addition to the CSDL
                            // Insert new errors into the ErrorList window
                            ProcessErrors(generatorErrors, projectItemUri, artifact);

                            if (generatorErrors.Count > 0
                                || string.IsNullOrEmpty(generatedCode))
                            {
                                // We do not want to bring the error list to the front if there are code generation errors
                                // since this could interrupt the editing of the model.
                                // in case of errors generate a comment in the code which will indicate 
                                // that the file failed to generate properly
                                generatedCode = GetCodeGenerationErrorComment(languageOption, inputFileName);
                            }

                            generatedBytes = Utils.StringToBytes(generatedCode, Encoding.UTF8);
                        }
                    }
                    else
                    {
                        generatedCode = GetCodeGenerationDisabledComment(languageOption, inputFileName);
                        generatedBytes = Utils.StringToBytes(generatedCode, Encoding.UTF8);
                    }
                }
                catch (Exception e)
                {
                    GeneratorErrorCallback(false, 0, e.Message, 0, 0);
                    ErrorList.BringToFront();
                }
                finally
                {
                    if (tempModelManager != null)
                    {
                        tempModelManager.Dispose();
                        tempModelManager = null;
                    }
                }
            }
            return generatedBytes;
        }

        private string GetCodeNamespace(string defaultNamespace, EFArtifact artifact)
        {
            var ns = (defaultNamespace == null ? String.Empty : defaultNamespace.Trim());

            // This is a work-around for Astoria bug SQLBUDT 639143.  They choke when we there is no code namespace, and it is too late for them 
            // to fix this bug in the netfx, so we have the below work-around. 
            if (string.IsNullOrEmpty(ns))
            {
                if (VsUtils.GetProjectKind(Project) == VsUtils.ProjectKind.VB)
                {
                    var vbRootNamespace = VsUtils.GetProjectPropertyByName(Project, "RootNamespace") as string;
                    if (string.IsNullOrEmpty(vbRootNamespace))
                    {
                        ns = artifact.ConceptualModel().Namespace.Value;
                    }
                }
                else
                {
                    ns = artifact.ConceptualModel().Namespace.Value;
                }
            }

            return ns;
        }

        private static bool InputFileHasContent(string inputFileContents)
        {
            if (inputFileContents != null)
            {
                for (var i = 0; i < inputFileContents.Length; i++)
                {
                    if (!Char.IsWhiteSpace(inputFileContents[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ProcessErrors(IList<EdmSchemaError> schemaErrors, Uri projectItemUri, EFArtifact artifact)
        {
            // since we will inevitably add *all* errors from the artifact set into the error list, we can easily
            // clear the entire error list here even though we just validate the CSDL.
            var errorList = ErrorListHelper.GetSingleDocErrorList(projectItemUri);
            if (errorList != null)
            {
                errorList.Clear();
            }

            if (artifact == null)
            {
                Debug.Fail("Where is the artifact? We should have created it at least through the temporary model manager");
                return;
            }

            // we have to remove both the RMV and SFG CSDL errors to prevent duplicates
            artifact.ArtifactSet.RemoveErrorsForArtifact(artifact, ErrorClass.Runtime_CSDL);

            // add all CSDL-based SFG errors to the artifact set
            if (schemaErrors.Count > 0)
            {
                foreach (var error in schemaErrors)
                {
                    // construct an ErrorInfo with correct line/column number and add it to the artifact set. Note that the CodeGen EdmSchemaError line
                    // refers to the line of the error inside the CSDL, so to get the line of the error in the edmx file we have to offset it by the line
                    // number where the conceptual model begins.
                    var edmxErrorLine = error.Line + artifact.ConceptualModel().GetLineNumber();
                    var efobject = artifact.FindEFObjectForLineAndColumn(edmxErrorLine, error.Column);
                    var errorInfo = new ErrorInfo(
                        GetErrorInfoSeverity(error), error.Message, efobject, error.ErrorCode, ErrorClass.Runtime_CSDL);
                    artifact.ArtifactSet.AddError(errorInfo);
                }
            }

            // get all the ErrorInfos for this artifact and add it to the error list
            var artifactSet = artifact.ArtifactSet;
            Debug.Assert(artifactSet != null, "Where is the artifact set for this artifact?");
            if (artifactSet != null)
            {
                var errors = artifactSet.GetArtifactOnlyErrors(artifact);
                if (errors.Count > 0)
                {
                    // resolve the hierarchy and item id for adding to the error list
                    var hierarchy = VSHelpers.GetVsHierarchy(ProjectItem.ContainingProject, Services.ServiceProvider);
                    var itemId = VsUtils.GetProjectItemId(hierarchy, ProjectItem);

                    Debug.Assert(hierarchy != null, "Why isn't there a hierarchy associated with this project item?");
                    Debug.Assert(itemId != VSConstants.VSITEMID_NIL, "There should be an item ID associated with this project item");

                    if (hierarchy != null
                        && itemId != VSConstants.VSITEMID_NIL)
                    {
                        ErrorListHelper.AddErrorInfosToErrorList(errors, hierarchy, itemId);
                    }
                }
            }
        }

        private static ErrorInfo.Severity GetErrorInfoSeverity(EdmSchemaError error)
        {
            switch (error.Severity)
            {
                case EdmSchemaErrorSeverity.Error:
                    return ErrorInfo.Severity.ERROR;
                case EdmSchemaErrorSeverity.Warning:
                    return ErrorInfo.Severity.WARNING;
                default:
                    Debug.Fail("Unexpected value for EdmSchemaErrorSeverity");
                    return ErrorInfo.Severity.ERROR;
            }
        }

        private static string GetCodeGenerationErrorComment(LanguageOption languageOption, string inputFileName)
        {
            if (LanguageOption.GenerateVBCode == languageOption)
            {
                return string.Format(CultureInfo.CurrentCulture, Strings.CodeGenerationFailureCommentVB, inputFileName);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, Strings.CodeGenerationFailureCommentCSharp, inputFileName);
            }
        }

        private static string GetCodeGenerationDisabledComment(LanguageOption languageOption, string inputFileName)
        {
            if (LanguageOption.GenerateVBCode == languageOption)
            {
                return string.Format(CultureInfo.CurrentCulture, Strings.CodeGenerationDisabledCommentVB, inputFileName);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, Strings.CodeGenerationDisabledCommentCSharp, inputFileName);
            }
        }

        private string GetCodeGenFilePathFromInputFile(string inputFileName)
        {
            // during an exclude, the SFG gets called *in between* the renames of the input file and the code-gen file.
            // therefore, if "Model.edmx.exclude" gets passed in as the input file, there won't yet be a "Model.Designer.cs.exclude".
            // we have to return back "Model.Designer.cs". If "Model.edmx" is passed in and both files were originally excluded,
            // then "Model.Designer.cs.exclude" exists.
            var extension = Path.GetExtension(inputFileName);
            if (extension.Equals(ExcludeExtension, StringComparison.CurrentCultureIgnoreCase))
            {
                // remove the exclude extension and add the default extension ('.Designer.cs')
                var inputFilePathWithoutExclude = inputFileName.Substring(
                    0, inputFileName.LastIndexOf(ExcludeExtension, StringComparison.CurrentCultureIgnoreCase));

                // if there is already an excluded model file, then the new excluded model input file 
                // will be something like: Model.edmx.2.exclude. We need to strip off this extension as well.
                var testNumericExtension = Path.GetExtension(inputFilePathWithoutExclude);
                int numericExtension;
                if (!String.IsNullOrEmpty(testNumericExtension)
                    && int.TryParse(testNumericExtension.Substring(1), NumberStyles.Any, CultureInfo.CurrentCulture, out numericExtension))
                {
                    inputFilePathWithoutExclude = inputFileName.Substring(
                        0, inputFilePathWithoutExclude.LastIndexOf(testNumericExtension, StringComparison.CurrentCultureIgnoreCase));
                }

                // change the .edmx extension to the default codegen extension (.Designer.cs)
                return Path.ChangeExtension(inputFilePathWithoutExclude, DefaultExtensionString);
            }

            // First check to see if there is a codegen file without the exclude extension.
            var codeGenPathWithoutExclude = Path.ChangeExtension(inputFileName, DefaultExtensionString);
            if (!File.Exists(codeGenPathWithoutExclude))
            {
                // if there is a codegen file with the extension, then return that
                var codeGenPathWithExclude = codeGenPathWithoutExclude + ExcludeExtension;
                if (File.Exists(codeGenPathWithExclude))
                {
                    return codeGenPathWithExclude;
                }
            }

            // not either the DefaultExtension or the exclude extension? User must have messed with the extension for some reason. Create
            // a normal codegen file with the default extension.
            return codeGenPathWithoutExclude;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static byte[] GetBytesOfExistingCodeGenFile(string generatedCodeFile)
        {
            var resultAsBytes = new byte[] { };
            try
            {
                using (var fileStream = File.OpenRead(generatedCodeFile))
                {
                    if (fileStream != null
                        && fileStream.Length != 0)
                    {
                        resultAsBytes = new byte[(int)fileStream.Length];
                        fileStream.Read(resultAsBytes, 0, resultAsBytes.Length);
                    }
                }
            }
            catch
            {
            }
            return resultAsBytes;
        }

        private bool IsAfterARename(ProjectItem projectItem)
        {
            var hier = SiteServiceProvider.GetService(typeof(IVsHierarchy)) as IVsHierarchy;
            if (hier != null)
            {
                // check if there is an item in our oldInputNames cache corresponding to the itemID.
                string oldInputName;
                var itemId = VsUtils.GetProjectItemId(hier, projectItem);
                if (_oldInputNames.TryGetValue(itemId, out oldInputName))
                {
                    // double-check that the new filename is not equal to the old filename. If it's not,
                    // then this is a rename
                    var newName = projectItem.get_FileNames(1);
                    if (!String.IsNullOrEmpty(newName)
                        && !newName.Equals(oldInputName))
                    {
                        _oldInputNames.Remove(itemId);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
