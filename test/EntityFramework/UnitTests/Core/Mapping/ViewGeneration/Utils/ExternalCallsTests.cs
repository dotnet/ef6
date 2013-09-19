// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Query;
    using System.Xml.Linq;
    using Xunit;

    public class ExternalCallsTests : TestBase
    {
        [Fact]
        public void CompileFunctionDefinition_uses_the_given_item_collection()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(ProductModel.CsdlWithFunctions).CreateReader() });

            Assert.Equal(
                "ProductModel.F_NoBody()",
                ((DbFunctionExpression)ExternalCalls.CompileFunctionDefinition(
                    "ProductModel.F_NoBody()",
                    new List<FunctionParameter>(),
                    edmItemCollection).Body).Function.Identity);
        }
    }
}
