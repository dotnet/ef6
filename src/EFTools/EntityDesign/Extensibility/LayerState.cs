// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    internal class LayerState
    {
        internal bool IsEnabled { get; set; }
        internal EntityDesignerCommand EnableCommand { get; set; }
    }
}
