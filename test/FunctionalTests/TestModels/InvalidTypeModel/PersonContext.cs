// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace InvalidTypeModel
{
    using System.Data.Entity;

    public class PersonContext : DbContext
    {
        public DbSet<Person> People { get; set; }
    }
}
