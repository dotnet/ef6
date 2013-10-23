// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests
{
    using System;
    using System.Activities;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.VS;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;

    [TestClass]
    public class ModelFirstTestsRemote
    {
        public ModelFirstTestsRemote()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
        }

        internal void TestPipeline(
            string testName,
            EdmItemCollection csdlInput,
            string workflowFilePath,
            Version versionOfCsdl,
            Func<WorkflowApplicationCompletedEventArgs, object> postInvokeCallback)
        {
            const string existingMsl =
                "<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2008/09/mapping/cs'>" +
                "  <EntityContainerMapping StorageEntityContainer='Model1StoreContainer' CdmEntityContainer='Model1Container' />" +
                "</Mapping>";

            const string existingSsdl =
                "<Schema Namespace='Store' Alias='Self' Provider='System.Data.SqlClient' ProviderManifestToken='2005' xmlns='http://schemas.microsoft.com/ado/2006/04/edm/ssdl' />";

            Exception savedException = null;
            var workflowCompleted = new AutoResetEvent(false);

            var completedHandler = new Action<WorkflowApplicationCompletedEventArgs>(
                e =>
                    {
                        try
                        {
                            var result = (string)postInvokeCallback(e);

                            Assert.AreEqual(
                                TestUtils.LoadEmbeddedResource("EFDesigner.InProcTests.Baselines." + testName + ".bsl"),
                                result);
                        }
                        catch (Exception ex)
                        {
                            // save off the exception, return out of the completion handler. If we throw here
                            // the wrong thread will catch the exception
                            savedException = ex;
                        }
                        finally
                        {
                            workflowCompleted.Set();
                        }
                    }
                );

            var unhandledExceptionHandler = new Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>(
                e =>
                    {
                        if (e.UnhandledException != null)
                        {
                            Console.WriteLine(e.UnhandledException);
                            savedException = e.UnhandledException;
                        }
                        workflowCompleted.Set();
                        return UnhandledExceptionAction.Terminate;
                    }
                );

            var resolvedWorkflowFileInfo =
                DatabaseGenerationEngine.ResolveAndValidateWorkflowPath(null, workflowFilePath);

            var workflowInstance = DatabaseGenerationEngine.CreateDatabaseScriptGenerationWorkflow(
                null,
                null,
                null,
                resolvedWorkflowFileInfo,
                "$(VSEFTools)\\DBGen\\SSDLToSQL10.tt",
                csdlInput,
                existingSsdl,
                existingMsl,
                "dbo",
                "TestDb",
                "System.Data.SqlClient",
                null,
                "2005",
                versionOfCsdl,
                completedHandler,
                unhandledExceptionHandler);

            // Kick off the workflow
            workflowInstance.Run();

            // Block this thread until the AutoResetEvent receives a signal for synchronous behavior (just in case)
            workflowCompleted.WaitOne();

            // Check to see if an exception was set, if so, throw it so the test framework can handle it
            if (savedException != null)
            {
                throw savedException;
            }
        }

        internal void ModelFirstVerifierRunner(string testName)
        {
            // TODO: Version1 !
            ModelFirstVerifierRunner(testName, EntityFrameworkVersion.Version1);
        }

        internal void ModelFirstVerifierRunner(string csdlArtifactName, Version versionOfCsdl)
        {
            UITestRunner.Execute(
                () =>
                    {
                        var workflowFilePath = Path.Combine(
                            VsUtils.GetVisualStudioInstallDir(),
                            @"Extensions\Microsoft\Entity Framework Tools\DBGen\TablePerTypeStrategy.xaml");

                        EdmItemCollection edmItemCollection;
                        using (
                            var csdlStream =
                                TestUtils.GetEmbeddedResourceStream("EFDesigner.InProcTests.TestData." + csdlArtifactName + ".csdl"))
                        {
                            edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(csdlStream) });
                        }

                        var testName = "ModelFirstVerifier_" + csdlArtifactName;

                        TestPipeline(
                            testName, edmItemCollection, workflowFilePath, versionOfCsdl,
                            args =>
                                {
                                    // Aggregate the inputs and outputs
                                    var ssdlOutput = (string)args.Outputs[EdmConstants.ssdlOutputName];
                                    var mslOutput = (string)args.Outputs[EdmConstants.mslOutputName];

                                    var storeItemCollection =
                                        EdmExtension.CreateAndValidateStoreItemCollection(
                                            ssdlOutput,
                                            EntityFrameworkVersion.Version2,
                                            new LegacyDbProviderServicesResolver(),
                                            false);

                                    // First we need to validate the MSL (the SSDL has already been validated
                                    // otherwise the SSDL to DDL step would have failed)
                                    new StorageMappingItemCollection(
                                        edmItemCollection,
                                        storeItemCollection,
                                        new[] { XmlReader.Create(new StringReader(mslOutput)) });

                                    var sb = new StringBuilder(
                                        new XElement(
                                            "StorageAndMappings",
                                            XElement.Parse(ssdlOutput, LoadOptions.PreserveWhitespace),
                                            new XText(Environment.NewLine + Environment.NewLine),
                                            new XComment("Finished generating the storage layer. Here are the mappings:"),
                                            new XText(Environment.NewLine + Environment.NewLine),
                                            XElement.Parse(mslOutput, LoadOptions.PreserveWhitespace)).ToString());

                                    // Enable the following when we can get the template to run InProc
                                    sb.AppendLine().AppendLine().AppendLine("The generated DDL:");
                                    sb.AppendLine(ScrubDdl((string)args.Outputs[EdmConstants.ddlOutputName]));

                                    return sb.ToString();
                                });
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_AssociationBetweenSubtypes()
        {
            // Association Between Subtypes
            ModelFirstVerifierRunner("AssociationBetweenSubtypes");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_AssociationBetweenSubtypes_NoNavProps()
        {
            // Association Between Subtypes without Navigation Properties (we shouldn't be relying on them)
            ModelFirstVerifierRunner("AssociationBetweenSubtypes_NoNavProps");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ManyManyAssociationBetweenSubtypes()
        {
            // *:* association between subtypes
            ModelFirstVerifierRunner("ManyManyAssociationBetweenSubtypes");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_SimpleInheritance()
        {
            // Simple inheritance
            ModelFirstVerifierRunner("SimpleInheritance");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ComplexInheritanceHierarchy()
        {
            // Complex inheritance hierarchy
            ModelFirstVerifierRunner("ComplexInheritanceHierarchy");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_DiscontinuedProduct()
        {
            // Association from root type to subtype
            ModelFirstVerifierRunner("DiscontinuedProduct");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ManyManyWithPkPkInheritance()
        {
            // Many to Many association bound to PK:PK with inheritance
            ModelFirstVerifierRunner("ManyManyWithPkPkInheritance");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_Northwind()
        {
            // Northwind (via ModelGen)
            ModelFirstVerifierRunner("Northwind");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_School()
        {
            // School model, testing TPT inheritance
            ModelFirstVerifierRunner("School");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_CompoundKeys()
        {
            // Simple entity type with compound keys.
            // This also tests Collation and Concurrency, which are ignored by the SQL Provider Manifest
            ModelFirstVerifierRunner("CompoundKeys");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OneToZeroOrOne()
        {
            // 1:0..1
            ModelFirstVerifierRunner("OneToZeroOrOne");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OneToMany()
        {
            // 1:* and 0..1:*
            ModelFirstVerifierRunner("OneToMany");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ManyToMany()
        {
            // *:*
            ModelFirstVerifierRunner("ManyToMany");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_PkPkWithSelfAssociation()
        {
            // PK:PK association with self association
            ModelFirstVerifierRunner("PkPkWithSelfAssociation");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_NestedComplexTypes()
        {
            // Model with nested ComplexTypes and inheritance
            ModelFirstVerifierRunner("NestedComplexTypes");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OneToManyCSideRefConstraint()
        {
            // Model with 1:* association, compound keys and C-side ref constraint
            ModelFirstVerifierRunner("OneToMany_CSideRefConstraint");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OneToOneCSideRefConstraint()
        {
            // Model with 1:1 association and self association with compound keys and C-side ref constraints
            ModelFirstVerifierRunner("OneToOne_CSideRefConstraint");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OneToZeroOrOneCSideRefConstraint()
        {
            // Model with 1:0..1 association and C-side ref constraint
            ModelFirstVerifierRunner("OneToZeroOrOne_CSideRefConstraint");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_OnDelete()
        {
            // This is 3 test cases rolled up into one:
            // 2: 'Cascade' defined normally on a '1' end
            // 3: 'Cascade' defined on 'OrderRC' end implies it is principal end but RefConstraint implies 'OrderLineItemRC' is the principal end
            // 4: 'Cascade' defined on 'OrderRCCorrect' end as well as RefConstraint implies it is principal end
            ModelFirstVerifierRunner("OnDelete", EntityFrameworkVersion.Version2);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_EmptyComplexType()
        {
            // This is 3 test cases:
            // 1. Make sure that a complex property of a complex type with no containing properties does not have a mapping
            // 2. Make sure that a complex property of a complex type containing a complex property of a complex type that does not have any containing properties does not have a mapping.
            // 3. A complex property of a CT with a CP of a CT that contains a scalar and a CP that is a CT that is empty
            ModelFirstVerifierRunner("EmptyComplexType", EntityFrameworkVersion.Version2);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ManyManySelfAssociation()
        {
            // Model with a simple *:* Self Association
            ModelFirstVerifierRunner("ManyManySelfAssociation", EntityFrameworkVersion.Version2);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_StoreGeneratedPattern()
        {
            // This is 9 test cases: (x/x indicates Should-be-mirrored-in-SSDL/Should-create-'IDENTITY(1,1)'-in-DDL)
            //               None    Identity    Computed       

            // Integer       0/0     1/1         1/0

            // NonInteger    0/0     1/0         1/0

            // Decimal       0/0     1/1         1/0
            ModelFirstVerifierRunner("StoreGeneratedPattern", EntityFrameworkVersion.Version2);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_EmptyModel()
        {
            // Empty Model, mainly testing that everything works without a CSDL namespace
            // (since we can't determine one from a blank CSDL)
            ModelFirstVerifierRunner("EmptyModel");
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_ComplexTypeCyclicCheck()
        {
            // Verify the cyclic complex type check against two complex properties mapped within
            // the hierarchy, in the same level, to the same complex type
            //
            // This will also test StoreGeneratedPattern across inheritance -- that we should not create IDENTITY
            // for subtypes' keys.
            ModelFirstVerifierRunner("ComplexTypeCyclicCheck", EntityFrameworkVersion.Version2);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ModelFirstVerifier_PropertyWithEnumType()
        {
            // Verify the property which type is an enum.
            ModelFirstVerifierRunner("PropertyWithEnumType", EntityFrameworkVersion.Version3);
        }

        private static string ScrubDdl(string ddl)
        {
            // need to remove the metadata at the beginning
            var startOfScript = ddl.IndexOf("SET QUOTED_IDENTIFIER");
            return startOfScript >= 0
                       ? ddl.Substring(startOfScript)
                       : ddl;
        }
    }
}
