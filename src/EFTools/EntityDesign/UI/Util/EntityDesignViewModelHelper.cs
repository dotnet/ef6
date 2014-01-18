// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Util
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal static class EntityDesignViewModelHelper
    {
        // <summary>
        //     Shows the New Function Dialog appropriate to the schemaVersion
        // </summary>
        // <param name="editingContext">The editing context to use.</param>
        // <param name="artifact">The artifact to use.</param>
        // <param name="selectedSproc">The selected stored procedure.</param>
        // <param name="sModel">The input storage model</param>
        // <param name="cModel">The input conceptual model</param>
        // <param name="cContainer">The input conceptual EntityContainer</param>
        // <param name="entityType">Return item originally selected for this FunctionImport (null means none selected)</param>
        // <param name="originatingId">Originating ID used for transaction.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static FunctionImport CreateFunctionImport(
            EditingContext editingContext,
            EFArtifact artifact,
            Function selectedSproc,
            StorageEntityModel sModel,
            ConceptualEntityModel cModel,
            ConceptualEntityContainer cContainer,
            EntityType entityType,
            string originatingId)
        {
            FunctionImport functionImportResult = null;

            Debug.Assert(editingContext != null, "editingContext should not be null");
            Debug.Assert(artifact != null, "artifact should not be null");
            Debug.Assert(!string.IsNullOrEmpty(originatingId), "originatingId should not be null or empty");

            // show dialog appropriate to framework version
            var result = ShowNewFunctionImportDialog(
                selectedSproc,
                null /* selectedSprocName Parameter */,
                sModel,
                cModel,
                cContainer,
                DialogsResource.NewFunctionImportDialog_AddFunctionImportTitle,
                entityType);

            // if user selected OK on the dialog then create the FunctionImport
            if (DialogResult.OK == result.DialogResult)
            {
                var commands = new Collection<Command>();

                CreateComplexTypeCommand createComplexTypeCommand = null;

                // Make the decision based on what is returned by the dialog.
                // If return type is a string and result schema is not null, that means the user would like create a new complex type for the function import return.
                if (result.ReturnType is string
                    && result.Schema != null)
                {
                    createComplexTypeCommand = CreateMatchingFunctionImportCommand.AddCreateComplexTypeCommands(
                        sModel, result.ReturnType as string, result.Schema.RawColumns, commands);
                }
                    // If ReturnType is a complex type and result schema is not null, the complex type needs to be updated to be in sync with schema columns.
                else if (result.ReturnType is ComplexType
                         && result.Schema != null)
                {
                    var complexType = result.ReturnType as ComplexType;
                    var propertiesDictionary = complexType.Properties().ToDictionary(p => p.LocalName.Value);
                    CreateMatchingFunctionImportCommand.AddChangeComplexTypePropertiesCommands(
                        complexType, propertiesDictionary, result.Schema.RawColumns, commands);
                }

                CreateFunctionImportCommand cmdFuncImp;
                if (createComplexTypeCommand == null)
                {
                    cmdFuncImp = new CreateFunctionImportCommand(cContainer, result.Function, result.FunctionName, result.ReturnType);
                }
                else
                {
                    // Pass in the pre-req command to create complex type to the command.
                    cmdFuncImp = new CreateFunctionImportCommand(cContainer, result.Function, result.FunctionName, createComplexTypeCommand);
                }

                commands.Add(cmdFuncImp);

                // now add a FunctionImport and a FunctionImportMapping (if appropriate)
                if (artifact.MappingModel() != null
                    && artifact.MappingModel().FirstEntityContainerMapping != null)
                {
                    var cmdFuncImpMapping = new CreateFunctionImportMappingCommand(
                        artifact.MappingModel().FirstEntityContainerMapping, result.Function, cmdFuncImp.Id);
                    cmdFuncImpMapping.AddPreReqCommand(cmdFuncImp);
                    commands.Add(cmdFuncImpMapping);

                    IDictionary<string, string> mapPropertyNameToColumnName = null;
                    if (result.Schema != null)
                    {
                        mapPropertyNameToColumnName =
                            ModelHelper.ConstructComplexTypePropertyNameToColumnNameMapping(
                                result.Schema.Columns.Select(c => c.Name).ToList());
                    }

                    // Create explicit function-import result type mapping if the return type is a complex type.
                    if (createComplexTypeCommand != null)
                    {
                        commands.Add(
                            new CreateFunctionImportTypeMappingCommand(cmdFuncImpMapping, createComplexTypeCommand)
                                {
                                    CreateDefaultScalarProperties = true,
                                    PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                                });
                    }
                    else if (result.ReturnType is ComplexType)
                    {
                        commands.Add(
                            new CreateFunctionImportTypeMappingCommand(cmdFuncImpMapping, result.ReturnType as ComplexType)
                                {
                                    CreateDefaultScalarProperties = true,
                                    PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                                });
                    }
                }

                var cp = new CommandProcessor(editingContext, originatingId, Resources.Tx_CreateFunctionImport, commands);
                cp.Invoke();

                functionImportResult = cmdFuncImp.FunctionImport;
                NavigateToFunction(functionImportResult);
            }

            return functionImportResult;
        }

        // <summary>
        //     Shows the New Function Dialog appropriate to the schemaVersion in EditMode
        // </summary>
        // <param name="editingContext">The editing context to use.</param>
        // <param name="functionImport">Stored procedure originally selected for this FunctionImport (null means none selected)</param>
        // <param name="sModel">The input storage model</param>
        // <param name="cModel">The input conceptual model</param>
        // <param name="cContainer">The input conceptual EntityContainer</param>
        // <param name="selectedObject">Return item originally selected for this FunctionImport (null means none selected)</param>
        // <param name="originatingId">Originating ID used for transaction.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static void EditFunctionImport(
            EditingContext editingContext,
            FunctionImport functionImport,
            StorageEntityModel sModel,
            ConceptualEntityModel cModel,
            ConceptualEntityContainer cContainer,
            object selectedObject,
            string originatingId)
        {
            Debug.Assert(editingContext != null, "editingContext should not be null");
            Debug.Assert(!string.IsNullOrEmpty(originatingId), "originatingId should not be null or empty");

            // show dialog appropriate to framework version
            var result = ShowNewFunctionImportDialog(
                functionImport.Function,
                functionImport.LocalName.Value,
                sModel,
                cModel,
                cContainer,
                DialogsResource.NewFunctionImportDialog_EditFunctionImportTitle,
                selectedObject);

            // if user selected OK on the dialog then create the FunctionImport
            if (DialogResult.OK == result.DialogResult)
            {
                var commands = new List<Command>();
                var cp = new CommandProcessor(editingContext, originatingId, Resources.Tx_UpdateFunctionImport);
                CreateComplexTypeCommand createComplexTypeCommand = null;

                // Make the decision based on what is returned by the dialog.
                // If return type is a string and result schema is not null, that means the user would like to create a new complex type for the function import return.
                if (result.ReturnType is string
                    && result.Schema != null)
                {
                    createComplexTypeCommand = CreateMatchingFunctionImportCommand.AddCreateComplexTypeCommands(
                        sModel, result.ReturnType as string, result.Schema.RawColumns, commands);
                }
                    // If ReturnType is a complex type and result schema is not null, the complex type needs to be updated to be in sync with schema columns.
                else if (result.ReturnType is ComplexType
                         && result.Schema != null)
                {
                    var complexType = result.ReturnType as ComplexType;
                    // Create Column properties dictionary. The keys will be either property's type-mapping column name if availabe or property's name.
                    var propertiesDictionary =
                        complexType.Properties().ToDictionary(p => EdmUtils.GetFunctionImportResultColumnName(functionImport, p));
                    CreateMatchingFunctionImportCommand.AddChangeComplexTypePropertiesCommands(
                        complexType, propertiesDictionary, result.Schema.RawColumns, commands);
                }

                // construct Dictionary mapping property name to column name for FunctionImportMapping
                IDictionary<string, string> mapPropertyNameToColumnName = null;
                if (result.Schema != null)
                {
                    mapPropertyNameToColumnName =
                        ModelHelper.ConstructComplexTypePropertyNameToColumnNameMapping(result.Schema.Columns.Select(c => c.Name).ToList());
                }

                // change the FunctionImport and FunctionImportMapping to match
                ChangeFunctionImportCommand cmdFuncImpSproc = null;
                // if result.IsComposable is true then set to True, but if false then use None if existing value is None, otherwise False
                var resultIsComposable = (result.IsComposable
                                              ? BoolOrNone.TrueValue
                                              : (BoolOrNone.NoneValue == functionImport.IsComposable.Value
                                                     ? BoolOrNone.NoneValue
                                                     : BoolOrNone.FalseValue));
                if (createComplexTypeCommand == null)
                {
                    cmdFuncImpSproc = new ChangeFunctionImportCommand(
                        cContainer, functionImport, result.Function, result.FunctionName, resultIsComposable, true, result.ReturnType);
                    // Create explicit function-import result type mapping if the return type is a complex type.
                    if (result.ReturnType is ComplexType)
                    {
                        cmdFuncImpSproc.PostInvokeEvent += (o, eventArgs) =>
                            {
                                if (functionImport != null
                                    && functionImport.FunctionImportMapping != null)
                                {
                                    // CreateFunctionImportTypeMappingCommand will be no op function-import's return is unchanged.
                                    CommandProcessor.InvokeSingleCommand(
                                        cp.CommandProcessorContext
                                        ,
                                        new CreateFunctionImportTypeMappingCommand(
                                            functionImport.FunctionImportMapping, result.ReturnType as ComplexType)
                                            {
                                                CreateDefaultScalarProperties = true,
                                                PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                                            });
                                }
                            };
                    }
                }
                else
                {
                    // Pass in the pre-req command to create complex type to the command.
                    cmdFuncImpSproc = new ChangeFunctionImportCommand(
                        cContainer, functionImport, result.Function, result.FunctionName,
                        resultIsComposable, createComplexTypeCommand);
                    // Create explicit function-import result type mapping if the return type is a complex type.
                    cmdFuncImpSproc.PostInvokeEvent += (o, eventArgs) =>
                        {
                            if (functionImport != null
                                && functionImport.FunctionImportMapping != null)
                            {
                                CommandProcessor.InvokeSingleCommand(
                                    cp.CommandProcessorContext,
                                    new CreateFunctionImportTypeMappingCommand(
                                        functionImport.FunctionImportMapping, createComplexTypeCommand)
                                        {
                                            CreateDefaultScalarProperties = true,
                                            PropertyNameToColumnNameMap = mapPropertyNameToColumnName
                                        });
                            }
                        };
                }
                commands.Add(cmdFuncImpSproc);
                commands.ForEach(x => cp.EnqueueCommand(x));
                cp.Invoke();
            }
        }

        // <summary>
        //     Show the dialog to edit an existing enum type.
        // </summary>
        public static void EditEnumType(
            EditingContext editingContext, string originatingId, EnumTypeViewModel enumTypeViewModel, EventHandler onDialogActivated = null)
        {
            if (editingContext == null)
            {
                throw new ArgumentNullException("editingContext");
            }
            if (enumTypeViewModel == null)
            {
                throw new ArgumentNullException("enumTypeViewModel");
            }

            Debug.Assert(
                !enumTypeViewModel.IsNew,
                typeof(EntityDesignViewModelHelper).Name + ".EditEnumType: Expected existing enum type is passed in");

            if (enumTypeViewModel.IsNew == false)
            {
                var result = ShowEnumTypeDialog(enumTypeViewModel, onDialogActivated);

                var enumType = enumTypeViewModel.EnumType;
                if (result == true)
                {
                    var cp = new CommandProcessor(editingContext, originatingId, Resources.Tx_UpdateEnumType);

                    cp.EnqueueCommand(
                        new SetEnumTypeFacetCommand(
                            enumType
                            , enumTypeViewModel.Name, enumTypeViewModel.SelectedUnderlyingType
                            , enumTypeViewModel.IsReferenceExternalType ? enumTypeViewModel.ExternalTypeName : String.Empty
                            , enumTypeViewModel.IsFlag));

                    // We delete and create enum type members.
                    // TODO: we might want to do intelligent edit for large number of enum type member.
                    foreach (var enumTypeMember in enumType.Members())
                    {
                        cp.EnqueueCommand(enumTypeMember.GetDeleteCommand());
                    }

                    foreach (var enumTypeMemberViewModel in enumTypeViewModel.Members)
                    {
                        if (String.IsNullOrWhiteSpace(enumTypeMemberViewModel.Name) == false)
                        {
                            cp.EnqueueCommand(
                                new CreateEnumTypeMemberCommand(enumType, enumTypeMemberViewModel.Name, enumTypeMemberViewModel.Value));
                        }
                    }

                    cp.Invoke();
                }
            }
        }

        // <summary>
        //     Show the dialog to create a new enum type.
        // </summary>
        public static EnumType AddNewEnumType(
            string selectedUnderlyingType, EditingContext editingContext, string originatingId, EventHandler onDialogActivated = null)
        {
            if (editingContext == null)
            {
                throw new ArgumentNullException("editingContext");
            }

            var artifactService = editingContext.GetEFArtifactService();
            var entityDesignArtifact = artifactService.Artifact as EntityDesignArtifact;

            Debug.Assert(
                entityDesignArtifact != null,
                typeof(EntityDesignViewModelHelper).Name
                + ".AddEnumType: Unable to find Entity Design Artifact from the passed in editing context.");

            if (entityDesignArtifact != null)
            {
                var vm = new EnumTypeViewModel(entityDesignArtifact, selectedUnderlyingType);

                var result = ShowEnumTypeDialog(vm, onDialogActivated);

                if (result == true
                    && vm.IsValid)
                {
                    var cp = new CommandProcessor(editingContext, originatingId, Resources.Tx_CreateEnumType);
                    var createEnumTypeCommand = new CreateEnumTypeCommand(
                        vm.Name, vm.SelectedUnderlyingType
                        , (vm.IsReferenceExternalType ? vm.ExternalTypeName : String.Empty), vm.IsFlag, false);

                    cp.EnqueueCommand(createEnumTypeCommand);

                    foreach (var member in vm.Members)
                    {
                        if (String.IsNullOrWhiteSpace(member.Name) == false)
                        {
                            cp.EnqueueCommand(new CreateEnumTypeMemberCommand(createEnumTypeCommand, member.Name, member.Value));
                        }
                    }
                    cp.Invoke();
                    return createEnumTypeCommand.EnumType;
                }
            }
            return null;
        }

        private static bool? ShowEnumTypeDialog(EnumTypeViewModel enumTypeViewModel, EventHandler onDialogActivated)
        {
            bool? result;
            try
            {
                var dialog = new EnumTypeDialog(enumTypeViewModel);
                if (onDialogActivated != null)
                {
                    EnumTypeDialog.DialogActivatedTestEvent += onDialogActivated;
                }
                result = dialog.ShowModal();
            }
            finally
            {
                if (onDialogActivated != null)
                {
                    EnumTypeDialog.DialogActivatedTestEvent -= onDialogActivated;
                }
            }

            return result;
        }

        private static EntityDesignNewFunctionImportResult ShowNewFunctionImportDialog(
            Function selectedSproc,
            string selectedSprocName,
            StorageEntityModel sModel,
            ConceptualEntityModel cModel,
            ConceptualEntityContainer cContainer,
            string dialogTitle,
            Object selectedObject)
        {
            using (var dialog = new NewFunctionImportDialog(
                selectedSproc,
                selectedSprocName,
                sModel.Functions(),
                cModel.ComplexTypes(),
                cModel.EntityTypes(),
                cContainer,
                selectedObject))
            {
                dialog.Text = dialogTitle;

                var dialogResult = dialog.ShowDialog();

                var result = new EntityDesignNewFunctionImportResult
                    {
                        DialogResult = dialogResult,
                        Function = dialog.Function,
                        FunctionName = dialog.FunctionImportName,
                        IsComposable = dialog.IsComposable,
                        ReturnType = dialog.ReturnType,
                        Schema = dialog.Schema
                    };

                return result;
            }
        }

        private static void NavigateToFunction(FunctionImport fi)
        {
            if (fi != null
                && fi.FunctionImportMapping != null)
            {
                Debug.Assert(fi.FunctionImportMapping.FunctionImportName != null, "FunctionImportName is null");
                if (fi.FunctionImportMapping.FunctionImportName != null)
                {
                    ExplorerNavigationHelper.NavigateTo(fi.FunctionImportMapping.FunctionImportName.Target);
                }
            }
        }
    }
}
