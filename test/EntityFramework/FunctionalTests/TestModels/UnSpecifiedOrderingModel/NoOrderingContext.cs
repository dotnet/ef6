// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace UnSpecifiedOrderingModel
{
    using System.Data.Entity;

    public class NoOrderingContext : DbContext
    {
        public DbSet<CompositeKeyEntityWithNoOrdering> CompositeKeyEntities { get; set; }
    }
}
