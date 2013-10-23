// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class PropertyDescriptorEqualityComparer : IEqualityComparer<PropertyDescriptor>
    {
        #region IEqualityComparer<PropertyDescriptor> Members

        public bool Equals(PropertyDescriptor x, PropertyDescriptor y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x == null && y != null)
                ||
                (x != null && y == null))
            {
                return false;
            }

            Debug.Assert(
                x.ComponentType != null && y.ComponentType != null,
                "Either the PropertyDescriptor {" + x.Name + "} or {" + y.Name + "} has a null ComponentType");
            if (x.ComponentType != null
                && y.ComponentType != null)
            {
                return x.Equals(y) && x.ComponentType.FullName.Equals(y.ComponentType.FullName, StringComparison.Ordinal);
            }

            return false;
        }

        public int GetHashCode(PropertyDescriptor obj)
        {
            var hashCode = obj.GetHashCode();
            Debug.Assert(
                obj.ComponentType != null && obj.ComponentType.FullName != null,
                "ComponentType or ComponentType.FullName of PropertyDescriptor {" + obj.Name + "} is null");
            if (obj.ComponentType != null
                && obj.ComponentType.FullName != null)
            {
                hashCode ^= obj.ComponentType.FullName.GetHashCode();
            }
            return hashCode;
        }

        #endregion
    }
}
