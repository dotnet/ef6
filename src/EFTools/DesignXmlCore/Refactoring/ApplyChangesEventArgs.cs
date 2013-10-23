// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    ///     This class provides information to PreApply and PostApply methods so that RefactoringContributors and RefactorOperations can perform actions.
    ///     Input will include the change proposals selected for application.
    /// </summary>
    internal sealed class ApplyChangesEventArgs : CancelEventArgs
    {
        private readonly IList<FileChange> _changes;

        /// <summary>
        ///     Contructor of this event, which takes the a list of FileChange.
        /// </summary>
        /// <param name="changes"></param>
        public ApplyChangesEventArgs(IList<FileChange> changes)
        {
            _changes = changes;
        }

        /// <summary>
        ///     List of FileChange will be applied.
        /// </summary>
        public IList<FileChange> Changes
        {
            get { return _changes; }
        }
    }
}
