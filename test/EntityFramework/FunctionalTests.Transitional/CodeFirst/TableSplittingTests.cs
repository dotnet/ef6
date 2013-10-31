// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class TableSplittingTests : FunctionalTestBase
    {
        [Table("T1")]
        public class E1
        {
            [Key]
            public virtual int Id { get; set; }

            public virtual string P1 { get; set; }

            [Required]
            public virtual E2 E2 { get; set; }

            [ForeignKey("E3")]
            [Column("E1_E3Id")]
            public virtual int? E3Id { get; set; }

            public virtual E3 E3 { get; set; }
        }

        [Table("T1")]
        public class E2
        {
            [Key]
            public virtual int Id { get; set; }

            public virtual string P2 { get; set; }

            [Required]
            public virtual E1 E1 { get; set; }

            [ForeignKey("E3")]
            [Column("E2_E3Id")]
            public virtual int? E3Id { get; set; }

            public virtual E3 E3 { get; set; }
        }

        [Table("T2")]
        public class E3
        {
            [Key]
            public virtual int Id { get; set; }

            [InverseProperty("E3")]
            public virtual ICollection<E1> E1s { get; set; }

            [InverseProperty("E3")]
            public virtual ICollection<E2> E2s { get; set; }
        }

        [Fact]
        public void CodePlex643_entities_are_configured_correctly()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<E2>().HasRequired(e => e.E1).WithRequiredPrincipal(e => e.E2);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(3, databaseMapping.Model.EntityTypes.Count());
            Assert.Equal(3, databaseMapping.Model.AssociationTypes.Count());
            Assert.Equal(2, databaseMapping.Database.EntityTypes.Count());
            Assert.Equal(2, databaseMapping.Database.AssociationTypes.Count());

            databaseMapping.Assert<E1>("T1")
                .HasColumn("Id")
                .HasColumn("P1")
                .HasColumn("P2")
                .HasForeignKeyColumn("E1_E3Id", "T2")
                .HasForeignKeyColumn("E2_E3Id", "T2")
                .ColumnCountEquals(5);

            databaseMapping.Assert<E2>("T1")
                .HasColumn("Id")
                .HasColumn("P1")
                .HasColumn("P2")
                .HasForeignKeyColumn("E1_E3Id", "T2")
                .HasForeignKeyColumn("E2_E3Id", "T2")
                .ColumnCountEquals(5);

            databaseMapping.Assert<E3>("T2")
                .HasColumn("Id");
        }

        [Table("A")]
        public class A
        {
            public virtual int Id { get; set; }
        }

        [Table("B")]
        public class B : A
        {
            [ForeignKey("Z")]
            public virtual int ZId { get; set; }

            public virtual Z Z { get; set; }
        }

        [Table("B")]
        public class B1 : B
        {
        }

        [Table("C")]
        public class C : A
        {
            [ForeignKey("Z")]
            public virtual int ZId { get; set; }

            public virtual Z Z { get; set; }
        }

        [Table("C")]
        public class C1 : C
        {
        }

        public class Z
        {
            public virtual int Id { get; set; }

            [InverseProperty("Z")]
            public virtual ICollection<B> Bs { get; set; }

            [InverseProperty("Z")]
            public virtual ICollection<C> Cs { get; set; }
        }

        [Fact]
        public void CodePlex677_entities_are_configured_correctly()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<A>();
            modelBuilder.Entity<B>();
            modelBuilder.Entity<B1>();
            modelBuilder.Entity<C>();
            modelBuilder.Entity<C1>();
            modelBuilder.Entity<Z>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<A>("A")
                .HasColumn("Id");

            databaseMapping.Assert<B>("B")
                .HasColumn("Id")
                .HasColumn("ZId")
                .HasColumn("Discriminator")
                .HasForeignKeyColumn("ZId", "Z")
                .HasForeignKeyColumn("Id", "A");

            databaseMapping.Assert<B1>("B")
                .HasColumn("Id")
                .HasColumn("ZId")
                .HasColumn("Discriminator")
                .HasForeignKeyColumn("ZId", "Z")
                .HasForeignKeyColumn("Id", "A");

            databaseMapping.Assert<C>("C")
                .HasColumn("Id")
                .HasColumn("ZId")
                .HasColumn("Discriminator")
                .HasForeignKeyColumn("ZId", "Z")
                .HasForeignKeyColumn("Id", "A");

            databaseMapping.Assert<C1>("C")
                .HasColumn("Id")
                .HasColumn("ZId")
                .HasColumn("Discriminator")
                .HasForeignKeyColumn("ZId", "Z")
                .HasForeignKeyColumn("Id", "A");

            databaseMapping.Assert<Z>("Z")
                .HasColumn("Id");
        }
    }
}
