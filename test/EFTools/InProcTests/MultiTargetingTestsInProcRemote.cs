// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Test.Unit.InProc
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Threading;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.EFDesigner;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;
    using Microsoft.Win32;

    [TestClass]
    public class MultiTargetingTestsInProcRemote
    {
        public TestContext TestContext { get; set; }

        private static DTE Dte
        {
            get { return VsIdeTestHostContext.Dte; }
        }

        private readonly EFArtifactHelper _efArtifactHelper =
            new EFArtifactHelper(EFArtifactHelper.GetEntityDesignModelManager(VsIdeTestHostContext.ServiceProvider));

        private enum FrameworkVersion
        {
            V30,
            V35,
            V40
        };

        private static IDictionary<FrameworkVersion, string> _mapFameworkRegistryPath;

        public MultiTargetingTestsInProcRemote()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);

            _mapFameworkRegistryPath = new Dictionary<FrameworkVersion, string>();
            _mapFameworkRegistryPath[FrameworkVersion.V35] = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5";
            _mapFameworkRegistryPath[FrameworkVersion.V40] = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4.0";
        }

        private class ArtifactStatus
        {
            public readonly bool IsDesignerSafe;
            public readonly bool IsStructurallySafe;
            public readonly bool IsVersionSafe;

            public ArtifactStatus(bool isDesignerSafe, bool isStructurallySafe, bool isVersionSafe)
            {
                IsDesignerSafe = isDesignerSafe;
                IsStructurallySafe = isStructurallySafe;
                IsVersionSafe = isVersionSafe;
            }
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void MultiTargeting40ProjectFileOpen()
        {
            if (!NETFrameworkVersionInstalled(FrameworkVersion.V40))
            {
                TestContext.WriteLine(
                    TestContext.TestName + "skipped - .NET Framework " + FrameworkVersion.V40.ToString("G") + " not installed.");
                return;
            }

            TestProjectMultiTargeting(
                "MT40Project",
                "TestMultiTargeting40Project",
                new Dictionary<FrameworkVersion, ArtifactStatus>
                    {
                        { FrameworkVersion.V40, new ArtifactStatus(isDesignerSafe: true, isStructurallySafe: true, isVersionSafe: true) },
                        { FrameworkVersion.V35, new ArtifactStatus(isDesignerSafe: false, isStructurallySafe: true, isVersionSafe: false) }
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void MultiTargeting35ProjectFileOpen()
        {
            if (!NETFrameworkVersionInstalled(FrameworkVersion.V35))
            {
                TestContext.WriteLine(
                    TestContext.TestName + "skipped - .NET Framework " + FrameworkVersion.V35.ToString("G") + " not installed.");
                return;
            }

            TestProjectMultiTargeting(
                "MT35Project",
                "TestMultiTargeting35Project",
                new Dictionary<FrameworkVersion, ArtifactStatus>
                    {
                        { FrameworkVersion.V40, new ArtifactStatus(isDesignerSafe: false, isStructurallySafe: true, isVersionSafe: false) },
                        { FrameworkVersion.V35, new ArtifactStatus(isDesignerSafe: true, isStructurallySafe: true, isVersionSafe: true) }
                    });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void MultiTargeting30ProjectFileOpen()
        {
            // we can't use registry to determine whether net fx 3.0
            if (!NETFrameworkVersionInstalled(FrameworkVersion.V35))
            {
                TestContext.WriteLine(
                    TestContext.TestName + "skipped - .NET Framework " + FrameworkVersion.V35.ToString("G") + " not installed.");
                return;
            }

            TestProjectMultiTargeting(
                "MT30Project",
                "TestMultiTargeting30Project",
                new Dictionary<FrameworkVersion, ArtifactStatus>
                    {
                        { FrameworkVersion.V40, new ArtifactStatus(isDesignerSafe: false, isStructurallySafe: true, isVersionSafe: false) },
                        { FrameworkVersion.V35, new ArtifactStatus(isDesignerSafe: false, isStructurallySafe: true, isVersionSafe: false) }
                    });
        }

        private void TestProjectMultiTargeting(
            string projectName, string testName, IDictionary<FrameworkVersion, ArtifactStatus> expectedResults)
        {
            var projectDir = Path.Combine(TestContext.DeploymentDirectory, @"TestData\InProc\MultiTargeting", projectName);
            var solnFilePath = Path.Combine(projectDir, projectName + ".sln");
            try
            {
                Dte.OpenSolution(solnFilePath);

                // We need to wait until the project loading events are processed
                // Otherwise, we could get into a state where we open the EDMX file
                // into the miscellaneous files project

                Project project;
                var attempts = 100;
                do
                {
                    project = Dte.FindProject(projectName);
                    ProcessEvents();
                    if (attempts-- < 0)
                    {
                        Assert.Fail("Cannot open solution");
                    }
                }
                while (project == null);

                TestLoadingArtifact(project, projectDir + @"\NorthwindModel_40.edmx", expectedResults[FrameworkVersion.V40]);
                TestLoadingArtifact(project, projectDir + @"\NorthwindModel_35.edmx", expectedResults[FrameworkVersion.V35]);
            }
            finally
            {
                Dte.CloseSolution(false);
            }
        }

        private void TestLoadingArtifact(Project project, string filePath, ArtifactStatus expectedArtifactStatus)
        {
            Assert.IsNotNull(expectedArtifactStatus);

            // get a new artifact, which will automatically open up the EDMX file in VS
            EntityDesignArtifact entityDesignArtifact = null;

            try
            {
                var edmxProjectItem = project.GetProjectItemByName(Path.GetFileName(filePath));
                Assert.IsNotNull(edmxProjectItem);
                Dte.OpenFile(edmxProjectItem.FileNames[0]);

                entityDesignArtifact =
                    (EntityDesignArtifact)_efArtifactHelper.GetNewOrExistingArtifact(
                        TestUtils.FileName2Uri(edmxProjectItem.FileNames[0]));

                Assert.AreEqual(expectedArtifactStatus.IsStructurallySafe, entityDesignArtifact.IsStructurallySafe);
                Assert.AreEqual(expectedArtifactStatus.IsVersionSafe, entityDesignArtifact.IsVersionSafe);
                Assert.AreEqual(expectedArtifactStatus.IsDesignerSafe, entityDesignArtifact.IsDesignerSafe);
            }
            finally
            {
                if (entityDesignArtifact != null)
                {
                    Dte.CloseDocument(entityDesignArtifact.Uri.LocalPath, false);
                }
            }
        }

        /// <summary>
        ///     Determines whether a test should be skipped or not. We skip the test if:
        ///     - the skip-test registry key exists.
        ///     - the required .net framework is not installed in the target machine.
        /// </summary>
        /// <param name="requiredFrameworkVersion"></param>
        /// <returns></returns>
        private bool NETFrameworkVersionInstalled(FrameworkVersion requiredFrameworkVersion)
        {
            return Registry.LocalMachine.OpenSubKey(_mapFameworkRegistryPath[requiredFrameworkVersion]) != null;
        }

        private void ProcessEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background, new DispatcherOperationCallback(o => frame.Continue = false), null);
            Dispatcher.PushFrame(frame);
        }
    }
}
