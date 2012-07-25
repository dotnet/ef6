// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.Utilities
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Management.Automation;
    using EnvDTE;

    /// <summary>
    /// Provides a way of dispatching specific calls form the PowerShell commands'
    /// AppDomain to the Visual Studio's main AppDomain.
    /// </summary>
    [SuppressMessage("Microsoft.Contracts", "CC1036",
        Justification = "Due to a bug in code contracts IsNullOrWhiteSpace isn't recognized as pure.")]
    [CLSCompliant(false)]
    public class DomainDispatcher : MarshalByRefObject
    {
        private readonly PSCmdlet _cmdlet;
        private readonly DTE _dte;

        public DomainDispatcher(PSCmdlet cmdlet)
        {
            Contract.Requires(cmdlet != null);

            _cmdlet = cmdlet;
            _dte = (DTE)cmdlet.GetVariableValue("DTE");
        }

        public void WriteLine(string text)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(text));

            _cmdlet.Host.UI.WriteLine(text);
        }

        public void WriteWarning(string text)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(text));

            _cmdlet.WriteWarning(text);
        }

        public void WriteVerbose(string text)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(text));

            _cmdlet.WriteVerbose(text);
        }

        public void OpenFile(string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));

            _dte.ItemOperations.OpenFile(fileName);
        }

        public void NewTextFile(string text, string item = @"General\Text File")
        {
            var window = _dte.ItemOperations.NewFile(item);
            var textDocument = (TextDocument)window.Document.Object("TextDocument");
            var editPoint = textDocument.StartPoint.CreateEditPoint();
            editPoint.Insert(text);
        }
    }
}
