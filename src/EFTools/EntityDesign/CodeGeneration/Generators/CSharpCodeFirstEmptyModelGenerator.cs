// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using System.Data.Entity.Infrastructure;
    using System.Globalization;

    internal class CSharpCodeFirstEmptyModelGenerator : IContextGenerator
    {
        private const string CSharpCodeFileTemplate = 
@"namespace {0}
{{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class {1} : DbContext
    {{
        {2}
        public {1}()
            : base(""name={3}"")
        {{
        }}

        {4}

        // public virtual DbSet<MyEntity> MyEntities {{ get; set; }}
    }}

    //public class MyEntity
    //{{
    //    public int Id {{ get; set; }}
    //    public string Name {{ get; set; }}
    //}}
}}";

        public string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            var ctorComment = 
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_CS,
                    contextClassName,
                    codeNamespace);

            return
                string.Format(
                    CultureInfo.CurrentCulture, 
                    CSharpCodeFileTemplate, 
                    codeNamespace, 
                    contextClassName,
                    ctorComment,
                    connectionStringName,
                    Resources.CodeFirstCodeFile_DbSetComment_CS);
        }
    }
}
