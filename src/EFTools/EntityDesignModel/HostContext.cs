// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Validation;

    internal sealed class HostContext
    {
        public static readonly HostContext Instance = new HostContext();

        private HostContext()
        {
        }

        // TODO: this feels fishy. EntityDesignModel does not have access to ErrorList
        // and this is the way to avoid taking the additional dependency. Figure out 
        // a better way to log the error.
        public void LogUpdateModelWizardError(ErrorInfo errorInfo, string fileInfoPath)
        {
            if (LogUpdateModelWizardErrorAction != null)
            {
                LogUpdateModelWizardErrorAction(errorInfo, fileInfoPath);
            }
        }

        public Action<ErrorInfo, string> LogUpdateModelWizardErrorAction { get; set; }
    }
}
