// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring;

    internal class EFRenameContributorInput : ContributorInput
    {
        private readonly EFObject _objectToBeRenamed;
        private readonly string _oldName;
        private readonly string _newName;

        internal EFRenameContributorInput(EFObject objectToBeRenamed, string newName, string oldName)
        {
            ArgumentValidation.CheckForNullReference(objectToBeRenamed, "objectToBeRenamed");
            ArgumentValidation.CheckForNullReference(newName, "newName");
            ArgumentValidation.CheckForNullReference(oldName, "oldName");

            _objectToBeRenamed = objectToBeRenamed;
            _oldName = oldName;
            _newName = newName;
        }

        internal string NewName
        {
            get { return _newName; }
        }

        internal EFObject ObjectToBeRenamed
        {
            get { return _objectToBeRenamed; }
        }

        internal string OldName
        {
            get { return _oldName; }
        }

        public override bool Equals(object obj)
        {
            var isEqual = false;
            var otherInput = obj as EFRenameContributorInput;
            if (otherInput != null)
            {
                isEqual = (_objectToBeRenamed == otherInput._objectToBeRenamed) &&
                          string.CompareOrdinal(_newName, otherInput._newName) == 0 &&
                          string.CompareOrdinal(_oldName, otherInput._oldName) == 0;
            }

            return isEqual;
        }

        public override int GetHashCode()
        {
            return _objectToBeRenamed.GetHashCode() ^ _newName.GetHashCode() ^ _oldName.GetHashCode();
        }
    }
}
