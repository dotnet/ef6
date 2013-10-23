// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;

    /// <summary>
    ///     contains the ViewModel to support the Explorer View of the
    ///     conceptual and storage spaces
    /// </summary>
    internal class ExplorerViewModel : IExplorerViewModel
    {
        public ExplorerViewModel(EditingContext editingContext, ExplorerRootNode edmRootNode)
        {
            EditingContext = editingContext;
            EDMRootNode = edmRootNode;
        }

        #region Properties

        public EditingContext EditingContext { get; set; }

        public ExplorerEFElement RootNode
        {
            get { return EDMRootNode; }
        }

        public ExplorerRootNode EDMRootNode { get; set; }

        #endregion
    }

    /// <summary>
    ///     Extension methods for the IExplorerViewModel interface.
    /// </summary>
    internal static class IExplorerViewModelExtensions
    {
        internal static ExplorerRootNode EDMRootNode(this IExplorerViewModel viewModel)
        {
            return ((ExplorerViewModel)viewModel).EDMRootNode;
        }
    }
}
