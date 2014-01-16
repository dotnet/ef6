// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.Data.Entity.Migrations.Utilities;
    using System.Linq;
    using Xunit;

    public class AnnotationCodeGeneratorTests
    {
        [Fact]
        public void GetExtraNamespaces_returns_empty_list()
        {
            Assert.Empty(new TestGenerator().GetExtraNamespaces(Enumerable.Empty<string>()));
        }

        [Fact]
        public void GetExtraNamespaces_checks_arguments()
        {
            Assert.Equal(
                "annotationNames",
                Assert.Throws<ArgumentNullException>(() => new TestGenerator().GetExtraNamespaces(null)).ParamName);
        }

        public class TestGenerator : AnnotationCodeGenerator
        {
            public override void Generate(string annotationName, object annotation, IndentedTextWriter writer)
            {
            }
        }
    }
}
