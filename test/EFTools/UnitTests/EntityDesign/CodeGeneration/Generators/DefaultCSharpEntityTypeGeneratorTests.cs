// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Linq;
    using Xunit;

    public class DefaultCSharpEntityTypeGeneratorTests : GeneratorTestBase
    {
        [Fact]
        public void Generate_returns_code()
        {
            var generator = new DefaultCSharpEntityTypeGenerator();
            var result = generator.Generate(
                Model.ConceptualModel.Container.EntitySets.First(),
                Model,
                "WebApplication1.Models");

            Assert.Equal(
                @"namespace WebApplication1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Entity
    {
        public int Id { get; set; }
    }
}
",
                result);
        }
    }
}
