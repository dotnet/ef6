// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    /// <summary>
    ///     provides a mechanism for root branches created via the TreeGridDesignerRootBranch
    ///     attribute to be sited with the current selection and column collection.
    /// </summary>
    internal interface ITreeGridDesignerInitializeBranch
    {
        /// <summary>
        ///     Called to initialize the branch
        /// </summary>
        /// <param name="selection">currently selected object</param>
        /// <param name="columns">columns currently displayed in the tree</param>
        bool Initialize(object selection, TreeGridDesignerColumnDescriptor[] columns);
    }
}
