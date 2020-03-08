// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using SimpleModel;

    public class CodeFirstScaffoldingContext : DbContext
    {
        public string ExtraInfo { get; private set; }

        public CodeFirstScaffoldingContext(string extraInfo)
        {
            ExtraInfo = extraInfo;
        }

        public DbSet<Product> Products { get; set; }
    }
}
