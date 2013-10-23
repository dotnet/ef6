// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    /// <summary>
    ///     Interface that contains extra public APIs used by the
    ///     operation designer extensions to the VirtualTree control.
    /// </summary>
    internal interface ITreeGridDesignerBranch
    {
        /// <summary>
        ///     Gives the branch a chance to handle key press events
        /// </summary>
        ProcessKeyResult ProcessKeyPress(int row, int column, char keyPressed, Keys modifiers);

        /// <summary>
        ///     Gives the branch a chance to handle key down events
        /// </summary>
        ProcessKeyResult ProcessKeyDown(int row, int column, KeyEventArgs e);

        /// <summary>
        ///     Retrieves info about whether the branch supported editing/viewing
        ///     the given cell.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        TreeGridDesignerValueSupportedStates GetValueSupported(int row, int column);

        /// <summary>
        ///     Specifies the default command that should be executed for the specified index.
        ///     The default command is executed when the tree control is double-clicked on.
        /// </summary>
        CommandID GetDefaultAction(int row);

        /// <summary>
        ///     Indicates that a new creator node should be inserted at the given index
        /// </summary>
        /// <param name="absIndex">index to insert</param>
        /// <param name="creatorNodeIndex">specifies the creator node to insert</param>
        void InsertCreatorNode(int row, int creatorNodeIndex);

        /// <summary>
        ///     Called to indicate that the branch should end insert mode.
        /// </summary>
        /// <param name="row">row index specifying the insertion</param>
        void EndInsert(int row);

        /// <summary>
        ///     Gets/sets read-only state of the entire branch.  Text in read-only branches appears grayed-out
        ///     and cannot be edited.  Creator nodes also do not appear.
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        ///     Deletes a cell in the branch
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void Delete(int row, int column);

        /// <summary>
        ///     Returns the component that was used to initialize the branch
        /// </summary>
        /// <returns></returns>
        object GetBranchComponent();

        /// <summary>
        ///     Called to indicate column changes that require modificating the branch
        /// </summary>
        /// <param name="args">Info about required modifications</param>
        void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args);
    }
}
