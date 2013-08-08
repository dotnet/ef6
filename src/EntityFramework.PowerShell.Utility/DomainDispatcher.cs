// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Management.Automation;
    using EnvDTE;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// 
    /// Provides a way of dispatching specific calls form the PowerShell commands'
    /// AppDomain to the Visual Studio's main AppDomain.
    /// </summary>
    [CLSCompliant(false)]
    public class DomainDispatcher : MarshalByRefObject
    {
        private readonly PSCmdlet _cmdlet;
        private readonly DTE _dte;

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Initializes a new instance of the <see cref="DomainDispatcher"/> class.
        /// </summary>
        public DomainDispatcher()
        {
            // Testing    
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Initializes a new instance of the <see cref="DomainDispatcher"/> class.
        /// </summary>
        /// <param name="cmdlet">The PowerShell command that is being executed.</param>
        public DomainDispatcher(PSCmdlet cmdlet)
        {
            // Not using Check here because this assembly is very small and without resources
            if (cmdlet == null)
            {
                throw new ArgumentNullException("cmdlet");
            }

            _cmdlet = cmdlet;
            _dte = (DTE)cmdlet.GetVariableValue("DTE");
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Writes a line of text to the UI.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteLine(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.Host.UI.WriteLine(text);
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Writes a warning to the UI.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteWarning(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.WriteWarning(text);
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Writes verbose information to the UI.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteVerbose(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.WriteVerbose(text);
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Opens the file given file in Visual Studio.
        /// </summary>
        /// <param name="fileName">Path of the file to open.</param>
        public virtual void OpenFile(string fileName)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            _dte.ItemOperations.OpenFile(fileName);
        }

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// 
        /// Opens a new text file in Visual Studio without creating the file on disk.
        /// </summary>
        /// <param name="text">The text to add to the new file.</param>
        /// <param name="item">
        /// The virtual path to the item template to use for the new file based on the tree nodes 
        /// from the left pane of the new item dialog box and the item name from the right pane.
        /// </param>
        public void NewTextFile(string text, string item = @"General\Text File")
        {
            var window = _dte.ItemOperations.NewFile(item);
            var textDocument = (TextDocument)window.Document.Object("TextDocument");
            var editPoint = textDocument.StartPoint.CreateEditPoint();
            editPoint.Insert(text);
        }
    }
}
