// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     Encapsulates information about a child branch contained within a header branch
    /// </summary>
    internal class ChildBranchInfo
    {
        private readonly IBranch _branch;
        private readonly string _name;
        private readonly object _id;

        /// <summary>
        ///     event handler to listen to child branch modifications necessary to support header
        ///     checkbox columns.  stored here so that it can be removed from the branch at the
        ///     appropriate time
        /// </summary>
        private BranchModificationEventHandler _childModificationHandler;

        /// <summary>
        ///     Create a new child branch.
        /// </summary>
        internal ChildBranchInfo(IBranch branch, string name, object id)
        {
            _branch = branch;
            _name = name;
            _id = id;
        }

        /// <summary>
        ///     Gets the child branch
        /// </summary>
        internal IBranch Branch
        {
            get { return _branch; }
        }

        /// <summary>
        ///     Gets the header name
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Returns an object identifying this header
        /// </summary>
        internal object Id
        {
            get { return _id; }
        }

        internal BranchModificationEventHandler ChildModificationHandler
        {
            get { return _childModificationHandler; }
            set
            {
                if (_childModificationHandler != null)
                {
                    _branch.OnBranchModification -= _childModificationHandler;
                }

                _childModificationHandler = value;

                if (_childModificationHandler != null)
                {
                    _branch.OnBranchModification += _childModificationHandler;
                }
            }
        }
    }
}
