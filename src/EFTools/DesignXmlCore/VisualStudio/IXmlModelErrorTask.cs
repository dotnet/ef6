// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;

    internal interface IXmlModelErrorTask
    {
        IServiceProvider ServiceProvider { get; }
        uint ItemID { get; }
    }
}
