// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Commands;

    // The instance of this class decides when to begin/end an undo scope.
    // This enables operations among PropertyTypeDescriptors to be grouped in a single transaction.
    internal class LinkedDescriptorContextItem : ContextItem
    {
        #region Fields

        private readonly List<LinkedPropertyTypeDescriptor> _propertyTypeDescriptors = new List<LinkedPropertyTypeDescriptor>();
        private CommandProcessorContext _cpc;
        private int _counter;

        #endregion

        #region Override methods

        internal override Type ItemType
        {
            get { return typeof(LinkedDescriptorContextItem); }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~LinkedDescriptorContextItem()
        {
            Debug.Assert(
                _counter == 0 && _cpc == null,
                "Undo scope is not closed when the context item is garbage-collected. This means 'Reset' method is not called.");
        }

        internal void RegisterDescriptor(LinkedPropertyTypeDescriptor propertyTypeDescriptor)
        {
            Debug.Assert(
                _propertyTypeDescriptors.Contains(propertyTypeDescriptor) == false,
                "The passed in property type descriptor is already registered");
            if (_propertyTypeDescriptors.Contains(propertyTypeDescriptor) == false)
            {
                _propertyTypeDescriptors.Add(propertyTypeDescriptor);
            }
        }

        internal void BeginPropertyValueUpdate(EditingContext editingContext, string transactionName)
        {
            if (_cpc == null)
            {
                Debug.Assert(_counter == 0, "CommandProcessorContext is null when counter value is not 0?");
                if (_counter == 0)
                {
                    _cpc = PropertyWindowViewModelHelper.CreateCommandProcessorContext(editingContext, transactionName);
                    _cpc.EditingContext.ParentUndoUnitStarted = true;
                    _cpc.Artifact.XmlModelProvider.BeginUndoScope(transactionName);
                }
            }
        }

        internal void EndPropertyValueUpdate()
        {
            // CommandProcessorContext value can be null if an exception is thrown when the property value is updated.
            // In that case do nothing.
            if (_cpc != null)
            {
                _counter++;
                // When the value of the counter is equal to the number of propertyTypeDescriptor,
                // it indicates that all property values have been updated; so we need to close the undo scope and reset counter.
                if (_counter == _propertyTypeDescriptors.Count)
                {
                    Reset();
                }
            }
        }

        // When there is an exception during an update, we need to make sure that we reset the counter and close the undo scope.
        internal void OnPropertyValueUpdateException()
        {
            Reset();
        }

        private void Reset()
        {
            Debug.Assert(_cpc != null, "CommandProcessorContext should have been created.");

            if (_cpc != null)
            {
                try
                {
                    _cpc.Artifact.XmlModelProvider.EndUndoScope();
                    _cpc.EditingContext.ParentUndoUnitStarted = false;
                }
                finally
                {
                    PropertyWindowViewModelHelper.RemoveCommandProcessorContext();
                    _cpc = null;
                    _counter = 0;
                }
            }
        }
    }
}
