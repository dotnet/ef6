// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class EdmModelVisitorTests
    {
        [Fact]
        public void VisitEdmModel_should_visit_edm_functions()
        {
            var visitorMock
                = new Mock<EdmModelVisitor>
                      {
                          CallBase = true
                      };

            var function = new EdmFunction();
            var model = new EdmModel(DataSpace.SSpace);
            model.AddItem(function);

            visitorMock.Object.VisitEdmModel(model);

            visitorMock.Verify(v => v.VisitFunctions(It.IsAny<IEnumerable<EdmFunction>>()), Times.Once());
            visitorMock.Verify(v => v.VisitMetadataItem(function), Times.Once());
            visitorMock.Verify(v => v.VisitEdmFunction(function), Times.Once());
        }

        [Fact]
        public void VisitEdmFunction_should_visit_edm_function_parameters()
        {
            var visitorMock
                = new Mock<EdmModelVisitor>
                      {
                          CallBase = true
                      };

            var functionParameter
                = new FunctionParameter(
                    "P",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            var functionPayload
                = new EdmFunctionPayload
                      {
                          Parameters = new[] { functionParameter }
                      };

            var function = new EdmFunction("F", "N", DataSpace.SSpace, functionPayload);

            visitorMock.Object.VisitEdmFunction(function);

            visitorMock.Verify(v => v.VisitMetadataItem(functionParameter), Times.Once());
            visitorMock.Verify(v => v.VisitFunctionParameter(functionParameter), Times.Once());
        }
    }
}
