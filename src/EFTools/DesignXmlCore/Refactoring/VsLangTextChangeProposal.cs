// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using Microsoft.Data.Entity.Design.Common;

    /// <summary>
    ///     VsLang changes require a root node that contains the full name of the object definition being refactored.
    /// </summary>
    internal class VsLangTextChangeProposal : TextChangeProposal
    {
        internal VsLangTextChangeProposal(
            string projectName, string fileName, string newValue, string objectDefinitionFullName, bool isRootChange = false)
            : base(projectName, fileName, newValue)
        {
            ArgumentValidation.CheckForNullReference(objectDefinitionFullName, "objectDefinitionFullName");
            ObjectDefinitionFullName = objectDefinitionFullName;
            IsRootChange = isRootChange;
        }

        /// <summary>
        ///     Denotes whether this change proposal should be shown as a root node in the preview tree. Typically only the change proposal
        ///     that targets the object definition will be set as a root node.
        /// </summary>
        internal bool IsRootChange { get; private set; }

        /// <summary>
        ///     The full name of the object definition being refactored.
        /// </summary>
        internal string ObjectDefinitionFullName { get; private set; }

        public override bool Equals(object obj)
        {
            var isEqual = false;
            var other = obj as VsLangTextChangeProposal;
            if (other != null)
            {
                isEqual = base.Equals(obj);
                if (isEqual)
                {
                    isEqual = (IsRootChange == other.IsRootChange
                               && string.CompareOrdinal(ObjectDefinitionFullName, other.ObjectDefinitionFullName) == 0);
                }
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ IsRootChange.GetHashCode() ^ ObjectDefinitionFullName.GetHashCode();
        }
    }
}
