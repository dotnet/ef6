// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    internal class CodeElementRenameData
    {
        internal RefactorTargetType RefactorTargetType { get; private set; }
        internal string NewName { get; private set; }
        internal string NewEntitySetName { get; private set; }
        internal string OldName { get; private set; }
        internal string OldEntitySetName { get; private set; }
        internal string ParentEntityTypeName { get; private set; }

        internal CodeElementRenameData(string newName, string newEntitySetName, string oldName, string oldEntitySetName)
        {
            NewName = newName;
            NewEntitySetName = newEntitySetName;
            OldName = oldName;
            OldEntitySetName = oldEntitySetName;
            RefactorTargetType = RefactorTargetType.Class;
        }

        internal CodeElementRenameData(string newName, string oldName, string parentEntityTypeName)
        {
            NewName = newName;
            OldName = oldName;
            ParentEntityTypeName = parentEntityTypeName;
            RefactorTargetType = RefactorTargetType.Property;
        }
    }
}
