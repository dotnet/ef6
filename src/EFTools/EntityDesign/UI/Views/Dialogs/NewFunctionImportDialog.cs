// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EntityProperty = Microsoft.Data.Entity.Design.Model.Entity.Property;

// remove this one

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Data.Sql;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio.Data.Services;
    using ComplexType = Microsoft.Data.Entity.Design.Model.Entity.ComplexType;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using Resources = Microsoft.Data.Tools.XmlDesignerBase.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class NewFunctionImportDialog : Form
    {
        #region TESTS

        // For test purposes only!!!
        private static event EventHandler DialogActivatedTestEventStorage;
        private static event EventHandler GetResultColumnsCompletedEventStorage;

        // This should be set only in the test code!!!
        internal static event EventHandler DialogActivatedTestEvent
        {
            add { DialogActivatedTestEventStorage += value; }
            remove { DialogActivatedTestEventStorage -= value; }
        }

        internal static event EventHandler GetResultColumnsCompletedEvent
        {
            add { GetResultColumnsCompletedEventStorage += value; }
            remove { GetResultColumnsCompletedEventStorage -= value; }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // set the FunctionImport name to have the focus if it is enabled (fixes bug 807143)
            if (functionImportNameTextBox.Enabled)
            {
                functionImportNameTextBox.Focus();
            }

            // For test purposes only!!!
            if (DialogActivatedTestEventStorage != null)
            {
                DialogActivatedTestEventStorage(this, EventArgs.Empty);
            }
        }

        #endregion

        // <summary>
        //     Function Import
        // </summary>
        private enum DialogMode
        {
            New, // New function import mode.
            FullEdit // Edit function import; Users can edit any fields.
        }

        private const int NotSupportedDataType = -1;

        private bool _needsValidation;
        private bool _isWorkerRunning;
        private FeatureState _complexTypeFeatureState;
        private FeatureState _composableFunctionImportFeatureState;
        private FeatureState _getColumnInformationFeatureState;
        private readonly DialogMode _mode;
        private IDataSchemaProcedure _lastGeneratedStoredProc;
        private Cursor _oldCursor;
        private readonly ConceptualEntityContainer _container;
        private ConnectionManager.ConnectionString _connectionString;
        private Project _currentProject;
        private bool _updateSelectedComplexType;

        private ToolTip _featureDisabledToolTip;
        private Control _controlWithToolTipShown;
        private readonly FunctionImport _editedFunctionImport;
        private IVsDataConnection _openedDbConnection;
        private readonly ICollection<Function> _functions; // list of functions originally passed to constructor
        private string[] _sortedEdmPrimitiveTypes;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal NewFunctionImportDialog(
            Function baseFunction,
            string functionImportName,
            ICollection<Function> functions,
            IEnumerable<ComplexType> complexTypes,
            IEnumerable<EntityType> entityTypes,
            ConceptualEntityContainer container,
            object selectedElement)
        {
            // The dialog 3 mode:
            // - New function import: to create a new function import
            // - Full edit function import: the dialog is launched from model browser; all fields are editable
            _mode = DialogMode.New;
            _editedFunctionImport = container.FunctionImports().Where(x => x.LocalName.Value == functionImportName).FirstOrDefault();
            if (_editedFunctionImport != null)
            {
                _mode = DialogMode.FullEdit;
            }
            _functions = functions;
            _lastGeneratedStoredProc = null;
            _container = container;
            _updateSelectedComplexType = false;
            InitializeSupportedFeatures();
            InitializeComponent();
            InitializeDialogFont();

            // set tooltip on functionImportComposableCheckBox if not supported
            if (false == _composableFunctionImportFeatureState.IsEnabled())
            {
                var isComposableToolTipMsg = string.Format(
                    CultureInfo.CurrentCulture, DialogsResource.NewFunctionImportDialog_IsComposableTooltipText,
                    EntityFrameworkVersion.Version2);
                var isComposableToolTip = new ToolTip();
                isComposableToolTip.ShowAlways = true; // show even if control inactive
                isComposableToolTip.SetToolTip(functionImportComposableCheckBox, isComposableToolTipMsg);
            }

            // once the components are initialized, check the functionImportComposableCheckBox if appropriate
            if (_composableFunctionImportFeatureState.IsEnabled())
            {
                if (DialogMode.FullEdit == _mode)
                {
                    functionImportComposableCheckBox.Checked = (BoolOrNone.TrueValue == _editedFunctionImport.IsComposable.Value);
                }
                else
                {
                    Debug.Assert(_mode == DialogMode.New, "Unexpected mode");

                    functionImportComposableCheckBox.Checked = baseFunction != null && baseFunction.IsComposable.Value;
                }
            }

            // Hide the Update button/GetColumnInformation frame if this functionality isn't allowed
            if (!_getColumnInformationFeatureState.IsVisible())
            {
                updateComplexTypeButton.Visible = false;
                returnTypeShapeGroup.Visible = false;
                if (ClientSize.Height > returnTypeShapeGroup.Height)
                {
                    var newSize = new Size(Size.Width, Size.Height - returnTypeShapeGroup.Height);
                    MinimumSize = newSize;
                    Size = newSize;
                }
            }

            PopulateComboBoxes(complexTypes, entityTypes, functions);
            UpdateStateComboBoxes(selectedElement, baseFunction, functionImportName);
            SetComplexTypeTooltip();
            CheckOkButtonEnabled();
            UpdateReturnTypeComboBoxesState();
            UpdateReturnTypeInfoAreaState();
            SetCreateNewComplexTypeButtonProperties();

            if (components == null)
            {
                components = new Container();
            }
            // Since Visual Studio has already defined Dispose method in the generated file(designer.cs),
            // we instantiates Disposer class that calls our custom dispose method when Form is disposed.
            components.Add(new Disposer(OnDispose));

            cancelButton.BackColor = SystemColors.Control;
            cancelButton.ForeColor = SystemColors.ControlText;
            okButton.BackColor = SystemColors.Control;
            okButton.ForeColor = SystemColors.ControlText;
            getColumnInformationButton.BackColor = SystemColors.Control;
            getColumnInformationButton.ForeColor = SystemColors.ControlText;
            createNewComplexTypeButton.BackColor = SystemColors.Control;
            createNewComplexTypeButton.ForeColor = SystemColors.ControlText;
            updateComplexTypeButton.BackColor = SystemColors.Control;
            updateComplexTypeButton.ForeColor = SystemColors.ControlText;
        }

        private void OnDispose(bool disposing)
        {
            Debug.Assert(
                _openedDbConnection == null,
                "There is still open DBConnection when NewFunctionImportDialog is closing.");

            if (_openedDbConnection != null)
            {
                _openedDbConnection.Close();
                _openedDbConnection = null;
            }
        }

        #region Overridden Methods

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_needsValidation)
            {
                // ReturnType should never be Null at this point.
                Debug.Assert(ReturnType != null, "ReturnType is null.");

                _needsValidation = false;
                // Function import validation
                if (!EscherAttributeContentValidator.IsValidCsdlFunctionImportName(FunctionImportName))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewFunctionImportDialog_InvalidFunctionImportNameMsg);
                    e.Cancel = true;
                    functionImportNameTextBox.Focus();
                }
                else
                {
                    string msg;
                    if (_mode == DialogMode.New
                        && !ModelHelper.IsUniqueName(typeof(FunctionImport), _container, FunctionImportName, false, out msg))
                    {
                        VsUtils.ShowErrorDialog(DialogsResource.NewFunctionImportDialog_EnsureUniqueNameMsg);
                        e.Cancel = true;
                        functionImportNameTextBox.Focus();
                        return;
                    }
                }
                // If a new complex type will be created, we need to validate the name that user entered.
                if (complexTypeReturnButton.Checked
                    && complexTypeReturnComboBox.SelectedItem == null
                    && !EscherAttributeContentValidator.IsValidCsdlComplexTypeName(complexTypeReturnComboBox.Text))
                {
                    var errorMessage = String.Format(
                        CultureInfo.CurrentCulture, DialogsResource.NewFunctionImportDialog_NotValidComplexTypeName,
                        complexTypeReturnComboBox.Text);
                    VsUtils.ShowErrorDialog(errorMessage);
                    e.Cancel = true;
                    complexTypeReturnComboBox.Focus();
                    return;
                }
            }

            // If the return type is not complex type, we need to clear out the Schema property to avoid a new complex type being created.
            if (!complexTypeReturnButton.Checked)
            {
                Schema = null;
            }
                // If the return type is complex type, but the user doesn't want to update the selected complex type than we need to clear the schema property.
            else if (complexTypeReturnButton.Checked
                     && complexTypeReturnComboBox.SelectedItem != null
                     && !_updateSelectedComplexType)
            {
                Schema = null;
            }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        #endregion

        #region Internal Methods

        // <summary>
        //     Display the tooltip if the user hover the mouse on the complex return type controls.
        // </summary>
        private void ReturnTypeArea_OnMouseMove(Object sender, MouseEventArgs e)
        {
            var parent = sender as Control;
            if (parent == null)
            {
                return;
            }

            var ctrl = parent.GetChildAtPoint(e.Location);

            if (ctrl == complexTypeReturnButton
                || ctrl == complexTypeReturnComboBox
                || ctrl == createNewComplexTypeButton
                || ctrl == updateComplexTypeButton)
            {
                // if the user hover on control where tooltip is shown, just return.
                if (ctrl == _controlWithToolTipShown)
                {
                    return;
                }

                var tipString = _featureDisabledToolTip.GetToolTip(ctrl);
                // calculate the screen coordinate of the mouse
                _featureDisabledToolTip.Show(tipString, ctrl, 2, ctrl.Height + 2);
                _controlWithToolTipShown = ctrl;
            }
            else if (_controlWithToolTipShown != null)
            {
                _featureDisabledToolTip.Hide(_controlWithToolTipShown);
                _controlWithToolTipShown = null;
            }
        }

        private void ReturnTypeArea_OnMouseLeave(object sender, EventArgs e)
        {
            if (_controlWithToolTipShown != null)
            {
                _featureDisabledToolTip.Hide(_controlWithToolTipShown);
            }
            _controlWithToolTipShown = null;
        }

        internal Function Function
        {
            get { return storedProcComboBox.SelectedItem as Function; }
        }

        internal string FunctionImportName
        {
            get { return functionImportNameTextBox.Text; }
        }

        internal object ReturnType
        {
            get
            {
                if (emptyReturnTypeButton.Checked)
                {
                    return Resources.NoneDisplayValueUsedForUX;
                }
                else if (scalarTypeReturnButton.Checked)
                {
                    return scalarTypeReturnComboBox.SelectedItem;
                }
                else if (complexTypeReturnButton.Checked)
                {
                    // If SelectedItem is null, check the text value. This indicates that the user wants to create a new complex type.
                    if (complexTypeReturnComboBox.SelectedItem != null)
                    {
                        return complexTypeReturnComboBox.SelectedItem;
                    }
                    else if (!String.IsNullOrEmpty(complexTypeReturnComboBox.Text))
                    {
                        return complexTypeReturnComboBox.Text;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (entityTypeReturnButton.Checked)
                {
                    return entityTypeReturnComboBox.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        internal bool IsComposable
        {
            get { return functionImportComposableCheckBox.Checked; }
        }

        internal IDataSchemaProcedure Schema
        {
            get { return _lastGeneratedStoredProc; }
            private set
            {
                _lastGeneratedStoredProc = value;
                SetCreateNewComplexTypeButtonProperties();
            }
        }

        // <summary>
        //     Determine whether a stored procedure returns columns schemas are available or not.
        //     Since we cache the columns schema information in Schema property, we should be able to calculate the value of this property from Schema property.
        // </summary>
        private bool IsResultTypeAvailable
        {
            get { return (Schema != null && Schema.Columns != null && Schema.Columns.Count > 0); }
        }

        #endregion

        #region Event handler

        private void storedProcComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Schema = null;
            _updateSelectedComplexType = false;
            UpdateReturnTypeComboBoxesState();
            UpdateReturnTypeInfoAreaState();
            CheckOkButtonEnabled();
            // if the index has changed, we need to make the complex-type combo box to be readonly-mode.
            SetComplexTypeReturnComboBoxStyle(false);
        }

        private void scalarTypeReturnComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReturnTypeComboBoxesState();
            CheckOkButtonEnabled();
        }

        private void entityTypeReturnComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReturnTypeComboBoxesState();
            CheckOkButtonEnabled();
        }

        private void complexTypeReturnComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If the user changed the selected complex type, instead of resetting the return type view list area
            // which makes the user have to click on "get column information" button to see the sproc return columns again, we
            // automatically refresh the area to show sproc return columns information.
            if (_updateSelectedComplexType)
            {
                // reset the diff mode flag since we are no longer displaying the diff between sproc and return complex type columns
                _updateSelectedComplexType = false;

                // At this point, both Schema and columns should not be null.
                Debug.Assert(
                    Schema != null && Schema.Columns != null,
                    "Either schema or schema's columns is null when the dialog is in diff mode.");

                if (Schema != null
                    && Schema.Columns != null)
                {
                    if (Schema.Columns.Count > 0)
                    {
                        UpdateComplexTypeList(Schema.Columns);
                        UpdateReturnTypeInfoAreaState();
                    }
                    else
                    {
                        // display no columns returned by the stored procedure.
                        detectErrorLabel.Text = DialogsResource.NewFunctionImportDialog_NoColumnsReturned;
                        returnTypeShapeTabControl.SelectedTab = detectErrorTabPage;
                        UpdateReturnTypeListState(false);
                    }
                }
            }

            UpdateReturnTypeComboBoxesState();
            CheckOkButtonEnabled();
            // if the index has changed, we need to make the complex-type combo box to be not writable.
            SetComplexTypeReturnComboBoxStyle(false);
        }

        private void functionImportNameTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckOkButtonEnabled();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _needsValidation = true;
        }

        private void functionImportComposableCheckBox_Click(object sender, EventArgs e)
        {
            // update which functions are displayed in drop-down
            PopulateStoreProcedureList(_functions);
            CheckOkButtonEnabled();
        }

        private void returnTypeButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateReturnTypeComboBoxesState();
            CheckOkButtonEnabled();
        }

        // <summary>
        //     If we don't override the Up and Down arrow behavior then by default they navigate to the next Tab Stop
        //     (which is the corresponding combo-box) and after that arrow keys only select the next/previous combo-box entry.
        //     We are overriding this behavior so that the user can use the arrow keys to navigate to the next/previous
        //     radio button instead. This addresses Accessibility bug 807144.
        // </summary>
        private void returnTypeButton_ArrowPressed(object sender, Keys key)
        {
            if (emptyReturnTypeButton == sender)
            {
                switch (key)
                {
                    case Keys.Up:
                        storedProcComboBox.Focus(); // navigates to previous logical control above the Empty Return Type radio button
                        return;

                    case Keys.Down:
                        scalarTypeReturnButton.Checked = true;
                        scalarTypeReturnButton.Focus();
                        return;

                    default:
                        return;
                }
            }

            if (scalarTypeReturnButton == sender)
            {
                switch (key)
                {
                    case Keys.Up:
                        emptyReturnTypeButton.Checked = true;
                        emptyReturnTypeButton.Focus();
                        return;

                    case Keys.Down:
                        complexTypeReturnButton.Checked = true;
                        complexTypeReturnButton.Focus();
                        return;

                    default:
                        return;
                }
            }

            if (complexTypeReturnButton == sender)
            {
                switch (key)
                {
                    case Keys.Up:
                        scalarTypeReturnButton.Checked = true;
                        scalarTypeReturnButton.Focus();
                        return;

                    case Keys.Down:
                        entityTypeReturnButton.Checked = true;
                        entityTypeReturnButton.Focus();
                        return;

                    default:
                        return;
                }
            }

            if (entityTypeReturnButton == sender)
            {
                switch (key)
                {
                    case Keys.Up:
                        complexTypeReturnButton.Checked = true;
                        complexTypeReturnButton.Focus();
                        return;

                    case Keys.Down:
                        // navigates to next logical control below the Entity Type Return Type radio button
                        getColumnInformationButton.Focus();
                        return;

                    default:
                        return;
                }
            }
        }

        private void getColumnInformationButton_Click(object sender, EventArgs e)
        {
            _updateSelectedComplexType = false;
            LaunchBackgroundProcessToRetrieveColumnsSchema();
        }

        // <summary>
        //     Create New Complex Type button click event handler
        // </summary>
        private void createNewComplexTypeButton_Click(object sender, EventArgs args)
        {
            // need to reset the return type list view to not show the diff between the columns schema and function import return type properties.
            if (_updateSelectedComplexType)
            {
                _updateSelectedComplexType = false;
                // Need to update the return type view list.
                UpdateComplexTypeList(Schema.Columns);
            }
            updateComplexTypeButton.Enabled = false;

            // Clears out any selected item in complex type combo box
            complexTypeReturnComboBox.SelectedItem = null;

            // Select complex type radio button and set the font to bold for visual effect.
            complexTypeReturnButton.Checked = true;
            complexTypeReturnButton.Font = new Font(complexTypeReturnButton.Font, complexTypeReturnButton.Font.Style | FontStyle.Bold);

            // Set the complex combo box style to be writable so the user can enter a new complex type name.
            SetComplexTypeReturnComboBoxStyle(true);
            complexTypeReturnComboBox.Focus();

            // Calculate the new complex type name.
            var complexTypeBaseName = (string.IsNullOrEmpty(FunctionImportName) ? Function.LocalName.Value : FunctionImportName);
            var complexTypeName = GetComplexTypeName(complexTypeBaseName);
            complexTypeReturnComboBox.Text = complexTypeName;
            CheckOkButtonEnabled();
        }

        // <summary>
        //     Update complex type button event handler.
        // </summary>
        private void updateComplexTypeButton_Click(object sender, EventArgs args)
        {
            // Turn on the update selected complex type flag.
            _updateSelectedComplexType = true;
            // If schema is not null, then just update the return type list view.
            if (Schema != null)
            {
                UpdateComplexTypeList(Schema.Columns);
            }
            else
            {
                // Get column schema information from the database.
                LaunchBackgroundProcessToRetrieveColumnsSchema();
            }
        }

        #endregion

        #region Private Methods

        private void CheckOkButtonEnabled()
        {
            if (Function == null
                || String.IsNullOrEmpty(FunctionImportName)
                || ReturnType == null
                || _isWorkerRunning)
            {
                okButton.Enabled = false;
            }
            else
            {
                okButton.Enabled = true;
            }
        }

        private string GetComplexTypeName(string baseName)
        {
            Debug.Assert(!string.IsNullOrEmpty(baseName), "baseName should not be null or empty");

            // Note: the Complex Type name format should not be localized (see TFS #754869)
            var complexTypeName = String.Format(CultureInfo.CurrentCulture, "{0}_Result", baseName);
            var model = _container.Artifact.ConceptualModel();
            complexTypeName = ModelHelper.GetUniqueName(typeof(ComplexType), model, complexTypeName);

            return complexTypeName;
        }

        // <summary>
        //     Set scalar-return-type and entity-return-type combo boxes not enabled/enabled state.
        // </summary>
        private void UpdateReturnTypeComboBoxesState()
        {
            scalarTypeReturnComboBox.Enabled = scalarTypeReturnButton.Checked;
            // Only enable this control, if the radio button is selected and function import returning complex type feature is supported.
            complexTypeReturnComboBox.Enabled = complexTypeReturnButton.Checked && _complexTypeFeatureState.IsEnabled();
            // Only enable this control if complexTypeReturnComboBox is enabled, the selected item is not null, and stored procedure combo box selected item is not null
            updateComplexTypeButton.Enabled = complexTypeReturnComboBox.Enabled && (complexTypeReturnComboBox.SelectedItem != null)
                                              && (storedProcComboBox.SelectedItem != null);
            entityTypeReturnComboBox.Enabled = entityTypeReturnButton.Checked;

            emptyReturnTypeButton.Font = functionImportNameTextBox.Font;
            scalarTypeReturnButton.Font = functionImportNameTextBox.Font;
            complexTypeReturnButton.Font = functionImportNameTextBox.Font;
            entityTypeReturnButton.Font = functionImportNameTextBox.Font;

            complexTypeReturnButton.Enabled = _complexTypeFeatureState.IsEnabled();
        }

        private void UpdateReturnTypeListState(bool isEnabled)
        {
            returnTypeShapeListView.Visible = isEnabled;
            returnTypeShapeTabControl.Visible = !isEnabled;
        }

        private void SetDoWorkState(bool isRunning)
        {
            if (isRunning)
            {
                // cursor
                _oldCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                // state
                functionImportNameTextBox.Enabled = false;
                functionImportComposableCheckBox.Enabled = false;
                storedProcComboBox.Enabled = false;
                scalarTypeReturnComboBox.Enabled = false;
                complexTypeReturnComboBox.Enabled = false;
                entityTypeReturnComboBox.Enabled = false;
                emptyReturnTypeButton.Enabled = false;
                scalarTypeReturnButton.Enabled = false;
                complexTypeReturnButton.Enabled = false;
                entityTypeReturnButton.Enabled = false;
                getColumnInformationButton.Enabled = false;
                createNewComplexTypeButton.Enabled = false;
                returnTypeShapeTabControl.SelectedTab = detectingTabPage;
            }
            else
            {
                // cursor
                Cursor = _oldCursor;

                // state
                // we enable name text box, composable checkbox and sproc combo box if the mode is not return-type-edit only
                // and the IsComposable setting is supported by the artifact version.
                functionImportNameTextBox.Enabled = true;
                functionImportComposableCheckBox.Enabled = _composableFunctionImportFeatureState.IsEnabled();
                storedProcComboBox.Enabled = true;
                emptyReturnTypeButton.Enabled = true;
                scalarTypeReturnButton.Enabled = true;
                complexTypeReturnButton.Enabled = true;
                entityTypeReturnButton.Enabled = true;
                getColumnInformationButton.Enabled = true;
                returnTypeShapeTabControl.SelectedTab = detectTabPage;
                UpdateReturnTypeComboBoxesState();
            }

            CheckOkButtonEnabled();
        }

        private bool UpdateComplexTypeList(IList<IDataSchemaColumn> columns)
        {
            // Clear the return type shape list view.
            returnTypeShapeListView.Items.Clear();

            // Return type list view has 2 modes:
            //    - Normal mode: we only show the stored procedure return columns information
            //    - Diff Mode: we display the diff between the selected complex type properties and sproc return columns.
            if (!_updateSelectedComplexType)
            {
                // we need to remove action column because it is not needed in this mode.
                if (returnTypeShapeListView.Columns.Contains(columnAction))
                {
                    columnAction.DisplayIndex = 1;
                    returnTypeShapeListView.Columns.Remove(columnAction);
                }

                // iterate through the column and add new item in the list view.
                foreach (var column in columns)
                {
                    returnTypeShapeListView.Items.Add(CreateReturnTypeShapeListItem(column, null, null));
                }
            }
            else
            {
                // we need to add action column if the column is missing.
                if (!returnTypeShapeListView.Columns.Contains(columnAction))
                {
                    returnTypeShapeListView.Columns.Insert(0, columnAction);
                }

                // Update complex type mode
                // Create a sorted list for both schema columns and complex type properties.
                var sortedColumns = columns.OrderBy(col => col.Name).ToList();
                // ad this point, the selected item must not be null.
                var selectedComplexType = complexTypeReturnComboBox.SelectedItem as ComplexType;
                Debug.Assert(selectedComplexType != null, "There is no selected complex type.");

                if (selectedComplexType != null)
                {
                    var sortedProperties = selectedComplexType.Properties().OrderBy(
                        entityProperty =>
                        EdmUtils.GetFunctionImportResultColumnName(_editedFunctionImport, entityProperty)).ToList();

                    // now iterate both list and display the diff.
                    var propertyIndex = 0;
                    EntityProperty prop;
                    var propertyName = String.Empty;

                    var storageModel = _container.Artifact.StorageModel();
                    for (var i = 0; i < sortedColumns.Count; i++)
                    {
                        var col = sortedColumns[i];
                        prop = null;
                        // Add delete rows for all properties whose name less than column name.
                        while (propertyIndex < sortedProperties.Count)
                        {
                            prop = sortedProperties[propertyIndex];
                            propertyName = EdmUtils.GetFunctionImportResultColumnName(_editedFunctionImport, prop);
                            if (String.Compare(propertyName, col.Name, StringComparison.CurrentCulture) >= 0)
                            {
                                break;
                            }
                            returnTypeShapeListView.Items.Add(
                                CreateReturnTypeShapeListItem(
                                    null, prop, DialogsResource.NewFunctionImportDialog_ReturnTypeListDeleteAction));
                            propertyIndex++;
                        }

                        if (prop == null)
                        {
                            // Insert Add row for the column.
                            returnTypeShapeListView.Items.Add(
                                CreateReturnTypeShapeListItem(col, null, DialogsResource.NewFunctionImportDialog_ReturnTypeListAddAction));
                        }
                            // both complex type property and column has the same name.
                        else if (prop != null
                                 && String.Compare(col.Name, propertyName, StringComparison.CurrentCulture) == 0)
                        {
                            // compare the actual objects - no action if they are the same.
                            if (ModelHelper.IsPropertyEquivalentToSchemaColumn(storageModel, prop, col))
                            {
                                returnTypeShapeListView.Items.Add(
                                    CreateReturnTypeShapeListItem(col, null, DialogsResource.NewFunctionImportDialog_ReturnTypeListNoAction));
                            }
                                // else add update row.
                            else
                            {
                                returnTypeShapeListView.Items.Add(
                                    CreateReturnTypeShapeListItem(
                                        col, prop, DialogsResource.NewFunctionImportDialog_ReturnTypeListUpdateAction));
                            }
                            propertyIndex++;
                        }
                            // Insert Add row for the column
                        else
                        {
                            returnTypeShapeListView.Items.Add(
                                CreateReturnTypeShapeListItem(col, null, DialogsResource.NewFunctionImportDialog_ReturnTypeListAddAction));
                        }
                    }
                    // Display the remaining items in complex types properties as delete rows.
                    while (propertyIndex < sortedProperties.Count)
                    {
                        prop = sortedProperties[propertyIndex];
                        returnTypeShapeListView.Items.Add(
                            CreateReturnTypeShapeListItem(null, prop, DialogsResource.NewFunctionImportDialog_ReturnTypeListDeleteAction));
                        propertyIndex++;
                    }
                }
            }

            return (returnTypeShapeListView.Items.Count > 0);
        }

        // <summary>
        //     Helper function to create an item row in return type list view.
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private ListViewItem CreateReturnTypeShapeListItem(IDataSchemaColumn column, EntityProperty property, string action)
        {
            var storageModel = _container.Artifact.StorageModel();
            Debug.Assert(storageModel != null, "Storage model is null");
            Debug.Assert(column != null || property != null, "Both data schema column and complex type property is null");
            ListViewItem item = null;
            var isUpdateMode = false;

            if (column != null
                || property != null)
            {
                PrimitiveType columnPrimitiveType = null;
                // Retrieve column primitive type
                if (column != null
                    && column.ProviderDataType != NotSupportedDataType)
                {
                    columnPrimitiveType = ModelHelper.GetPrimitiveType(storageModel, column.NativeDataType, column.ProviderDataType);
                }

                // if we could not find primitive type for the column, that means the column type is not supported.
                if (column != null
                    && columnPrimitiveType == null
                    && !String.IsNullOrEmpty(action))
                {
                    // overwrite the action column.
                    action = DialogsResource.NewFunctionImportDialog_ReturnTypeListNoAction;
                }

                // if action is null or string empty, the first column is the column name.
                if (String.IsNullOrEmpty(action))
                {
                    item = new ListViewItem(column != null ? column.Name : property.LocalName.Value);
                }
                else
                {
                    item = new ListViewItem(action);
                    item.SubItems.Add(column != null ? column.Name : property.LocalName.Value);
                    isUpdateMode = action == DialogsResource.NewFunctionImportDialog_ReturnTypeListUpdateAction;
                }

                PrimitiveType propertyPrimitiveType = null;
                if (property != null)
                {
                    propertyPrimitiveType = ModelHelper.GetPrimitiveTypeFromString(property.TypeName);
                }

                // Edm Type Name
                // if we could not find primitive type for the column, that means the column type is not supported.
                if (column != null
                    && columnPrimitiveType == null)
                {
                    item.SubItems.Add(DialogsResource.NewFunctionImportDialog_NotSupportedColumnType);
                }
                else
                {
                    item.SubItems.Add(
                        GetReturnTypeListViewCellText(
                            isUpdateMode
                            , (propertyPrimitiveType != null ? propertyPrimitiveType.GetEdmPrimitiveType().Name : null)
                            , (columnPrimitiveType != null ? columnPrimitiveType.GetEdmPrimitiveType().Name : null)));
                }

                // DB Type for the CSDL type property should always be null.
                item.SubItems.Add(
                    GetReturnTypeListViewCellText(
                        false
                        , null
                        , (columnPrimitiveType != null ? columnPrimitiveType.Name : null)));

                // Nullable
                item.SubItems.Add(
                    GetReturnTypeListViewCellText(
                        isUpdateMode
                        , (property != null ? GetColumnNullableFacetText(property.Nullable) : null)
                        , (column != null ? GetColumnNullableFacetText(column.IsNullable) : null)));

                // Size
                string propertySize = null;
                if (property != null
                    && ModelHelper.IsValidModelFacet(property.TypeName, EntityProperty.AttributeMaxLength)
                    && property.MaxLength != null
                    && property.MaxLength.Value != null)
                {
                    propertySize = property.MaxLength.Value.ToString();
                }
                item.SubItems.Add(
                    GetReturnTypeListViewCellText(
                        isUpdateMode, propertySize
                        , (column != null ? ModelHelper.GetMaxLengthFacetText(column.Size) : null)));

                // Precision
                string propertyPrecision = null;
                if (property != null
                    && ModelHelper.IsValidModelFacet(property.TypeName, EntityProperty.AttributePrecision)
                    && property.Precision != null)
                {
                    propertyPrecision = property.Precision.Value.ToString();
                }
                item.SubItems.Add(
                    GetReturnTypeListViewCellText(
                        isUpdateMode, propertyPrecision
                        , (column != null && column.Precision != null ? column.Precision.ToString() : null)));

                // Scale
                string propertyScale = null;
                if (property != null
                    && ModelHelper.IsValidModelFacet(property.TypeName, EntityProperty.AttributeScale)
                    && property.Scale != null)
                {
                    propertyScale = property.Scale.Value.ToString();
                }
                item.SubItems.Add(
                    GetReturnTypeListViewCellText(
                        isUpdateMode, propertyScale
                        , (column != null && column.Scale != null ? column.Scale.ToString() : null)));
            }
            return item;
        }

        private static string GetColumnNullableFacetText(DefaultableValueBoolOrNone nullableDefaultableValue)
        {
            var primitiveValue = nullableDefaultableValue.Value.PrimitiveValue;
            if (null != nullableDefaultableValue.Value.StringValue)
            {
                // isNullable is set to 'None' - so use default
                StringOrPrimitive<bool> defaultValue = nullableDefaultableValue.DefaultValue;
                if (null == defaultValue.StringValue)
                {
                    primitiveValue = defaultValue.PrimitiveValue;
                }
                else
                {
                    // even default is set to 'None'
                    return null;
                }
            }

            if (primitiveValue)
            {
                return Condition.IsNullConstant;
            }
            else
            {
                return Condition.IsNotNullConstant;
            }
        }

        private static string GetColumnNullableFacetText(bool isNullable)
        {
            if (isNullable)
            {
                return Condition.IsNullConstant;
            }
            else
            {
                return Condition.IsNotNullConstant;
            }
        }

        // <summary>
        //     Helper method to determine what should be displayed in the return type view cell.
        //     if both source and target text are not null, we display: "source->target".
        // </summary>
        private static string GetReturnTypeListViewCellText(bool isUpdateMode, string sourceText, string targetText)
        {
            if (String.IsNullOrEmpty(sourceText)
                && String.IsNullOrEmpty(targetText))
            {
                return String.Empty;
            }
            else if (isUpdateMode)
            {
                // if both strings are not null and different, we need to display in the following format (string1)->string2
                if (String.Compare(sourceText, targetText, StringComparison.CurrentCulture) != 0)
                {
                    return String.Format(
                        CultureInfo.CurrentCulture
                        , DialogsResource.NewFunctionImportDialog_ReturnTypeListViewItemChange, sourceText, targetText);
                }
                else
                {
                    // return either one of them
                    return targetText;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetText))
                {
                    return targetText;
                }
                else if (!string.IsNullOrEmpty(sourceText))
                {
                    return sourceText;
                }
            }
            return String.Empty;
        }

        private void PopulateComboBoxes(
            IEnumerable<ComplexType> complexTypes,
            IEnumerable<EntityType> entityTypes,
            ICollection<Function> functions)
        {
            // Scalar types - first add primitive types
            scalarTypeReturnComboBox.Items.AddRange(_sortedEdmPrimitiveTypes);

            // Now add complex types, sorted by name
            if (complexTypes != null)
            {
                var complexTypesArray = complexTypes.ToArray();
                Array.Sort(complexTypesArray, new EFNameableItemComparer());
                complexTypeReturnComboBox.Items.AddRange(complexTypesArray);
            }

            // Now add entity types, sorted by name
            if (entityTypes != null)
            {
                var entityTypesArray = entityTypes.ToArray();
                Array.Sort(entityTypesArray, new EFNameableItemComparer());
                entityTypeReturnComboBox.Items.AddRange(entityTypesArray);
            }

            // Store Procedure List
            PopulateStoreProcedureList(functions);
        }

        private void UpdateReturnTypeInfoAreaState()
        {
            TabPage page;

            getColumnInformationButton.Enabled = false;

            if (CurrentProject == null
                || ConnectionString == null)
            {
                page = connectionTabPage;
            }
                // If function import returning complex type is not supported and we are detecting 
                // sproc returning more than 1 column.
            else if (!_complexTypeFeatureState.IsEnabled()
                     && Schema != null
                     && Schema.Columns.Count > 1)
            {
                page = detectErrorTabPage;
            }
            else if (storedProcComboBox.SelectedItem != null)
            {
                getColumnInformationButton.Enabled = true;
                page = detectTabPage;
            }
            else
            {
                page = selectTabPage;
            }

            UpdateReturnTypeListState(IsResultTypeAvailable);
            returnTypeShapeTabControl.SelectedTab = page;
        }

        private void UpdateStateComboBoxes(object selectedElement, Function baseFunction, string functionImportName)
        {
            // This code was refactored from EditFunctionImportDialog.
            // If selectedEntityType is not null, selects the EntityType radio button and selects the entity type from the combo box.
            var selectedElementAsString = selectedElement as string;
            var selectedElementAsComplexType = selectedElement as ComplexType;
            var selectedElementAsEntityType = selectedElement as EntityType;
            if (null != selectedElementAsString
                && Resources.NoneDisplayValueUsedForUX != selectedElementAsString)
            {
                // primitive type
                if (scalarTypeReturnComboBox.Items.Contains(selectedElementAsString))
                {
                    scalarTypeReturnComboBox.SelectedItem = selectedElementAsString;
                    scalarTypeReturnButton.Checked = true;
                }
                else
                {
                    Debug.Fail(
                        "Selected Element " + selectedElementAsString
                        + " represents a primitive type but is not in the primitive types drop-down");
                    emptyReturnTypeButton.Checked = true;
                }
            }
            else if (null != selectedElementAsComplexType)
            {
                // ComplexType
                if (complexTypeReturnComboBox.Items.Contains(selectedElementAsComplexType))
                {
                    complexTypeReturnComboBox.SelectedItem = selectedElementAsComplexType;
                    complexTypeReturnButton.Checked = true;
                }
                else
                {
                    Debug.Fail(
                        "Selected Element " + selectedElementAsComplexType.ToPrettyString()
                        + " is a ComplexType but is not in ComplexType drop-down");
                    emptyReturnTypeButton.Checked = true;
                }
            }
            else if (null != selectedElementAsEntityType)
            {
                // EntityType
                if (entityTypeReturnComboBox.Items.Contains(selectedElementAsEntityType))
                {
                    entityTypeReturnComboBox.SelectedItem = selectedElementAsEntityType;
                    entityTypeReturnButton.Checked = true;
                }
                else
                {
                    Debug.Fail(
                        "Selected Element " + selectedElementAsEntityType.ToPrettyString()
                        + " is an EntityType but is not in EntityType drop-down");
                    emptyReturnTypeButton.Checked = true;
                }
            }
            else
            {
                emptyReturnTypeButton.Checked = true;
            }

            if (false == string.IsNullOrEmpty(functionImportName))
            {
                functionImportNameTextBox.Text = functionImportName;
            }

            // initialize predefined function
            if (baseFunction != null)
            {
                storedProcComboBox.SelectedItem = baseFunction;
                if (string.IsNullOrEmpty(functionImportNameTextBox.Text)
                    && baseFunction.LocalName != null
                    && false == string.IsNullOrEmpty(baseFunction.LocalName.Value))
                {
                    // here we're auto-assigning a FunctionImport name based on the Function, but
                    // there may be an existing FunctionImport with name baseFunction.LocalName.Value
                    // which is not mapped to that Function so need to uniquify name
                    functionImportNameTextBox.Text = ModelHelper.GetUniqueName(
                        typeof(FunctionImport), _container, baseFunction.LocalName.Value);
                }
            }

            // Disallow the user to change the function import composable checkbox
            // if the feature is not supported
            functionImportComposableCheckBox.Enabled = _composableFunctionImportFeatureState.IsEnabled();
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            IDataSchemaProcedure storedProc = null;
            _openedDbConnection = Connection;
            if (_openedDbConnection != null)
            {
                var server = new DataSchemaServer(_openedDbConnection);
                var function = (Function)e.Argument;
                storedProc = server.GetProcedureOrFunction(function.DatabaseSchemaName, function.DatabaseFunctionName);
            }
            e.Result = storedProc;
        }

        private void OnWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _isWorkerRunning = false;

            SetDoWorkState(false);

            try
            {
                if (e.Error != null)
                {
                    if (CriticalException.IsCriticalException(e.Error))
                    {
                        throw e.Error;
                    }
                    var errMsgWithInnerExceptions = VsUtils.ConstructInnerExceptionErrorMessage(e.Error);
                    var errMsg = string.Format(
                        CultureInfo.CurrentCulture, DialogsResource.NewFunctionImportDialog_GetColumnInfoException
                        , e.Error.GetType().FullName, errMsgWithInnerExceptions);
                    detectErrorLabel.Text = errMsg;
                    returnTypeShapeTabControl.SelectedTab = detectErrorTabPage;
                }
                else if (!e.Cancelled)
                {
                    var schemaProcedure = e.Result as IDataSchemaProcedure;

                    if (schemaProcedure == null)
                    {
                        Debug.Assert(
                            Function != null,
                            "this.Function should not be null. If it is null then 'Get Columns Information' button should be disabled which should stop you getting to here.");
                        var sprocName = (Function == null ? string.Empty : (DatabaseObject.CreateFromFunction(Function).ToString()));
                        detectErrorLabel.Text = string.Format(
                            CultureInfo.CurrentCulture, DialogsResource.NewFunctionImportDialog_CouldNotFindStoredProcedure, sprocName);
                        returnTypeShapeTabControl.SelectedTab = detectErrorTabPage;
                    }
                    else
                    {
                        // regardless of whether it's a stored proc or a function...
                        if (schemaProcedure.Columns != null)
                        {
                            if (UpdateComplexTypeList(schemaProcedure.Columns))
                            {
                                Schema = schemaProcedure;
                                UpdateReturnTypeListState(IsResultTypeAvailable);
                                returnTypeShapeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                                returnTypeShapeListView.Items[0].Selected = true;
                            }
                            else
                            {
                                detectErrorLabel.Text = DialogsResource.NewFunctionImportDialog_NoColumnsReturned;
                                returnTypeShapeTabControl.SelectedTab = detectErrorTabPage;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_openedDbConnection != null)
                {
                    _openedDbConnection.Close();
                    _openedDbConnection = null;
                }
            }

            if (GetResultColumnsCompletedEventStorage != null)
            {
                GetResultColumnsCompletedEventStorage(this, EventArgs.Empty);
            }
        }

        private IVsDataConnection Connection
        {
            get
            {
                IVsDataConnection connection = null;

                if (ConnectionString != null)
                {
                    var designTimeConnectionString = ConnectionString.GetDesignTimeProviderConnectionString(CurrentProject);
                    var provider = ConnectionString.Provider;
                    var dcm = (IVsDataConnectionManager)Services.ServiceProvider.GetService(typeof(IVsDataConnectionManager));
                    connection = dcm.GetConnection(provider, designTimeConnectionString, false);
                }

                return connection;
            }
        }

        private ConnectionManager.ConnectionString ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    if (CurrentProject != null
                        && _container.LocalName != null)
                    {
                        _connectionString = ConnectionManager.GetConnectionStringObject(CurrentProject, _container.LocalName.Value);
                    }
                }
                return _connectionString;
            }
        }

        private Project CurrentProject
        {
            get
            {
                if (_currentProject == null)
                {
                    var documentPath = _container.Artifact.Uri.LocalPath;
                    _currentProject = VSHelpers.GetProjectForDocument(documentPath, PackageManager.Package);
                }
                return _currentProject;
            }
        }

        // set up supported features based on artifact's schema version
        private void InitializeSupportedFeatures()
        {
            _complexTypeFeatureState = FeatureState.VisibleButDisabled;
            _composableFunctionImportFeatureState = FeatureState.VisibleButDisabled;
            _getColumnInformationFeatureState = FeatureState.VisibleAndEnabled;

            if (_container == null)
            {
                Debug.Fail("_container should be non-null");
            }
            else if (_container.Artifact == null)
            {
                Debug.Fail("_container.Artifact should be non-null");
            }
            else if (_container.Artifact.SchemaVersion == null)
            {
                Debug.Fail("_container.Artifact.SchemaVersion should be non-null");
            }
            else
            {
                var schemaVersion = _container.Artifact.SchemaVersion;
                _complexTypeFeatureState = EdmFeatureManager.GetFunctionImportReturningComplexTypeFeatureState(schemaVersion);
                _composableFunctionImportFeatureState = EdmFeatureManager.GetComposableFunctionImportFeatureState(schemaVersion);
                _getColumnInformationFeatureState = EdmFeatureManager.GetFunctionImportColumnInformationFeatureState(_container.Artifact);
                _sortedEdmPrimitiveTypes = ModelHelper.AllPrimitiveTypesSorted(schemaVersion);
            }
        }

        private void InitializeDialogFont()
        {
            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }
        }

        private void PopulateStoreProcedureList(ICollection<Function> functions)
        {
            storedProcComboBox.Items.Clear(); // clear out old list

            if (functions != null)
            {
                // if Function Import is composable then only list composable functions,
                // if Function Import is non-composable then only list non-composable functions
                var functionsToList = from function in functions
                                      where (function.IsComposable.Value == functionImportComposableCheckBox.Checked)
                                      select function;

                if (functionImportComposableCheckBox.Checked)
                {
                    Debug.Assert(functionsToList.All(f => f.IsComposable.Value), "Unexpected non-composable function.");

                    // TVFs return row types so the return type cannot be defined inline. If the result is defined 
                    // inline it must be a scalar valued function and scalar valued functions are currently not
                    // supported in the conceptual model. Therefore, to prevent the user from creating a FunctionImport 
                    // for a scalar valued function we need to filter out all scalar valued functions. Note that a composable
                    // function always returns something and v3 SSDL schema only allows a Collection of RowTypes for
                    // non-inline ReturnType definition. Also you cannot have both inline and non-inline ReturnType
                    // defined for a single function - they are mutually exclusive. As a result we can assume that a 
                    // composable function is a TVF if the inline ReturnType is null. I don't think there is a better/stronger 
                    // way to assert this with current design. Drilling down to the actual return type would either require 
                    // to change the design or reparse the Xml fragment for the current function. 
                    functionsToList = functionsToList.Where(f => f.ReturnType.Value == null);
                }

                foreach (var function in functionsToList)
                {
                    storedProcComboBox.Items.Add(function);
                }
            }
        }

        private void SetComplexTypeTooltip()
        {
            if (!_complexTypeFeatureState.IsEnabled())
            {
                // Show tool tip on a control explaining why it is not enabled.
                // Unfortunately, by default, the tooltip is not shown when parent control is not enabled.
                // We need to manually show the tool tip. This issue will go away once the we move to use WPF.
                _controlWithToolTipShown = null;
                _featureDisabledToolTip = new ToolTip();
                _featureDisabledToolTip.SetToolTip(complexTypeReturnButton, Design.Resources.DisabledFeatureTooltip);
                _featureDisabledToolTip.SetToolTip(complexTypeReturnComboBox, Design.Resources.DisabledFeatureTooltip);
                _featureDisabledToolTip.SetToolTip(createNewComplexTypeButton, Design.Resources.DisabledFeatureTooltip);
                _featureDisabledToolTip.SetToolTip(updateComplexTypeButton, Design.Resources.DisabledFeatureTooltip);
                complexTypeReturnButton.Parent.MouseMove += ReturnTypeArea_OnMouseMove;
                complexTypeReturnButton.Parent.MouseLeave += ReturnTypeArea_OnMouseLeave;
                createNewComplexTypeButton.Parent.MouseMove += ReturnTypeArea_OnMouseMove;
                createNewComplexTypeButton.Parent.MouseLeave += ReturnTypeArea_OnMouseLeave;
            }
        }

        // <summary>
        //     ComplexType combo box can have 2 mode (editable vs non-editable mode).
        // </summary>
        private void SetComplexTypeReturnComboBoxStyle(bool isEditable)
        {
            complexTypeReturnComboBox.DropDownStyle = isEditable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            complexTypeReturnComboBox.AutoCompleteMode = isEditable ? AutoCompleteMode.SuggestAppend : AutoCompleteMode.None;
            complexTypeReturnComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        // <summary>
        //     Determine whether we need to enable/disable create new complex type button.
        // </summary>
        private void SetCreateNewComplexTypeButtonProperties()
        {
            // Only enable createNewComplexTypeButton if we could get column information and create function import returning complex type is supported
            createNewComplexTypeButton.Enabled = (Schema != null) && _complexTypeFeatureState.IsEnabled();
        }

        // <summary>
        //     Launch background process to retrieve columns schema info from database.
        // </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void LaunchBackgroundProcessToRetrieveColumnsSchema()
        {
            if (CurrentProject != null
                && ConnectionString != null
                && Function != null)
            {
                // update UI
                _isWorkerRunning = true;
                SetDoWorkState(true);

                // execute
                var bw = new BackgroundWorker();
                bw.WorkerReportsProgress = false;
                bw.WorkerSupportsCancellation = false;
                bw.DoWork += OnDoWork;
                bw.RunWorkerCompleted += OnWorkCompleted;
                bw.RunWorkerAsync(Function);
            }
        }

        #endregion

        // Helper class to add custom steps on Form Dispose.
        private class Disposer : Component
        {
            private readonly Action<bool> _dispose;

            internal Disposer(Action<bool> disposeCallback)
            {
                _dispose = disposeCallback;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _dispose(disposing);
            }
        }
    }
}
