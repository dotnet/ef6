// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.VS;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Activities;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class WorkflowActivityTestsRemote
    {
        public WorkflowActivityTestsRemote()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
        }

        private IDictionary<string, object> InvokeWorkflowActivity<A>(Dictionary<string, object> inputs, EdmParameterBag parameterBag)
            where A : Activity, new()
        {
            var invoker = new WorkflowInvoker(new A());

            var symbolResolver = new SymbolResolver();
            symbolResolver.Add(typeof(EdmParameterBag).Name, parameterBag);
            invoker.Extensions.Add(symbolResolver);

            return invoker.Invoke(inputs);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SQLCETest()
        {
            UITestRunner.Execute(
                () =>
                    {
                        var sqlCeInputs = new Dictionary<string, object>
                            {
                                {
                                    "SsdlInput",
                                    TestUtils.LoadEmbeddedResource("EFDesigner.InProcTests.TestData.AssocBetSubtypesV2SQLCE40.ssdl")
                                },
                                {
                                    "ExistingSsdlInput",
                                    TestUtils.LoadEmbeddedResource(
                                        "EFDesigner.InProcTests.TestData.AssocBetSubtypesV2SQLCE40_Existing.ssdl")
                                },
                                {
                                    "TemplatePath",
                                    @"$(VSEFTools)\DBGen\SSDLToSQL10.tt"
                                }
                            };

                        var sqlCeParameterBag = new EdmParameterBag(
                            null,
                            null,
                            EntityFrameworkVersion.Version2,
                            "System.Data.SqlServerCe.4.0",
                            "4.0",
                            null,
                            "dbo",
                            "TestDb",
                            @"$(VSEFTools)\DBGen\SSDLToSQL10.tt",
                            @"D:\Foo\Blah\t3est.edmx");

                        var outputs = InvokeWorkflowActivity<SsdlToDdlActivity>(sqlCeInputs, sqlCeParameterBag);
                        object ddlObj;
                        if (outputs.TryGetValue("DdlOutput", out ddlObj))
                        {
                            Assert.AreEqual(
                                TestUtils.LoadEmbeddedResource("EFDesigner.InProcTests.Baselines.WorkflowActivityTests_SQLCETest.bsl"),
                                ScrubSQLCEDDL((string)ddlObj));
                        }
                        else
                        {
                            Assert.Fail("Could not find DDL output from SSDL to DDL activity for SQL CE");
                        }
                    });
        }

        private static string ScrubSQLCEDDL(string ddl)
        {
            // need to remove the metadata at the beginning. QUOTED_IDENTIFIER is not present here.
            var startOfScript = ddl.IndexOf("-- Dropping existing FOREIGN KEY constraints");
            if (startOfScript != -1)
            {
                return ddl.Substring(startOfScript);
            }
            return ddl;
        }
    }
}
