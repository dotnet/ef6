// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SystemThread = System.Threading.Thread;

namespace EFDesigner.InProcTests
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Automation;
    using EFDesignerTestInfrastructure;
    using EFDesignerTestInfrastructure.VS;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestTools.VsIdeTesting;
    using Process = System.Diagnostics.Process;

    /// <summary>
    ///     This class has End to end tests for designer UI
    /// </summary>
    [TestClass]
    public class E2ETests
    {
        private readonly IEdmPackage _package;
        private readonly ResourcesHelper _resourceHelper;

        /// <summary>
        ///     Number of retries for UI operation if it fails
        /// </summary>
        private const int numRetry = 5;

        public E2ETests()
        {
            PackageManager.LoadEDMPackage(VsIdeTestHostContext.ServiceProvider);
            _package = PackageManager.Package;
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
        [TestMethod]
        [HostType("VS IDE")]
        [Timeout(2 * 60 * 1000)]
        [TestCategory("DoNotRunOnCI")]
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
                            var wizard = GetWizard();

                            // Walk thru the Empty model selection
                            CreateEmptyModel(wizard);
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
        }

        [TestMethod]
        [HostType("VS IDE")]
        [Timeout(4 * 60 * 1000)]
        [TestCategory("DoNotRunOnCI")]
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
                            var wizard = GetWizard();

                            // Walk thru the Wizard with existing DB option
                            CreateModelFromDB(wizard, "Library");
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

            // Launch the Model wizard
            DteExtensions.AddNewItem(Dte, @"Data\ADO.NET Entity Data Model", "Model1.edmx", project);

            wizardDiscoveryThread.Join();

            if (exceptionCaught != null)
            {
                throw exceptionCaught;
            }

            // Make sure expected files are generated
            var csfile = project.GetProjectItemByName("Model1.designer.cs");
            Assert.IsNotNull(csfile, "csfile is null");

            var diagramFile = project.GetProjectItemByName("Model1.edmx.diagram");
            Assert.IsNotNull(diagramFile, "diagramfile is null");

            var contextFile = project.GetProjectItemByName("Model1.context.cs");
            Assert.IsNotNull(contextFile, "context is null");

            var ttFile = project.GetProjectItemByName("Model1.tt");
            Assert.IsNotNull(ttFile, "ttfile is null");

            var bookfile = project.GetProjectItemByName("book.cs");
            Assert.IsNotNull(ttFile, "bookfile is null");
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

            var createDatabase = "CREATE DATABASE Library";
            var createTable = "CREATE TABLE Books (BookID INTEGER PRIMARY KEY IDENTITY," +
                              "Title CHAR(50) NOT NULL , Author CHAR(50), " +
                              "PageCount INTEGER,Topic CHAR(30),Code CHAR(15))";
            var insertFirstRow = "INSERT INTO BOOKS (TITLE,AUTHOR,PAGECOUNT,TOPIC,CODE)"
                                 + "VALUES('Test Book','Test Author', 100, 'Test Topic', 'Test Code');";

            ExecuteSqlCommand(
                @"Data Source=(localdb)\v11.0;initial catalog=Master;integrated security=True",
                createDatabase);

            var connectionString =
                @"Data Source=(localdb)\v11.0;initial catalog=Library;integrated security=True;Pooling=false";

            InvokeOperationWithRetry(() => { ExecuteSqlCommand(connectionString, createTable); });
            InvokeOperationWithRetry(() => { ExecuteSqlCommand(connectionString, insertFirstRow); });
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
            using (var mycon = new SqlConnection())
            {
                try
                {
                    mycon.ConnectionString = connectionString;

                    var mycomm = new SqlCommand();
                    mycomm.CommandType = CommandType.Text;
                    mycomm.CommandText = commandString;
                    mycomm.Connection = mycon;

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

        /// <summary>
        ///     Tries to find the wizard after it is launched.
        /// </summary>
        /// <returns>the wizard element</returns>
        private AutomationElement GetWizard()
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":In GetWizard");

            // Find the visual studio 
            var condition = new PropertyCondition(
                AutomationElement.ProcessIdProperty,
                Process.GetCurrentProcess().Id,
                PropertyConditionFlags.None);

            var VisualStudio = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);

            if (VisualStudio == null)
            {
                Trace.WriteLine(DateTime.Now.ToLongTimeString() + "Could not find VS");
                throw new InvalidOperationException("InGetWizard:Could not find VS");
            }

            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":trying to find wizard");

            // try to find the wizard   
            var wizard = FindElement(
                VisualStudio,
                _resourceHelper.GetModelWizardResourceString("WizardFormDialog_Title"));

            return wizard;
        }

        /// <summary>
        ///     Chooses 'Create Empty Model' option for the wizard and clicks finish button
        /// </summary>
        private void CreateEmptyModel(AutomationElement wizard)
        {
            // Select the empty model option
            SelectModelOption(
                wizard,
                _resourceHelper.GetModelWizardResourceString("EmptyModelOption"));

            InvokeOperationWithRetry(
                () =>
                    {
                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Finding finish button");
                        var finishButton = FindElement(
                            wizard,
                            _resourceHelper.GetWizardResourceString("ButtonFinishText"));

                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Invoking Finish button");
                        InvokeClick(finishButton);
                    });
        }

        /// <summary>
        ///     Chooses the 'Generate from database' option for the wizard and walks thru the wizard
        /// </summary>
        private void CreateModelFromDB(AutomationElement wizard, string dbName)
        {
            // Select the 'Generate from Database' option
            SelectModelOption(
                wizard,
                _resourceHelper.GetModelWizardResourceString("GenerateFromDatabaseOption"));

            ClickNextButton(wizard);
            var newConnectionButton = FindElement(
                wizard,
                _resourceHelper.GetModelWizardResourceString("NewDatabaseConnectionBtn"));

            InvokeClick(newConnectionButton);

            HandleConnectionDialog(wizard, dbName);

            // You cant click the next button until, it is done reading from the DB
            // So retry clicking the next button. So that we dont have to keep checking
            // if it is done reading from the DB that was specified
            InvokeOperationWithRetry(() => ClickNextButton(wizard));

            ClickNextButton(wizard);

            CheckAllCheckBoxes(wizard);

            InvokeOperationWithRetry(
                () =>
                    {
                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Finding finish button");
                        var finishButton = FindElement(
                            wizard,
                            _resourceHelper.GetWizardResourceString("ButtonFinishText"));

                        Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Invoking Finish button");
                        InvokeClick(finishButton);
                    });
        }

        // checks all the checkboxes on the "which database objects to select" dialog
        private void CheckAllCheckBoxes(AutomationElement wizard)
        {
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem);
            var retryCount = 10;
            AutomationElementCollection list = null;
            var milisecondsToSleep = 300;
            AutomationElement dbObjects = null;

            while (retryCount-- > 0)
            {
                try
                {
                    dbObjects = FindElement(
                        wizard,
                        _resourceHelper.GetModelWizardResourceString("WhichDatabaseObjectsLabel"));
                }
                catch
                {
                    SystemThread.Sleep(milisecondsToSleep);
                    milisecondsToSleep += 300;
                    break;
                }

                list = dbObjects.FindAll(TreeScope.Descendants, condition);

                if (list == null
                    || list.Count == 0)
                {
                    SystemThread.Sleep(milisecondsToSleep);
                    milisecondsToSleep += 300;
                }
                else
                {
                    retryCount = 0;
                    break;
                }
            }

            if (list == null
                || list.Count == 0)
            {
                throw new InvalidOperationException("Checking all the boxes failed");
            }

            foreach (AutomationElement checkbox in list)
            {
                var togglePattern = (TogglePattern)checkbox.GetCurrentPattern(TogglePattern.Pattern);
                togglePattern.Toggle();
            }
        }

        // Handles the new connection dialog
        private void HandleConnectionDialog(AutomationElement wizard, string dbName)
        {
            var serverNameText = FindTextBox(
                wizard,
                _resourceHelper.GetConnectionDialogResourceString("serverLabel.Text"));
            EnterText(serverNameText, @"(localdb)\v11.0");

            var refreshButton = FindElement(
                wizard,
                _resourceHelper.GetConnectionDialogResourceString("refreshButton.Text"));
            refreshButton.SetFocus();

            var dbNameText = FindTextBox(
                wizard,
                _resourceHelper.GetConnectionDialogResourceString("selectDatabaseRadioButton.Text"));
            EnterText(dbNameText, dbName);

            var okButton = FindElement(
                wizard,
                _resourceHelper.GetConnectionDialogResourceString("acceptButton.Text"));
            InvokeClick(okButton);
        }

        // This method finds the element with 'elementName' in all the descendants of parent element
        private AutomationElement FindElement(AutomationElement parent, string elementName)
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ": In Find Element " + elementName);

            // The loop will be exited either if time runs out or the dialog is found
            var retryCount = numRetry;
            var milisecondsToSleep = 100;
            AutomationElement element = null;
            Exception exceptionCaught = null;

            while (retryCount-- > 0)
            {
                // try to find the wizard                
                var condition = new PropertyCondition(
                    AutomationElement.NameProperty,
                    elementName,
                    PropertyConditionFlags.IgnoreCase);

                try
                {
                    element = parent.FindFirst(TreeScope.Descendants, condition);
                }
                catch (Exception ex)
                {
                    exceptionCaught = ex;
                    Trace.WriteLine(ex);
                }

                if (element != null)
                {
                    return element;
                }

                Trace.WriteLine(DateTime.Now.ToLongTimeString() + ": element not found Sleeping");

                SystemThread.Sleep(milisecondsToSleep);
                milisecondsToSleep += 200;
            }

            throw new InvalidOperationException(
                string.Format("Find element: {0} failed", elementName),
                exceptionCaught);
        }

        private void SelectModelOption(AutomationElement wizard, string modelOption)
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":SelectModelOption");
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":trying to find option list");
            var optionList = FindElement(
                wizard,
                _resourceHelper.GetModelWizardResourceString("StartPage_PromptLabelText"));

            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Option list found, finding " + modelOption);
            var model = FindElement(optionList, modelOption);

            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Model option found");

            var selectionPattern = (SelectionItemPattern)model.GetCurrentPattern(SelectionItemPattern.Pattern);

            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Selecting the option");
            InvokeOperationWithRetry(() => selectionPattern.Select());
        }

        // Finds textbox with given name, find element doesnt work as it finds the label
        // not the text box
        private AutomationElement FindTextBox(AutomationElement parent, string elementName)
        {
            var retryCount = numRetry;
            var milisecondsToSleep = 100;
            AutomationElement element = null;
            Exception exceptionCaught = null;

            var nameCondition = new PropertyCondition(
                AutomationElement.NameProperty,
                elementName,
                PropertyConditionFlags.IgnoreCase);

            var textCondition = new PropertyCondition(
                AutomationElement.ControlTypeProperty,
                ControlType.Edit);

            var andCondition = new AndCondition(nameCondition, textCondition);

            while (retryCount-- > 0)
            {
                try
                {
                    element = parent.FindFirst(TreeScope.Descendants, andCondition);
                }
                catch (Exception ex)
                {
                    exceptionCaught = ex;
                    Trace.WriteLine(ex);
                }

                if (element != null)
                {
                    return element;
                }

                Trace.WriteLine(DateTime.Now.ToLongTimeString() + ": textbox not found Sleeping");
                SystemThread.Sleep(milisecondsToSleep);
                milisecondsToSleep += 200;
            }

            throw new InvalidOperationException(
                string.Format("Find textbox: {0} failed", elementName),
                exceptionCaught);
        }

        // Invokes click on a button
        private void InvokeClick(AutomationElement button)
        {
            Trace.WriteLine(DateTime.Now.ToLongTimeString() + ":Invoking button");
            InvokeOperationWithRetry(
                () =>
                    {
                        var invokePattern = (InvokePattern)button.GetCurrentPattern(InvokePattern.Pattern);
                        if (invokePattern != null)
                        {
                            invokePattern.Invoke();
                        }
                        else
                        {
                            throw new InvalidOperationException("Invoke click failed:Invoke pattern null");
                        }
                    });
        }

        // Invokes click on next button
        private void ClickNextButton(AutomationElement wizard)
        {
            var nextButton = FindElement(
                wizard,
                _resourceHelper.GetWizardResourceString("ButtonNextText"));

            InvokeClick(nextButton);
        }

        // Enters text in the text box
        private void EnterText(AutomationElement textBox, string text)
        {
            InvokeOperationWithRetry(
                () =>
                    {
                        var valuePattern = (ValuePattern)textBox.GetCurrentPattern(ValuePattern.Pattern);
                        if (valuePattern != null)
                        {
                            valuePattern.SetValue(text);
                        }
                        else
                        {
                            throw new InvalidOperationException("Value pattern null");
                        }
                    });
        }

        // Creates a new thred for action and starts it
        private SystemThread ExecuteThreadedAction(Action action, string threadName = "Worker")
        {
            var thread = new SystemThread(
                new ThreadStart(action)) { Name = threadName };
            thread.Start();
            return thread;
        }

        // Invokes a given operation with retry
        private static void InvokeOperationWithRetry(Action operation)
        {
            var retryCount = numRetry;
            var milisecondsToSleep = 100;
            Exception exceptionCaught = null;
            while (retryCount-- > 0)
            {
                try
                {
                    operation();
                    Trace.WriteLine(DateTime.Now.ToLongTimeString() + ": operation successful");
                    return;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    exceptionCaught = ex;
                    Trace.WriteLine(DateTime.Now.ToLongTimeString() + ": operation failed ..... sleeping");
                    SystemThread.Sleep(milisecondsToSleep);
                    milisecondsToSleep += 200;
                }
            }

            throw new InvalidOperationException(
                "Operation failed, exceeded number of retries",
                exceptionCaught);
        }
    }
}
