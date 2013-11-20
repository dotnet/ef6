// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class LayerManager
    {
        private const string _propertyNameFormat = "IsLayerEnabled_{0}";
        private readonly EFArtifact _artifact;
        private EntityDesignSelectionContainer<LayerSelection> _selectionContainer;
        private readonly Dictionary<IEntityDesignerLayer, LayerState> _layer2state = new Dictionary<IEntityDesignerLayer, LayerState>();

        private readonly IDictionary<EntityDesignerCommand, CommandID> _commands2ids = new Dictionary<EntityDesignerCommand, CommandID>();
        private readonly IDictionary<int, EntityDesignerCommand> _intIds2commands = new Dictionary<int, EntityDesignerCommand>();

        private int _currentCommandId = PackageConstants.cmdIdLayerCommandsBase;
        private readonly List<int> _commandIdFreeList = new List<int>();

        private int _currentRefactoringCommandId = PackageConstants.cmdIdLayerRefactoringCommandsBase;
        private readonly List<int> _refactoringCommandIdFreeList = new List<int>();

        internal EFObject SelectedEFObject
        {
            get
            {
                var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(_artifact.Uri);
                if (editingContext != null)
                {
                    Selection selection = editingContext.Items.GetValue<UI.Views.EntityDesigner.EntityDesignerSelection>();
                    if (selection != null)
                    {
                        return selection.PrimarySelection;
                    }
                }
                return null;
            }
        }

        internal IEnumerable<IEntityDesignerLayer> EnabledLayerExtensions
        {
            get
            {
                return from l2s in _layer2state
                       where l2s.Value.IsEnabled
                       select l2s.Key;
            }
        }

        internal LayerManager(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Must pass in non-null artifact to LayerManager");

            _artifact = artifact;
        }

        private CommandID GetCommandID(EntityDesignerCommand command)
        {
            return _commands2ids.Where(kvp => kvp.Key.Equals(command)).Select(kvp => kvp.Value).FirstOrDefault();
        }

        private bool AddCommand(EntityDesignerCommand command)
        {
            var usingIdFromFreeList = false;

            int newCommandId;

            // If this is a refactoring command, we have a separate
            // free list and counter to keep track of it since it
            // lives in a separate command ID range.
            var currentCommandId = _currentCommandId;
            var freeList = _commandIdFreeList;
            if (command.IsRefactoringCommand)
            {
                freeList = _refactoringCommandIdFreeList;
                currentCommandId = _currentRefactoringCommandId;
            }

            if (freeList.Count > 0)
            {
                // _commandIdFreeList is tracking gaps in our command IDs, so use these instead of incrementing the current Id counter
                // if available.
                newCommandId = freeList[0];
                usingIdFromFreeList = true;
            }
            else
            {
                newCommandId = currentCommandId;
            }

            var commandId = new CommandID(PackageConstants.guidEscherCmdSet, newCommandId);

            DynamicStatusMenuCommand menuCommand;
            if (PackageManager.Package.CommandSet.AddCommand(commandId, command, out menuCommand))
            {
                // If we are attempting to use an id from the free list then we should
                // remove it from the free list if we've successfully added the command, or
                // just increment our current counter. This is different for refactoring commands
                // since refactoring commands live in a separate command ID range.
                if (command.IsRefactoringCommand)
                {
                    if (usingIdFromFreeList)
                    {
                        _refactoringCommandIdFreeList.RemoveAt(0);
                    }
                    else
                    {
                        _currentRefactoringCommandId = newCommandId + 1;
                    }
                }
                else
                {
                    if (usingIdFromFreeList)
                    {
                        _commandIdFreeList.RemoveAt(0);
                    }
                    else
                    {
                        _currentCommandId = newCommandId + 1;
                    }
                }

                _commands2ids.Add(command, commandId);
                _intIds2commands.Add(commandId.ID, command);

                return true;
            }

            return false;
        }

        private void RemoveCommand(EntityDesignerCommand command)
        {
            var commandID = GetCommandID(command);
            if (commandID != null)
            {
                if (PackageManager.Package.CommandSet.RemoveCommand(commandID))
                {
                    _commands2ids.Remove(command);

                    var indexToRemove = commandID.ID;
                    Debug.Assert(indexToRemove >= 0, "The index to remove is less than zero");
                    if (indexToRemove >= 0)
                    {
                        _intIds2commands.Remove(indexToRemove);

                        // Add the Id of the command we remove to the free list, so that we can fill the command Id gaps
                        // when we next load a dynamic command. The list differs for a refactoring command since
                        // those commands live in a different Command ID range.
                        var freeList = _commandIdFreeList;
                        if (command.IsRefactoringCommand)
                        {
                            freeList = _refactoringCommandIdFreeList;
                        }

                        freeList.Add(indexToRemove);
                        freeList.Sort();
                    }
                }
            }
        }

        private bool AddEnableLayerCommand(
            IEntityDesignerLayer layer, bool isAlreadyEnabled, out EntityDesignerCommand entityDesignerCommand)
        {
            var menuItemText = GetEnableLayerCommandText(layer.Name, isAlreadyEnabled);
            entityDesignerCommand = new EntityDesignerCommand(menuItemText, (xel, dv, ss, pc, iss) => { ToggleLayerEnabled(layer, xel); });

            return AddCommand(entityDesignerCommand);
        }

        internal void ToggleLayerEnabled(IEntityDesignerLayer layer, XObject selectedXObject)
        {
            LayerState layerState;
            if (_layer2state.TryGetValue(layer, out layerState))
            {
                layerState.IsEnabled = !layerState.IsEnabled;
                layerState.EnableCommand.Name = GetEnableLayerCommandText(layer.Name, layerState.IsEnabled);
            }

            Debug.Assert(layerState != null, "LayerState is null for layer '" + layer.Name + "'");
            if (layerState != null
                && layerState.IsEnabled)
            {
                PersistLayerEnabled(layer, true);
                LoadLayer(layer, selectedXObject);
            }
            else
            {
                UnloadLayer(layer);
                PersistLayerEnabled(layer, false);
            }
        }

        private void UnloadLayer(IEntityDesignerLayer layer)
        {
            if (_artifact != null
                && _artifact.ConceptualModel() != null)
            {
                layer.OnBeforeLayerUnloaded(_artifact.ConceptualModel().XObject);
            }

            foreach (var command in GetCommands(layer))
            {
                RemoveCommand(command);
            }

            layer.ChangeEntityDesignerSelection -= layer_EntityDesignerSelectionChanged;

            if (_selectionContainer != null)
            {
                _selectionContainer.Dispose();
                _selectionContainer = null;
            }
        }

        internal void UnloadAllLayers()
        {
            var layersToRemove = new List<IEntityDesignerLayer>();
            foreach (var layer in _layer2state.Keys)
            {
                UnloadLayer(layer);
                layersToRemove.Add(layer);
            }

            foreach (var layer in layersToRemove)
            {
                _layer2state.Remove(layer);
            }
        }

        internal virtual void Unload()
        {
            StopListeningToSelections();
            UnloadAllLayers();

            var commandsToRemove = new List<EntityDesignerCommand>();
            foreach (var leftoverCommand in _commands2ids.Keys)
            {
                commandsToRemove.Add(leftoverCommand);
            }

            foreach (var command in commandsToRemove)
            {
                RemoveCommand(command);
            }
        }

        private void StopListeningToSelections()
        {
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(_artifact.Uri);
            editingContext.Items.Unsubscribe<UI.Views.EntityDesigner.EntityDesignerSelection>(OnEntityDesignerSelectionChanged);
        }

        internal void Load()
        {
            // Load all the layers first
            var extensions = EscherExtensionPointManager.LoadLayerExtensions();
            if (extensions != null)
            {
                var selectedEFElement = SelectedEFObject as EFElement;
                foreach (var ex in extensions)
                {
                    var layer = ex.Value;
                    if (layer != null)
                    {
                        var isLayerEnabled = IsLayerEnabled(layer.Name);
                        EntityDesignerCommand enableCommand;
                        var addedCommand = AddEnableLayerCommand(layer, isLayerEnabled, out enableCommand);
                        if (addedCommand && enableCommand != null)
                        {
                            if (isLayerEnabled)
                            {
                                LoadLayer(layer, selectedEFElement != null ? selectedEFElement.XObject : null);
                            }

                            var layerState = new LayerState { IsEnabled = isLayerEnabled, EnableCommand = enableCommand };
                            _layer2state.Add(layer, layerState);
                        }
                    }
                }
            }

            // TODO Now we load the commands from the command factories. At some point we should move this out of the layer manager
            // into a more global object (not tied to the lifetime of the artifact)
            foreach (var command in GetCommands())
            {
                AddCommand(command);
            }
            ListenToSelections();
        }

        private void ListenToSelections()
        {
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(_artifact.Uri);
            editingContext.Items.Subscribe<UI.Views.EntityDesigner.EntityDesignerSelection>(OnEntityDesignerSelectionChanged);
        }

        private void OnEntityDesignerSelectionChanged(UI.Views.EntityDesigner.EntityDesignerSelection selection)
        {
            // We are seeing selection.PrimarySelection == null when the artifact is reloaded so add check here to prevent NRE.
            if (selection.PrimarySelection != null)
            {
                var selectedXObject = selection.PrimarySelection.XObject;
                if (selectedXObject != null)
                {
                    foreach (var layer in _layer2state.Keys)
                    {
                        layer.OnSelectionChanged(selectedXObject);
                    }
                }
            }
        }

        private void LoadLayer(IEntityDesignerLayer layer, XObject selectedXObject)
        {
            if (_artifact != null
                && _artifact.ConceptualModel() != null)
            {
                if (selectedXObject != null)
                {
                    layer.OnAfterLayerLoaded(selectedXObject);
                }
                else
                {
                    layer.OnAfterLayerLoaded(_artifact.ConceptualModel().XObject);
                }
            }

            if (layer.ServiceProvider != null
                && PackageManager.Package.DocumentFrameMgr != null
                && PackageManager.Package.DocumentFrameMgr.EditingContextManager != null)
            {
                if (PackageManager.Package.DocumentFrameMgr.EditingContextManager.DoesContextExist(_artifact.Uri))
                {
                    var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(
                        _artifact.Uri);
                    Debug.Assert(editingContext != null, "EditingContext must not be null if we found that a context exists");
                    if (editingContext != null)
                    {
                        // TODO there should be one independent selection container for each layer.
                        _selectionContainer = new EntityDesignSelectionContainer<LayerSelection>(layer.ServiceProvider, editingContext);
                    }
                }
            }

            layer.ChangeEntityDesignerSelection += layer_EntityDesignerSelectionChanged;
            foreach (var command in GetCommands(layer))
            {
                AddCommand(command);
            }
        }

        private static IEnumerable<EntityDesignerCommand> GetCommands(IEntityDesignerLayer layer = null)
        {
            var commandsToReturn = new List<EntityDesignerCommand>();
            var commandsForLayer = EscherExtensionPointManager.LoadCommandExtensions(layer == null, layer != null);

            foreach (var lazyFactory in commandsForLayer)
            {
                var factory = lazyFactory.Value;
                if (factory != null)
                {
                    commandsToReturn.AddRange(factory.Commands);
                }
            }
            return commandsToReturn;
        }

        internal void layer_EntityDesignerSelectionChanged(object sender, ChangeEntityDesignerSelectionEventArgs e)
        {
            if (PackageManager.Package.DocumentFrameMgr != null
                && PackageManager.Package.DocumentFrameMgr.EditingContextManager != null)
            {
                if (PackageManager.Package.DocumentFrameMgr.EditingContextManager.DoesContextExist(_artifact.Uri))
                {
                    var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(
                        _artifact.Uri);
                    Debug.Assert(editingContext != null, "EditingContext must not be null if we found that a context exists");
                    if (editingContext != null)
                    {
                        // TODO handle multiple selection at some point
                        var selectedItems = new List<EFNameableItem>();
                        foreach (var selectionIdentifier in e.SelectionIdentifiers)
                        {
                            if (!String.IsNullOrEmpty(selectionIdentifier))
                            {
                                var nameableItem = XmlModelHelper.FindNameableItemViaIdentifier(
                                    _artifact.ConceptualModel(), selectionIdentifier);
                                if (nameableItem != null)
                                {
                                    selectedItems.Add(nameableItem);
                                }
                            }
                        }

                        if (selectedItems.Count > 0)
                        {
                            editingContext.Items.SetValue(new LayerSelection(selectedItems));
                        }
                    }
                }
            }
        }

        private static string GetEnableLayerCommandText(string layerName, bool isEnabled)
        {
            return String.Format(
                CultureInfo.CurrentCulture, isEnabled ? Resources.Layer_DisableLayer : Resources.Layer_EnableLayer, layerName);
        }

        private void PersistLayerEnabled(IEntityDesignerLayer layer, bool enable)
        {
            var editingContextMgr = PackageManager.Package.DocumentFrameMgr.EditingContextManager;
            Debug.Assert(editingContextMgr.DoesContextExist(_artifact.Uri), "There should be an existing editing context");
            if (editingContextMgr.DoesContextExist(_artifact.Uri))
            {
                var txname = string.Format(
                    CultureInfo.CurrentCulture, enable ? Resources.Tx_LayerEnable : Resources.Tx_LayerDisable, layer.Name);
                var cpc = new CommandProcessorContext(
                    editingContextMgr.GetNewOrExistingContext(_artifact.Uri), EfiTransactionOriginator.EntityDesignerOriginatorId, txname);
                var cmd = ModelHelper.CreateSetDesignerPropertyValueCommandFromArtifact(
                    cpc.Artifact, OptionsDesignerInfo.ElementName
                    , string.Format(CultureInfo.CurrentCulture, _propertyNameFormat, layer.Name)
                    , enable.ToString());
                if (cmd != null)
                {
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal bool IsLayerEnabled(string layerName)
        {
            if (!String.IsNullOrEmpty(layerName))
            {
                return ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                    OptionsDesignerInfo.ElementName
                    , String.Format(CultureInfo.CurrentCulture, _propertyNameFormat, layerName), false, _artifact);
            }
            return false;
        }

        internal void OnAfterTransactionCommitted(IEnumerable<Tuple<XObject, XObjectChange>> xmlChanges)
        {
            foreach (var layer in EnabledLayerExtensions)
            {
                layer.OnAfterTransactionCommitted(xmlChanges);
            }
        }

        internal IEnumerable<Lazy<T, M>> Filter<T, M>(
            IEnumerable<Lazy<T, M>> extensionList, bool excludeLayers = false, bool excludeNonLayers = false)
        {
            return extensionList.Where(
                l =>
                    {
                        if (l.Metadata != null)
                        {
                            var layerData = l.Metadata as IEntityDesignerLayerData;
                            if (layerData != null
                                && !String.IsNullOrWhiteSpace(layerData.LayerName))
                            {
                                return !excludeLayers && IsLayerEnabled(layerData.LayerName);
                            }
                        }
                        return !excludeNonLayers;
                    });
        }
    }
}
