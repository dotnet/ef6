// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmNavigationPropertyExtensions
    {
        public static object GetConfiguration(this EdmNavigationProperty navigationProperty)
        {
            Contract.Requires(navigationProperty != null);

            return navigationProperty.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EdmNavigationProperty navigationProperty, object configuration)
        {
            Contract.Requires(navigationProperty != null);

            navigationProperty.Annotations.SetConfiguration(configuration);
        }
    }
}
