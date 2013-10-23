// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;

    internal abstract class ModelChangeEventArgs : EventArgs
    {
        public abstract IEnumerable<ModelNodeChangeInfo> Changes { get; }
    }
}
