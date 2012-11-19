// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;

    public interface IConfigurationConvention : IConvention
    {
        void Apply(ModelConfiguration modelConfiguration);
    }
}
