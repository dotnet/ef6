// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SampleEntityFrameworkProvider
{
    public partial class SampleConnection
    {
        protected override DbProviderFactory DbProviderFactory
        {
            get
            {
                return SampleFactory.Instance;
            }
        }

        internal DbProviderFactory ProviderFactory
        {
            get 
            { 
                return DbProviderFactory;
            }
        }
    }
}
