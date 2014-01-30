// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using System.Data.Entity.Infrastructure;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using System.Globalization;

    internal class VBCodeFirstEmptyModelGenerator : IContextGenerator
    {
        private const string VBCodeFileTemplate =
@"Imports System
Imports System.Data.Entity
Imports System.Linq

Public Class {0}
    Inherits DbContext

    {1}
    Public Sub New()
        MyBase.New(""name={2}"")

        {3}

       ' Public Overridable Property MyEntities() As DbSet(Of MyEntity)

    End Sub

End Class

'Public Class MyEntity
'    Public Property Id() As Int
'    Public Property Name() As String
'End Class
";
        public string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            var ctorComment = 
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_VB,
                    contextClassName,
                    codeNamespace);

            return
                string.Format(
                    CultureInfo.CurrentCulture, 
                    VBCodeFileTemplate,  
                    contextClassName,
                    ctorComment,
                    connectionStringName,
                    Resources.CodeFirstCodeFile_DbSetComment_VB);
        }
    }
}
