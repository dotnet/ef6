// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;

    /// <summary>
    ///     The ChangeProposal class represents the changes returned from the contributors.
    ///     These changes represent the file, offset, lengths, new value, and old value of a proposed change.
    /// </summary>
    internal class TextChangeProposal : ChangeProposal
    {
        /// <summary>
        ///     Constructor of ChangeProposal.
        /// </summary>
        /// <param name="projectName">The project name with full path that this file belongs to.</param>
        /// <param name="fileName">The file name with full path which file this change is from.</param>
        /// <param name="newValue">New value of this change.</param>
        public TextChangeProposal(String projectName, String fileName, String newValue)
            : base(projectName, fileName, true)
        {
            NewValue = newValue;
        }

        /// <summary>
        ///     New value will change to.
        /// </summary>
        public String NewValue { get; set; }

        /// <summary>
        ///     Start offset of this change.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        ///     Length of this change.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        ///     Start line of this change.
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        ///     End line of this change.
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        ///     Start column of this change
        /// </summary>
        public int StartColumn { get; set; }

        /// <summary>
        ///     End column of this change
        /// </summary>
        public int EndColumn { get; set; }

        /// <summary>
        ///     Returns if this TextChangeProposal object has the same value as another object,
        ///     which also must be a TextChangeProposal
        /// </summary>
        public override bool Equals(object obj)
        {
            var isEqual = false;
            var other = obj as TextChangeProposal;
            if (other != null)
            {
                isEqual = base.Equals(obj);
                if (isEqual)
                {
                    isEqual = (StartLine == other.StartLine &&
                               StartColumn == other.StartColumn &&
                               string.Equals(FileName, other.FileName, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(ProjectName, other.ProjectName, StringComparison.OrdinalIgnoreCase));
                }
            }
            return isEqual;
        }

        /// <summary>
        ///     Returns hash code for this object
        /// </summary>
        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();
            hashCode = hashCode ^ StartLine;
            hashCode = hashCode ^ StartColumn;
            hashCode = hashCode ^ (FileName ?? string.Empty).GetHashCode();
            hashCode = hashCode ^ (ProjectName ?? string.Empty).GetHashCode();
            return hashCode;
        }
    }
}
