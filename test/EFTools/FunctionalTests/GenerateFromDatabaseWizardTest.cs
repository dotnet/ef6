// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.FunctionalTests
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using EFDesignerTestInfrastructure;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard;
    using Xunit;
    using global::FunctionalTests.TestHelpers;

    public class GenerateFromDatabaseWizardTest
    {
        [Fact]
        public void Complex_type_removed_from_model_when_TVF_removed()
        {
            using (var artifactHelper = new MockEFArtifactHelper())
            {
                var artifact =
                    artifactHelper.GetNewOrExistingArtifact(
                        TestUtils.FileName2Uri(@"TestArtifacts\FunctionImportsAndTvfs.edmx"));
                var removeFunctionImportsMethod =
                    typeof(ModelObjectItemWizard).GetMethod(
                        "CreateRemoveFunctionImportCommands", BindingFlags.Static | BindingFlags.NonPublic);
                var commands = ((IEnumerable)removeFunctionImportsMethod.Invoke(null, new object[] { artifact })).Cast<object>().ToArray();

                Assert.Equal(5, commands.Count());

                var functionImport = GetPropertyValue<FunctionImport>(commands[0], "FunctionImport");
                Assert.Equal("DeleteFunctionImportCommand", commands[0].GetType().Name);
                Assert.Equal("SingleColumnTableValuedFunction", functionImport.Name.Value);
                Assert.Equal("DeleteComplexTypeCommand", commands[1].GetType().Name);
                Assert.Same(functionImport.ReturnTypeAsComplexType.Target, GetPropertyValue<ComplexType>(commands[1], "EFElement"));

                functionImport = GetPropertyValue<FunctionImport>(commands[2], "FunctionImport");
                Assert.Equal("DeleteFunctionImportCommand", commands[2].GetType().Name);
                Assert.Equal("StoredProc", functionImport.Name.Value);

                functionImport = GetPropertyValue<FunctionImport>(commands[3], "FunctionImport");
                Assert.Equal("DeleteFunctionImportCommand", commands[3].GetType().Name);
                Assert.Equal("MultipleColumnTableValuedFunction", functionImport.Name.Value);
                Assert.Equal("DeleteComplexTypeCommand", commands[4].GetType().Name);
                Assert.Same(functionImport.ReturnTypeAsComplexType.Target, GetPropertyValue<ComplexType>(commands[4], "EFElement"));
            }
        }

        private static T GetPropertyValue<T>(object target, string propertyName)
        {
            var propInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propInfo == null)
            {
                throw new InvalidOperationException(propertyName + "property not found.");
            }

            return (T)propInfo.GetValue(target);
        }
    }
}
