// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class DefaultVBContextGeneratorTests : GeneratorTestBase
    {
        [Fact]
        public void Generate_returns_code()
        {
            var generator = new DefaultVBContextGenerator();
            var result = generator.Generate(Model, "WebApplication1.Models", "MyContext", "MyContextConnString");

            Assert.Equal(
                @"Imports System
Imports System.Data.Entity
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Linq

Partial Public Class MyContext
    Inherits DbContext

    Public Sub New()
        MyBase.New(""name=MyContextConnString"")        
    End Sub

    Public Overridable Property Entities As DbSet(Of Entity)

    Protected Overrides Sub OnModelCreating(ByVal modelBuilder As DbModelBuilder)
    End Sub
End Class
",
                result);
        }
    }
}
