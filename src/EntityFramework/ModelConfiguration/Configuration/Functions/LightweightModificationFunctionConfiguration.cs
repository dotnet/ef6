// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    public abstract class LightweightModificationFunctionConfiguration
    {
        private readonly ModificationFunctionConfiguration _configuration
            = new ModificationFunctionConfiguration();

        internal LightweightModificationFunctionConfiguration()
        {
        }

        internal ModificationFunctionConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
