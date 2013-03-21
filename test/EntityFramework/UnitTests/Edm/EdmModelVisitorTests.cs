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

            var function = new EdmFunction("F", "N", DataSpace.SSpace);
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

        [Fact]
        public void VisitEdmEntityContainer_visits_function_imports()
        {
            var functionPayload =
                new EdmFunctionPayload
                    {
                        IsFunctionImport = true
                    };

            var functionImport =
                new EdmFunction("f", "N", DataSpace.CSpace, functionPayload);

            var container = new EntityContainer("C", DataSpace.CSpace);
            container.AddFunctionImport(functionImport);

            var visitorMock =
                new Mock<EdmModelVisitor>
                    {
                        CallBase = true
                    };

            visitorMock.Object.VisitEdmModel(new EdmModel(container));

            visitorMock.Verify(v => v.VisitFunctionImports(container, It.IsAny<IEnumerable<EdmFunction>>()), Times.Once());
            visitorMock.Verify(v => v.VisitFunctionImport(functionImport), Times.Once());
        }

        [Fact]
        public void VisitFunctionImport_visits_function_import_input_and_returnparameters()
        {
            var typeUsage = 
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var inputParam = new FunctionParameter("p1", typeUsage, ParameterMode.In);
            var returnParam = new FunctionParameter("r", typeUsage, ParameterMode.ReturnValue);

            var functionPayload =
                new EdmFunctionPayload
                {
                    IsFunctionImport = true,
                    Parameters = new [] { inputParam },
                    ReturnParameters = new[] {returnParam}
                };

            var functionImport = new EdmFunction("f", "N", DataSpace.CSpace, functionPayload);

            var visitorMock =
                new Mock<EdmModelVisitor>
                    {
                        CallBase = true
                    };

            visitorMock.Object.VisitFunctionImport(functionImport);

            visitorMock.Verify(v => v.VisitFunctionImportParameter(inputParam), Times.Once());
            visitorMock.Verify(v => v.VisitFunctionImportReturnParameter(returnParam), Times.Once());
        }
    }
}
