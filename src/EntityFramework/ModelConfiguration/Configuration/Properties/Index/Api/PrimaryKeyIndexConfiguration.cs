// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Configures a primary key index.
    /// </summary>
    public class PrimaryKeyIndexConfiguration : IndexConfigurationBase<PrimaryKeyIndexConfiguration>
    {
        internal PrimaryKeyIndexConfiguration(Properties.Index.IndexConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
