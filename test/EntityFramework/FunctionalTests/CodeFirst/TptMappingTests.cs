// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Linq;    using Xunit;

    public class TptMappingTests : FunctionalTestBase
    {
        public abstract class A
        {
            public Guid ID { get; set; }
        }

        public class AA : A
        {
            public BA BAChild { get; set; }
        }

        public class AB : A
        {
            public BB BBChild { get; set; }
        }

        public abstract class B
        {
            public Guid ID { get; set; }
        }

        public class BA : B
        {
            public AA Parent { get; set; }
        }

        public class BB : B
        {
            public AB Parent { get; set; }
        }

        [Fact]
        public void CodePlex362_SSpace_associations_are_created_correctly()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<A>().ToTable("A");
            modelBuilder.Entity<AA>().ToTable("AA").HasRequired(o => o.BAChild).WithRequiredPrincipal(o => o.Parent);            modelBuilder.Entity<AB>().ToTable("AB").HasRequired(o => o.BBChild).WithRequiredPrincipal(o => o.Parent);
            modelBuilder.Entity<B>().ToTable("B");
            modelBuilder.Entity<BA>().ToTable("BA");
            modelBuilder.Entity<BB>().ToTable("BB");
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(6, databaseMapping.Database.AssociationTypes.Count());
            databaseMapping.Assert<AA>("AA").HasForeignKeyColumn("ID", "A");
            databaseMapping.Assert<AB>("AB").HasForeignKeyColumn("ID", "A");
            databaseMapping.Assert<BA>("BA").HasForeignKeyColumn("ID", "B");
            databaseMapping.Assert<BB>("BB").HasForeignKeyColumn("ID", "B");
            databaseMapping.Assert<BA>("BA").HasForeignKeyColumn("ID", "AA");
            databaseMapping.Assert<BB>("BB").HasForeignKeyColumn("ID", "AB");
        }    }
}
