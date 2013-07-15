// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Data.Entity.Infrastructure;
    using System.Text;
    using System.Xml;
    using Xunit;

    public class NameUniquificationTests : TestBase
    {
        [Fact] 
        public void CodePlex368_column_name_uniquification_is_deterministic()
        {
            string edmx1, edmx2;

            using (var context1 = new Namespace1.MyContext())
            {
                edmx1 = WriteEdmx(context1);
            }

            using (var context2 = new Namespace2.MyContext())
            {
                edmx2 = WriteEdmx(context2);
            }

            Assert.Equal(edmx1, edmx2.Replace("Namespace2", "Namespace1"));
        }

        private static string WriteEdmx(DbContext context)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                EdmxWriter.WriteEdmx(context, writer);
            }

            return builder.ToString();
        }
    }

    // TypeOne appears before TypeTwo
    namespace Namespace1
    {
        class MyContext : DbContext
        {
            public DbSet<TypeBase> Types { get; set; }
        }

        public class TypeBase
        {
            public int Id { get; set; }
        }
       
        public class TypeOne : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class TypeTwo : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class RelatedType
        {
            public int Id { get; set; }
        }
    }

    // TypeTwo appears before TypeOne
    namespace Namespace2
    {
        class MyContext : DbContext
        {
            public DbSet<TypeBase> Types { get; set; }
        }

        public class TypeBase
        {
            public int Id { get; set; }
        }

        public class TypeTwo : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class TypeOne : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class RelatedType
        {
            public int Id { get; set; }
        }
    }
}
