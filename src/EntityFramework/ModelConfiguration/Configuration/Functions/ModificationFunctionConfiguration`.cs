// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    public abstract class ModificationFunctionConfiguration<TEntityType>
        where TEntityType : class
    {
        private readonly ModificationFunctionConfiguration _configuration
            = new ModificationFunctionConfiguration();

        internal ModificationFunctionConfiguration()
        {
        }

        internal ModificationFunctionConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
