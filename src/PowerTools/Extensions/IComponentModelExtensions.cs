// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System;
    using System.Diagnostics.Contracts;
    using Microsoft.VisualStudio.ComponentModelHost;

    internal static class IComponentModelExtensions
    {
        public static object GetService(this IComponentModel componentModel, Type serviceType)
        {
            Contract.Requires(componentModel != null);
            Contract.Requires(serviceType != null);

            return typeof(IComponentModel).GetMethod("GetService")
                .MakeGenericMethod(serviceType)
                .Invoke(componentModel, null);
        }
    }
}
