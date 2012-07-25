// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Data.Common;

namespace SampleEntityFrameworkProvider
{
    public partial class SampleFactory : IServiceProvider
    {
        //Implement IServiceProvider
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(DbProviderServices))
                return SampleProviderServices.Instance;
            else
                return null;
        }
    }
}
