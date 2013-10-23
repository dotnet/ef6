// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;

    internal interface IXmlDesignerPackage : IServiceProvider
    {
        void InvokeOnForeground(SimpleDelegateClass.SimpleDelegate simpleDelegate);
        bool IsForegroundThread { get; }

        DocumentFrameMgr DocumentFrameMgr { get; }
        ModelManager ModelManager { get; }
        event ModelChangeEventHandler FileNameChanged;
        string GetResourceString(string resourceName);
    }

    internal class SimpleDelegateClass
    {
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
            Justification = "This name is meaningful in this context")]
        public delegate void SimpleDelegate();
    }
}
