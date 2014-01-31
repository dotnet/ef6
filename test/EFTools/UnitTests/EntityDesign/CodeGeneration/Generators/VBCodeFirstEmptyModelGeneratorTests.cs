// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using System.Globalization;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Xunit;

    public class VBCodeFirstEmptyModelGeneratorTests
    {
        [Fact]
        public void VBCodeFirstEmptyModelGeneratorTests_generates_code()
        {
            var generatedCode = new 
            VBCodeFirstEmptyModelGenerator()

                .Generate(null, "ConsoleApplication.Data", "MyContext", "MyContextConnString");

            var ctorComment =
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_VB,
                    "MyContext",
                    "ConsoleApplication.Data");

            Assert.Equal(@"Imports System
Imports System.Data.Entity
Imports System.Linq

Public Class MyContext
    Inherits DbContext

    " + ctorComment + @"
    Public Sub New()
        MyBase.New(""name=MyContextConnString"")
    End Sub

    " + Resources.CodeFirstCodeFile_DbSetComment_VB + @"
    ' Public Overridable Property MyEntities() As DbSet(Of MyEntity)

End Class

'Public Class MyEntity
'    Public Property Id() As Int32
'    Public Property Name() As String
'End Class
", generatedCode);
        }
    }
}
