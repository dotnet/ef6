// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Data.Entity;

    public class LocalDbLoginsContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
    }
}
