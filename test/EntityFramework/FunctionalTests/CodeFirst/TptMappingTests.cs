// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;    using System.ComponentModel.DataAnnotations;    using System.ComponentModel.DataAnnotations.Schema;    using System.Data.Entity.Infrastructure;    using System.Data.Entity.Core.Metadata.Edm;    using System.Data.Entity.Migrations.Edm;    using System.IO;    using System.Linq;    using System.Xml;    using System.Xml.Linq;    using Xunit;

    public class TptMappingTests : FunctionalTestBase
    {
        [Table("A")]
        public abstract class A
        {
            public Guid ID { get; set; }
        }

        [Table("AA")]
        public class AA : A
        {
            public BA BAChild { get; set; }
        }

        [Table("AB")]
        public class AB : A
        {
            public BB BBChild { get; set; }
        }

        [Table("B")]
        public abstract class B
        {
            public Guid ID { get; set; }
        }

        [Table("BA")]
        public class BA : B
        {
            public AA Parent { get; set; }
        }

        [Table("BB")]
        public class BB : B
        {
            public AB Parent { get; set; }
        }

        public class ABContext : DbContext        {            static ABContext()            {                Database.SetInitializer<ABContext>(null);            }
            protected override void OnModelCreating(DbModelBuilder modelBuilder)            {                modelBuilder.Entity<AA>().HasRequired(o => o.BAChild).WithRequiredPrincipal(o => o.Parent);                modelBuilder.Entity<AB>().HasRequired(o => o.BBChild).WithRequiredPrincipal(o => o.Parent);            }            public DbSet<A> As { get; set; }            public DbSet<B> Bs { get; set; }        }        [Fact]                public static void CodePlex362_ssdl_associations_are_created_correctly()        {            XDocument model;            using (var context = new ABContext())            using (var stream = new MemoryStream())            using (var writer = XmlWriter.Create(stream))            {                EdmxWriter.WriteEdmx(context, writer);                stream.Position = 0;                model = XDocument.Load(stream);            }            var ns = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");            var expected = new List<string>()                 { "A-AA", "A-AB", "B-BA", "B-BB", "AA-BA", "AB-BB" };            var actual =                from a in model.Descendants(ns + "Association")                from p in a.Descendants(ns + "Principal")                from d in a.Descendants(ns + "Dependent")                                select p.Attribute("Role").Value + "-" + d.Attribute("Role").Value;            Assert.Equal(                expected.OrderByDescending(s => s).ToArray(),                actual.OrderByDescending(s => s).ToArray());        }
    }
}
