// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class ErrorListHelperWrapper : IErrorListHelper
    {
        public void AddErrorInfosToErrorList(ICollection<ErrorInfo> errors, IVsHierarchy vsHierarchy, uint itemID, bool bringErrorListToFront = false)
        {
            ErrorListHelper.AddErrorInfosToErrorList(errors, vsHierarchy, itemID, bringErrorListToFront);
        }
    }
}
