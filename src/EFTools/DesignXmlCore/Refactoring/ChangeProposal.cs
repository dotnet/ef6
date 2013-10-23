// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;

    /// <summary>
    ///     Abstract base class of all kinds of change proposals
    /// </summary>
    internal abstract class ChangeProposal
    {
        internal ChangeProposal()
        {
        }

        internal ChangeProposal(String projectName, String filename, Boolean included)
        {
            ProjectName = projectName;
            FileName = filename;
            Included = included;
        }

        /// <summary>
        ///     Which project the change proposal came from.
        /// </summary>
        public String ProjectName { get; set; }

        /// <summary>
        ///     File name with full path.
        /// </summary>
        public String FileName { get; set; }

        /// <summary>
        ///     Will this ChangeProposal be included in apply changes or not.
        /// </summary>
        public Boolean Included { get; set; }

        /// <summary>
        ///     Determines whether this instance of ChangeProposal and a specified object, which must also be a
        ///     ChangeProposal object, have the same value
        /// </summary>
        public override bool Equals(object obj)
        {
            var isEqual = false;
            var other = obj as ChangeProposal;
            if (other != null)
            {
                isEqual = (FileName == other.FileName);
            }
            return isEqual;
        }

        /// <summary>
        ///     Returns hash code for this object
        /// </summary>
        public override int GetHashCode()
        {
            var hashcode = 0;
            if (FileName != null)
            {
                hashcode = hashcode ^ FileName.GetHashCode();
            }
            return hashcode;
        }
    }
}
