// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DataSourceWizardExtension
{
    using System.ComponentModel.Composition;
    using EnvDTE;
    using Microsoft.VSDesigner.Data.DataSourceWizard.Interface;

    /// <summary>
    ///     Wizard Data
    /// </summary>
    [Export(typeof(IDataSourceWizardData))]
    internal class EdmDataSourceWizardData : IDataSourceWizardData
    {
        internal EdmDataSourceWizardData()
        {
            EDMProjectItem = null;
            ContainingProject = null;
            IsCancelled = false;
        }

        internal Project ContainingProject { get; set; }

        internal ProjectItem EDMProjectItem { get; set; }

        /// <summary>
        ///     Return true if the user cancelled our wizard.
        /// </summary>
        internal bool IsCancelled { get; set; }
    }
}
