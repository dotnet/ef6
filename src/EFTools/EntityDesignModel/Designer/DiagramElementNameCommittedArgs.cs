// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    internal class DiagramElementNameCommittedArgs : EventArgs
    {
        internal DiagramElementNameCommittedArgs(string shapeName)
        {
            ShapeName = shapeName;
            PropertyName = null;
        }

        internal DiagramElementNameCommittedArgs(string shapeName, string propertyName)
        {
            ShapeName = shapeName;
            PropertyName = propertyName;
        }

        internal string ShapeName { get; private set; }
        internal string PropertyName { get; private set; }
    }
}
