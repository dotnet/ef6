// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Generators
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using System.Collections.Generic;
    using System.Globalization;

    internal class CSharpCodeFirstEmptyModelGenarator
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
            : base(""name={1}"")
        {{
        }}

		{3}

        // public DbSet<MyEntity> MyEntities {{ get; set; }}
    }}

    //public class MyEntity
    //{{
    //    public int Id {{ get; set; }}
    //    public string Name {{ get; set; }}
    //}}
}}";

        public IEnumerable<KeyValuePair<string, string>> Generate(string codeNamespace, string contextClassName)
        {
            var ctorComment = 
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.CodeFirstCodeFile_CtorComment_CS,
                    contextClassName,
                    codeNamespace);

            yield return new KeyValuePair<string, string>(
                contextClassName,
                string.Format(
                    CultureInfo.CurrentCulture, 
                    CSharpCodeFileTemplate, 
                    codeNamespace, 
                    contextClassName,
                    ctorComment,
                    Resources.CodeFirstCodeFile_DbSetComment_CS));
        }
    }
}
