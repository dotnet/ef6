// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Resources;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class LocDisplayNameAttribute : CommonLocDisplayNameAttribute
    {
        public LocDisplayNameAttribute(string name)
            : base(name)
        {
        }

        protected override ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocDescriptionAttribute : CommonLocDescriptionAttribute
    {
        public LocDescriptionAttribute(string description)
            : base(description)
        {
        }

        protected override ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class LocCategoryAttribute : CommonLocCategoryAttribute
    {
        public LocCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override ResourceManager ResourceManager
        {
            get { return Resources.ResourceManager; }
        }
    }
}
