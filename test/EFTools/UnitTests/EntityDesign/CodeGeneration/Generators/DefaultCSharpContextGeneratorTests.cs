// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class DefaultCSharpContextGeneratorTests : GeneratorTestBase
    {
        [Fact]
        public void Generate_returns_code()
        {
            var generator = new DefaultCSharpContextGenerator();
            var result = generator.Generate(Model.ConceptualModel.Container, Model, "WebApplication1.Models");

            Assert.Equal(
                @"namespace WebApplication1.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CodeFirstContainer : DbContext
    {
        public CodeFirstContainer()
            : base(""name=CodeFirstContainer"")
        {
        }

        public virtual DbSet<Entity> Entities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
",
                result);
        }
    }
}
