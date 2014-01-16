// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Utilities;

    public class CollationCSharpCodeGenerator : AnnotationCodeGenerator
    {
        public override IEnumerable<string> GetExtraNamespaces(IEnumerable<string> annotationNames)
        {
            return new[] { typeof(CollationAttribute).Namespace };
        }

        public override void Generate(string annotationName, object annotation, IndentedTextWriter writer)
        {
            writer.Write("new CollationAttribute(\"" + ((CollationAttribute)annotation).CollationName + "\")");
        }
    }
}
