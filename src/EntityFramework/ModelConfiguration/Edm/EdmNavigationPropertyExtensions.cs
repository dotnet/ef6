// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;

    internal static class EdmNavigationPropertyExtensions
    {
        public static object GetConfiguration(this NavigationProperty navigationProperty)
        {
            DebugCheck.NotNull(navigationProperty);

            return navigationProperty.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this NavigationProperty navigationProperty, object configuration)
        {
            DebugCheck.NotNull(navigationProperty);

            navigationProperty.Annotations.SetConfiguration(configuration);
        }
    }
}
