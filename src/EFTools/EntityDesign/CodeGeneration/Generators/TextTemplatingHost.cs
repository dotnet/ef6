namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating;
    using Microsoft.VisualStudio.TextTemplating.VSHost;

    internal class TextTemplatingHost : ITextTemplatingEngineHost, ITextTemplatingSessionHost
    {
        public IList<string> StandardAssemblyReferences
        {
            get { return new[] { "System" }; }
        }

        public IList<string> StandardImports
        {
            get { return new[] { "System" }; }
        }

        public string TemplateFile { get; set; }
        public ITextTemplatingSession Session { get; set; }
        internal virtual ITextTemplatingCallback Callback { get; private set; }

        public object GetHostOption(string optionName)
        {
            return null;
        }

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            throw new NotImplementedException();
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            Debug.Assert(errors != null, "errors is null.");

            if (Callback == null)
            {
                return;
            }

            foreach (CompilerError error in errors)
            {
                Callback.ErrorCallback(error.IsWarning, error.ErrorText, error.Line, error.Column);
            }
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            // NOTE: This runs templates in the main VS app domain
            return AppDomain.CurrentDomain;
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            Debug.Assert(!string.IsNullOrEmpty(assemblyReference), "assemblyReference is null or empty.");

            if (Path.IsPathRooted(assemblyReference))
            {
                return assemblyReference;
            }

            // Only resolve assemblies already loaded by VS
            return (from a in AppDomain.CurrentDomain.GetAssemblies()
                    where a.FullName == assemblyReference || a.GetName().Name == assemblyReference
                    select a.Location)
                .FirstOrDefault() ?? string.Empty;
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            throw new NotImplementedException();
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            throw new NotImplementedException();
        }

        public string ResolvePath(string path)
        {
            throw new NotImplementedException();
        }

        public void SetFileExtension(string extension)
        {
            if (Callback != null)
            {
                Callback.SetFileExtension(extension);
            }
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            if (Callback != null)
            {
                Callback.SetOutputEncoding(encoding, fromOutputDirective);
            }
        }

        public ITextTemplatingSession CreateSession()
        {
            return new TextTemplatingSession();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback callback = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(inputFile), "inputFile is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(content), "content is null or empty.");

            TemplateFile = inputFile;
            Callback = callback;

            return new Engine().ProcessTemplate(content, this);
        }
    }
}
