// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using System.Collections.Generic;
    using System.Globalization;

    internal class VBCodeFirstEmptyModelGenerator
    {
        private const string VBCodeFileTemplate =
@"Imports System
Imports System.Data.Entity
Imports System.Linq

Public Class {0}
    Inherits DbContext

	{1}
	Public Sub New()
		MyBase.New(""name={0}"")

		{2}

       ' Public Property MyEntities() As DbSet(Of MyEntity)

    End Sub

End Class

' Public Class MyEntity
'     Public Property Id() As Int
'	Public Property Name() As String
' End Class
";
        public IEnumerable<KeyValuePair<string, string>> Generate(string codeNamespace, string contextClassName)
        {
            var ctorComment = 
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_VB,
                    contextClassName,
                    codeNamespace);

            yield return new KeyValuePair<string, string>(
                contextClassName,
                string.Format(
                    CultureInfo.CurrentCulture, 
                    VBCodeFileTemplate,  
                    contextClassName,
                    ctorComment,
                    Resources.CodeFirstCodeFile_DbSetComment_VB));
        }
    }
}
