// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SystemThread = System.Threading.Thread;

namespace EFDesigner.InProcTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Automation;
    using System.Xml.Linq;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;
    using TestStack.White;
    using TestStack.White.Factory;
    using TestStack.White.InputDevices;
    using TestStack.White.UIItems;
    using TestStack.White.UIItems.Finders;
    using TestStack.White.UIItems.ListBoxItems;
    using TestStack.White.UIItems.TreeItems;
    using TestStack.White.UIItems.WPFUIItems;
    using Process = System.Diagnostics.Process;
    using Window = TestStack.White.UIItems.WindowItems.Window;

    /// <summary>
    ///     This class has End to end tests for designer UI
    /// </summary>
    //[TestClass]
    public class WhiteE2ETests
    {
        private readonly ResourcesHelper _resourceHelper;
        private Application _visualStudio;
        private Window _visualStudioMainWindow;
        private Window _wizard;
        private int _nameSuffix = 1;

        // Define model db attributes
        private const string ModelName = "LibraryModel";
        private const string EntityName = "Book";

        private readonly List<string> _entityAttributes = new List<string>
        {
            "BookID",
            "Author",
            "Code",
            "PageCount",
            "Title",
            "Topic"
        };

        public WhiteE2ETests()
        {
            _resourceHelper = new ResourcesHelper();
        }

        public TestContext TestContext { get; set; }

        private static DTE Dte
        {
            get { return VsIdeTestHostContext.Dte; }
        }

        [ClassInitialize]
        public static void ClasInitialize(TestContext testContext)
        {
            // Create a simple DB for tests to use
            CreateDatabase();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DropDatabase();
        }

        /// <summary>
        ///     Simple E2E test that selects empty model in the wizard
        /// </summary>
        //[TestMethod]
        //[HostType("VS IDE")]
        public void AddAnEmptyModel()
        {
            Exception exceptionCaught = null;
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + "Starting the test");

            // We need to create this thread to keep polling for wizard to show up and
            // walk thru the wizard. DTE call just launches the wizard and stays there
            // taking up the main thread
            var wizardDiscoveryThread = ExecuteThreadedAction(
                () =>
                {
                    try
                    {
                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":In thread wizardDiscoveryThread");

                        // This method polls for the the wizard to show up
                        _wizard = GetWizard();

                        // Walk thru the Empty model selection
                        CreateEmptyModel(_wizard);
                    }
                    catch (Exception ex)
                    {
                        exceptionCaught = ex;
                    }
                }, "UIExecutor");

            // On the main thread create a project
            var project = Dte.CreateProject(
                TestContext.TestRunDirectory,
                "EmptyModelTest",
                DteExtensions.ProjectKind.Executable,
                DteExtensions.ProjectLanguage.CSharp);

            Assert.IsNotNull(project, "Could not create project");

            // Launch the Model wizard
            DteExtensions.AddNewItem(Dte, @"Data\ADO.NET Entity Data Model", "Model1.edmx", project);

            wizardDiscoveryThread.Join();

            if (exceptionCaught != null)
            {
                throw exceptionCaught;
            }

            Trace.WriteLine(DateTime.Now.ToLongTimeString() + "wizardDiscoveryThread returned");

            // Make sure the expected files are created
            var csfile = project.GetProjectItemByName("Model1.designer.cs");
            Assert.IsNotNull(csfile, "csfile is not null");

            var diagramFile = project.GetProjectItemByName("Model1.edmx.diagram");
            Assert.IsNotNull(diagramFile, "diagramfile not null");

            var errors = Build();
            Assert.IsTrue(errors == null || errors.Count == 0);

            var designerSurface = _visualStudioMainWindow.Get<Panel>(SearchCriteria.ByText("Modeling Design Surface"));
            var entity1 = AddEntity(designerSurface);
            var entity2 = AddEntity(designerSurface);
            AddAssociation(designerSurface);
            var entity3 = AddEntity(designerSurface);
            AddInheritance(designerSurface, entity2, entity3);
            AddEnum(designerSurface);

            project.Save();
            CheckFilesEmptyModel(project);
        }

        private string AddEntity(IUIItem designerSurface)
        {
            designerSurface.RightClickAt(designerSurface.Location);
            var popUpMenu = _visualStudioMainWindow.Popup;
            var menuItem = popUpMenu.Item("Add New");
            menuItem.Click();
            var newEntity =
                menuItem.SubMenu(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("AddEntityTypeCommand_DesignerText")));
            newEntity.Click();
            var addEntity = _visualStudio.Find(
                x => x.Equals(_resourceHelper.GetEntityDesignResourceString("NewEntityDialog_Title")),
                InitializeOption.NoCache);

            var entityName =
                addEntity.Get<TextBox>(
                    SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("NewEntityDialog_EntityNameLabel")));
            var entityNameText = string.Format("Entity_{0}", _nameSuffix++);
            entityName.Text = entityNameText;
            var okButton =
                addEntity.Get<Button>(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("OKButton_AccessibleName")));
            okButton.Click();
            return entityNameText;
        }

        private void AddAssociation(IUIItem designerSurface)
        {
            designerSurface.RightClickAt(designerSurface.Location);
            var popUpMenu = _visualStudioMainWindow.Popup;
            var menuItem = popUpMenu.Item("Add New");
            menuItem.Click();
            var newAssociation = menuItem.SubMenu(SearchCriteria.ByText("Association..."));
            newAssociation.Click();
            var addEntity = _visualStudio.Find(
                x => x.Equals(_resourceHelper.GetEntityDesignResourceString("NewAssociationDialog_Title")),
                InitializeOption.NoCache);
            var okButton =
                addEntity.Get<Button>(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("OKButton_AccessibleName")));
            okButton.Click();
        }

        private void AddInheritance(IUIItem designerSurface, string baseEntity, string derivedEntity)
        {
            designerSurface.RightClickAt(designerSurface.Location);
            var popUpMenu = _visualStudioMainWindow.Popup;
            var menuItem = popUpMenu.Item("Add New");
            menuItem.Click();
            var newInheritance = menuItem.SubMenu(SearchCriteria.ByText("Inheritance..."));
            newInheritance.Click();
            var addInheritance = _visualStudio.Find(
                x => x.Equals(_resourceHelper.GetEntityDesignResourceString("NewInheritanceDialog_Title")),
                InitializeOption.NoCache);
            var entity1 =
                addInheritance.Get<ComboBox>(
                    SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("NewInheritanceDialog_SelectBaseEntity")));
            entity1.Item(baseEntity).Select();
            var entity2 =
                addInheritance.Get<ComboBox>(
                    SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("NewInheritanceDialog_SelectDerivedEntity")));
            entity2.Item(derivedEntity).Select();
            var okButton =
                addInheritance.Get<Button>(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("OKButton_AccessibleName")));
            okButton.Click();
        }

        private void AddEnum(IUIItem designerSurface)
        {
            designerSurface.RightClickAt(designerSurface.Location);
            var popUpMenu = _visualStudioMainWindow.Popup;
            var menuItem = popUpMenu.Item("Add New");
            menuItem.Click();
            var newEnum =
                menuItem.SubMenu(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("AddEnumTypeCommand_DesignerText")));
            newEnum.Click();
            var addEnum = _visualStudio.Find(
                x => x.Equals(_resourceHelper.GetEntityDesignResourceString("EnumDialog_NewEnumWindowTitle")),
                InitializeOption.NoCache);
            var enumTypeName = addEnum.Get<TextBox>(SearchCriteria.ByAutomationId("txtEnumTypeName"));
            enumTypeName.SetValue("EnumType" + _nameSuffix);

            var table = addEnum.Get<ListView>(SearchCriteria.ByAutomationId("dgEnumTypeMembers"));

            foreach (var row in table.Rows)
            {
                row.Select();
                row.DoubleClick();
                row.Focus();
            }

            var rowEntry =
                table.Get<UIItem>(
                    SearchCriteria.ByText(
                        "Item: Microsoft.Data.Entity.Design.UI.ViewModels.EnumTypeMemberViewModel, Column Display Index: 0"));
            var rowEntryValuePattern =
                (ValuePattern)
                    rowEntry.AutomationElement.GetCurrentPattern(ValuePattern.Pattern);
            rowEntryValuePattern.SetValue("EnumName" + _nameSuffix++);

            var btnOk = addEnum.Get<Button>(SearchCriteria.ByAutomationId("btnOk"));
            btnOk.Click();
        }

        private static void CheckFilesEmptyModel(Project project)
        {
            var edmxFile = project.GetProjectItemByName("Model1.edmx");
            Assert.IsNotNull(edmxFile, "edmxfile is null");

            var edmxPath = edmxFile.Properties.Item("FullPath").Value.ToString();
            var xmlEntries = XDocument.Load(edmxPath);
            var edmx = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx");
            var edm = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");

            var conceptualElement = xmlEntries.Elements(edmx + "Edmx")
                .Elements(edmx + "Runtime")
                .Elements(edmx + "ConceptualModels")
                .Elements(edm + "Schema").First();
            Assert.AreEqual((string)conceptualElement.Attribute("Namespace"), "Model1");

            var entity1Element = (from el in conceptualElement.Descendants(edm + "EntityType")
                where (string)el.Attribute("Name") == "Entity_1"
                select el).First();
            Assert.IsNotNull(entity1Element);

            var entity2Element = (from el in conceptualElement.Descendants(edm + "EntityType")
                where (string)el.Attribute("Name") == "Entity_2"
                select el).First();
            Assert.IsNotNull(entity2Element);

            var entity3Element = (from el in conceptualElement.Descendants(edm + "EntityType")
                where (string)el.Attribute("Name") == "Entity_3"
                select el).First();
            Assert.IsNotNull(entity3Element);

            var associationElement = (from el in conceptualElement.Descendants(edm + "Association")
                where (string)el.Attribute("Name") == "Entity_1Entity_2"
                select el).First();
            Assert.IsNotNull(associationElement);

            var enumElement = (from el in conceptualElement.Descendants(edm + "EnumType")
                where (string)el.Attribute("Name") == "EnumType4"
                select el).First();
            Assert.IsNotNull(enumElement);
        }

        //[TestMethod]
        //[HostType("VS IDE")]
        //[Timeout(4 * 60 * 1000)]
        public void AddModelFromExistingDB()
        {
            Exception exceptionCaught = null;
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + "Starting the test");

            // We need to create this thread to keep polling for wizard to show up and
            // walk thru the wizard. DTE call just launches the wizard and stays there
            // taking up the main thread
            var wizardDiscoveryThread = ExecuteThreadedAction(
                () =>
                {
                    try
                    {
                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":In thread wizardDiscoveryThread");

                        // This method polls for the the wizard to show up
                        _wizard = GetWizard();

                        // Walk thru the Wizard with existing DB option
                        CreateModelFromDB(_wizard, "Library");
                    }
                    catch (Exception ex)
                    {
                        exceptionCaught = ex;
                    }
                }, "UIExecutor");

            // On the main thread create a project
            var project = Dte.CreateProject(
                TestContext.TestRunDirectory,
                "ExistingDBTest",
                DteExtensions.ProjectKind.Executable,
                DteExtensions.ProjectLanguage.CSharp);

            Assert.IsNotNull(project, "Could not create project");

            // Create model from a small size database
            DteExtensions.AddNewItem(Dte, @"Data\ADO.NET Entity Data Model", "Model1.edmx", project);

            wizardDiscoveryThread.Join();

            if (exceptionCaught != null)
            {
                throw exceptionCaught;
            }

            // Make sure all expected files are generated.
            // Make sure EDMX is what you expected.
            CheckFilesExistingModel(project);

            // Build the project
            var errors = Build();
            Assert.IsTrue(errors == null || errors.Count == 0);

            // Check Model browser, mapping window, properties window to spot check them
            Dte.ExecuteCommand("View.EntityDataModelBrowser");
            var modelBrowserTree = _visualStudioMainWindow.Get<Tree>(SearchCriteria.ByAutomationId("ExplorerTreeView"));

            // See if entities exist in diagram
            var diagramNode = modelBrowserTree.Node("Model1.edmx", "Diagrams Diagrams", "Diagram Diagram1");
            ((ExpandCollapsePattern)diagramNode.AutomationElement.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
            foreach (var childNode in diagramNode.Nodes)
            {
                Dte.ExecuteCommand("View.EntityDataModelBrowser");
                ((SelectionItemPattern)childNode.AutomationElement.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();

                CheckProperties(ModelName + "." + EntityName + ":EntityType");
                Assert.AreEqual("EntityTypeShape " + EntityName, childNode.Text);
            }

            // See if entities exist in model and have expected properties
            Dte.ExecuteCommand("View.EntityDataModelBrowser");
            var modelNode = modelBrowserTree.Node(
                "Model1.edmx", "ConceptualEntityModel " + ModelName,
                "Entity Types");
            CheckEntityProperties(modelNode, "ConceptualEntityType {0}", "ConceptualProperty ", "{0}.{1}.{2}:Property");

            // See if entities exist in store and have expected properties
            Dte.ExecuteCommand("View.EntityDataModelBrowser");
            var storeNode = modelBrowserTree.Node(
                "Model1.edmx", "StorageEntityModel " + ModelName + ".Store",
                "Tables / Views");
            CheckEntityProperties(storeNode, "StorageEntityType {0}s", "StorageProperty ", "{0}.Store.{1}s.{2}:Property");

            // Future: Automate the designer surface.

            // Make sure app.config is updated correctly
            CheckAppConfig(project);
        }

        private void CheckProperties(string property)
        {
            Dte.ExecuteCommand("View.PropertiesWindow", string.Empty);
            var componentsBox = _visualStudioMainWindow.Get<ComboBox>(SearchCriteria.ByText("Components"));
            Assert.AreEqual(property, componentsBox.Item(0).Text);
        }

        private static void CheckFilesExistingModel(Project project)
        {
            var csfile = project.GetProjectItemByName("Model1.designer.cs");
            Assert.IsNotNull(csfile, "csfile is null");

            var diagramFile = project.GetProjectItemByName("Model1.edmx.diagram");
            Assert.IsNotNull(diagramFile, "diagramfile is null");

            var contextFile = project.GetProjectItemByName("Model1.context.cs");
            Assert.IsNotNull(contextFile, "context is null");

            var ttFile = project.GetProjectItemByName("Model1.tt");
            Assert.IsNotNull(ttFile, "ttfile is null");

            var bookfile = project.GetProjectItemByName("book.cs");
            Assert.IsNotNull(bookfile, "bookfile is null");

            var edmxFile = project.GetProjectItemByName("Model1.edmx");
            Assert.IsNotNull(edmxFile, "edmxfile is null");

            var edmxPath = edmxFile.Properties.Item("FullPath").Value.ToString();
            var xmlEntries = XDocument.Load(edmxPath);
            var edmx = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx");
            var ssdl = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
            var cs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs");

            var element = xmlEntries.Elements(edmx + "Edmx")
                .Elements(edmx + "Runtime")
                .Elements(edmx + "StorageModels")
                .Elements(ssdl + "Schema").First();
            Assert.AreEqual((string)element.Attribute("Namespace"), "LibraryModel.Store");
            Assert.AreEqual((string)element.Attribute("Provider"), "System.Data.SqlClient");

            element = (from el in element.Descendants(ssdl + "EntityType").First().Descendants(ssdl + "Property")
                where (string)el.Attribute("Name") == "BookID"
                select el).First();
            Assert.IsNotNull(element);
            Assert.AreEqual((string)element.Attribute("Type"), "int");
            Assert.AreEqual((string)element.Attribute("StoreGeneratedPattern"), "Identity");
            Assert.AreEqual((string)element.Attribute("Nullable"), "false");

            element = xmlEntries.Elements(edmx + "Edmx")
                .Elements(edmx + "Runtime")
                .Elements(edmx + "Mappings")
                .Elements(cs + "Mapping")
                .Elements(cs + "EntityContainerMapping").First();
            Assert.AreEqual((string)element.Attribute("StorageEntityContainer"), "LibraryModelStoreContainer");
            Assert.AreEqual((string)element.Attribute("CdmEntityContainer"), "LibraryEntities");
        }

        private void CheckEntityProperties(TreeNode node, string entityType, string entityProperty, string propertyValue)
        {
            Dte.ExecuteCommand("View.EntityDataModelBrowser");
            ((ExpandCollapsePattern)node.AutomationElement.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
            foreach (var childNode in node.Nodes)
            {
                Assert.AreEqual(string.Format(entityType, EntityName), childNode.Text);
                Dte.ExecuteCommand("View.EntityDataModelBrowser");
                ((ExpandCollapsePattern)childNode.AutomationElement.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
                foreach (var propertyNode in childNode.Nodes)
                {
                    Dte.ExecuteCommand("View.EntityDataModelBrowser");
                    ((SelectionItemPattern)propertyNode.AutomationElement.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
                    var cleanProperty = propertyNode.Text.Replace(entityProperty, "");

                    CheckProperties(string.Format(propertyValue, ModelName, EntityName, cleanProperty));
                    Assert.IsTrue(_entityAttributes.Contains(cleanProperty));
                }
            }
        }

        private static void CheckAppConfig(Project project)
        {
            var appConfigFile = project.GetProjectItemByName("app.config");
            var appConfigPath = appConfigFile.Properties.Item("FullPath").Value.ToString();

            var xmlEntries = XDocument.Load(appConfigPath);

            var element = xmlEntries.Elements("configuration")
                .Elements("connectionStrings")
                .Elements("add").First();
            Assert.AreEqual((string)element.Attribute("name"), "LibraryEntities");
        }

        /// <summary>
        ///     Tries to find the wizard after it is launched.
        /// </summary>
        /// <returns>the wizard element</returns>
        private Window GetWizard()
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":In GetWizard");

            _visualStudio = Application.Attach(Process.GetCurrentProcess().Id);
            _visualStudioMainWindow = _visualStudio.GetWindow(
                SearchCriteria.ByAutomationId("VisualStudioMainWindow"),
                InitializeOption.NoCache);

            var wizard = _visualStudio.Find(
                x => x.Equals(_resourceHelper.GetEntityDesignResourceString("WizardFormDialog_Title")),
                InitializeOption.NoCache);
            return wizard;
        }

        /// <summary>
        ///     Chooses 'Create Empty Model' option for the wizard and clicks finish button
        /// </summary>
        private void CreateEmptyModel(UIItemContainer wizard)
        {
            // Select the empty model option
            SelectModelOption(
                wizard,
                _resourceHelper.GetEntityDesignResourceString("EmptyModelOption"));

            var finish = wizard.Get<Button>(SearchCriteria.ByText(_resourceHelper.GetWizardFrameworkResourceString("ButtonFinishText")));
            finish.Click();
        }

        /// <summary>
        ///     Chooses the 'Generate from database' option for the wizard and walks thru the wizard
        /// </summary>
        private void CreateModelFromDB(Window wizard, string dbName)
        {
            // Select the 'Generate from Database' option
            SelectModelOption(
                wizard,
                _resourceHelper.GetEntityDesignResourceString("GenerateFromDatabaseOption"));

            ClickNextButton(wizard);

            var appconfig = _wizard.Get<TextBox>(SearchCriteria.ByAutomationId("textBoxAppConfigConnectionName"));
            if (appconfig.Text != "LibraryEntities")
            {
                var newConnectionButton =
                    wizard.Get<Button>(
                        SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("NewDatabaseConnectionBtn")));

                newConnectionButton.Click();
                HandleConnectionDialog(wizard, dbName);
                wizard.WaitTill(WaitTillNewDBSettingsLoaded);
            }

            ClickNextButton(wizard);

            var versionsPanel = _wizard.Get<Panel>(SearchCriteria.ByAutomationId("versionsPanel"));
            var selectionButton =
                versionsPanel.Get<RadioButton>(
                    SearchCriteria.ByText(String.Format(_resourceHelper.GetEntityDesignResourceString("EntityFrameworkVersionName"), "6.0")));
            Assert.IsTrue(selectionButton.IsSelected);
            ClickNextButton(wizard);

            wizard.WaitTill(WaitTillDBTablesLoaded, new TimeSpan(0, 1, 0));
            CheckAllCheckBoxes(wizard);

            var finishButton = wizard.Get<Button>(
                SearchCriteria.ByText(_resourceHelper.GetWizardFrameworkResourceString("ButtonFinishText")));
            finishButton.Click();
        }

        private void SelectModelOption(UIItemContainer wizard, string modelOption)
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":SelectModelOption");
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":trying to find option list");

            var options =
                wizard.Get<ListBox>(SearchCriteria.ByText(_resourceHelper.GetEntityDesignResourceString("StartPage_PromptLabelText")));
            var item = options.Item(modelOption);
            item.Select();
        }

        private bool WaitTillNewDBSettingsLoaded()
        {
            var appconfig = _wizard.Get<TextBox>(SearchCriteria.ByAutomationId("textBoxAppConfigConnectionName"));
            return appconfig.Text.Equals("LibraryEntities");
        }

        private bool WaitTillDBTablesLoaded()
        {
            var modelNamespaceTextBox = _wizard.Get<TextBox>(SearchCriteria.ByAutomationId("modelNamespaceTextBox"));
            return modelNamespaceTextBox.Enabled;
        }

        // checks all the checkboxes on the "which database objects to select" dialog
        private static void CheckAllCheckBoxes(UIItemContainer wizard)
        {
            var pane = wizard.Get<Panel>(SearchCriteria.ByAutomationId("treeView"));
            var tree = pane.Get<Tree>(SearchCriteria.ByControlType(ControlType.Tree));

            Assert.IsTrue(tree.Nodes.Count != 0);
            foreach (var checkbox in tree.Nodes)
            {
                checkbox.Click();
                Keyboard.Instance.Enter(" ");
            }
        }

        // Handles the new connection dialog
        private void HandleConnectionDialog(UIItemContainer wizard, string dbName)
        {
            var serverNameText = wizard.Get<TextBox>(
                SearchCriteria.ByText(
                    _resourceHelper.GetConnectionUIDialogResourceString("serverLabel.Text")));
            serverNameText.Enter(@"(localdb)\v11.0");

            var refreshButton = wizard.Get<Button>(
                SearchCriteria.ByText(
                    _resourceHelper.GetConnectionUIDialogResourceString("refreshButton.Text")));
            refreshButton.Focus();

            var dbNameText = wizard.Get<TextBox>(
                SearchCriteria.ByText(
                    _resourceHelper.GetConnectionUIDialogResourceString("selectDatabaseRadioButton.Text")));
            dbNameText.Enter(dbName);

            var okButton = wizard.Get<Button>(
                SearchCriteria.ByText(
                    _resourceHelper.GetConnectionUIDialogResourceString("acceptButton.Text")));
            okButton.Click();
        }

        // Invokes click on next button
        private void ClickNextButton(UIItemContainer wizard)
        {
            var finish = wizard.Get<Button>(
                SearchCriteria.ByText(_resourceHelper.GetWizardFrameworkResourceString("ButtonNextText")));
            finish.Click();
        }

        // Creates a new thread for action and starts it
        private static SystemThread ExecuteThreadedAction(Action action, string threadName = "Worker")
        {
            var thread = new SystemThread(
                new ThreadStart(action)) { Name = threadName };
            thread.Start();
            return thread;
        }

        /// <summary>
        ///     Create a simple db for tests to use
        /// </summary>
        private static void CreateDatabase()
        {
            try
            {
                DropDatabase();
            }
            catch
            {
            }

            const string createDatabase = "CREATE DATABASE Library";
            const string createTable = "CREATE TABLE Books (BookID INTEGER PRIMARY KEY IDENTITY," +
                                       "Title CHAR(50) NOT NULL , Author CHAR(50), " +
                                       "PageCount INTEGER,Topic CHAR(30),Code CHAR(15))";
            const string insertFirstRow = "INSERT INTO BOOKS (TITLE,AUTHOR,PAGECOUNT,TOPIC,CODE)"
                                          + "VALUES('Test Book','Test Author', 100, 'Test Topic', 'Test Code');";

            ExecuteSqlCommand(
                @"Data Source=(localdb)\v11.0;initial catalog=Master;integrated security=True",
                createDatabase);

            const string connectionString = @"Data Source=(localdb)\v11.0;initial catalog=Library;integrated security=True;Pooling=false";

            ExecuteSqlCommand(connectionString, createTable);
            ExecuteSqlCommand(connectionString, insertFirstRow);
        }

        /// <summary>
        ///     Create a simple db for tests to use
        /// </summary>
        private static void DropDatabase()
        {
            ExecuteSqlCommand(
                @"Data Source=(localdb)\v11.0;initial catalog=Master;integrated security=True",
                "DROP DATABASE Library");
        }

        private static void ExecuteSqlCommand(string connectionString, string commandString)
        {
            using (var mycon = new SqlConnection(connectionString))
            {
                try
                {
                    var mycomm = new SqlCommand { CommandType = CommandType.Text, CommandText = commandString, Connection = mycon };

                    mycon.Open();
                    SqlConnection.ClearAllPools();
                    mycomm.ExecuteNonQuery();
                }
                finally
                {
                    mycon.Close();
                }
            }
        }

        public static ErrorItems Build()
        {
            Dte.ExecuteCommand("Build.BuildSelection", String.Empty);
            var errors = GetErrors();

            return errors;
        }

        public static ErrorItems GetErrors()
        {
            var dte2 = (DTE2)Dte;
            Dte.ExecuteCommand("View.ErrorList", string.Empty);
            return dte2.ToolWindows.ErrorList.ErrorItems;
        }
    }
}
