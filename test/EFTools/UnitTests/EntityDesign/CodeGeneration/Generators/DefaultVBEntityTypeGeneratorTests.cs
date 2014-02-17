// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Linq;
    using Xunit;

    public class DefaultVBEntityTypeGeneratorTests : GeneratorTestBase
    {
        [Fact]
        public void Generate_returns_code()
        {
            var generator = new DefaultVBEntityTypeGenerator();
            var result = generator.Generate(
                Model.ConceptualModel.Container.EntitySets.First(),
                Model,
                "WebApplication1.Models");

            Assert.Equal(
                @"Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial

Partial Public Class Entity
    Public Property Id As Integer
End Class
",
                result);
        }
    }
}
