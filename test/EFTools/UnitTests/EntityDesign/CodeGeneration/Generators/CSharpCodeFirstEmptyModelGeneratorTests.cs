// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using System.Globalization;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Xunit;

    public class CSharpCodeFirstEmptyModelGeneratorTests
    {
        [Fact]
        public void CSharpCodeFirstEmptyModelGenerator_generates_code()
        {
            var generatedCode = new CSharpCodeFirstEmptyModelGenerator()
                .Generate(null, "ConsoleApplication.Data", "MyContext", "MyContextConnString");

            var ctorComment =
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_CS,
                    "MyContext",
                    "ConsoleApplication.Data");

            Assert.Equal(@"namespace ConsoleApplication.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class MyContext : DbContext
    {
        " + ctorComment + @"
        public MyContext()
            : base(""name=MyContextConnString"")
        {
        }

        " + Resources.CodeFirstCodeFile_DbSetComment_CS + @"

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}", generatedCode);
        }
    }
}
