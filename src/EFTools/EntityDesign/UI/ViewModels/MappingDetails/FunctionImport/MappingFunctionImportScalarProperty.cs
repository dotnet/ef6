// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     Class that represents a scalar property in the FunctionImportMapping.  This class has to be creatable without a ModelItem existing
    ///     since we want to be able to display every Property from the ReturnType of the FunctionImport, even if the Property isn't mapped
    ///     yet.  So, this class has the ability to store c-side Property information; and this is only used if
    ///     there isn't an associated ModelItem.
    /// </summary>
    [TreeGridDesignerRootBranch(typeof(FunctionImportScalarPropertyBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 3)]
    internal class MappingFunctionImportScalarProperty : MappingFunctionImportMappingRoot
    {
        // this is used when we don't yet have a ScalarProperty to link to
        private Property _property;

        public MappingFunctionImportScalarProperty(EditingContext context, FunctionImportScalarProperty sp, MappingEFElement parent)
            : base(context, sp, parent)
        {
            Debug.Assert(sp != null, "FunctionImportScalarProperty shouldn't be null");
        }

        /// <summary>
        ///     Constructor for the dummy node when there is no corresponding ScalarProperty
        /// </summary>
        public MappingFunctionImportScalarProperty(EditingContext context, Property property, MappingEFElement parent)
            : base(context, null, parent)
        {
            Debug.Assert(property != null, "Property shouldn't be null");
            _property = property;
        }

        internal FunctionImportScalarProperty ScalarProperty
        {
            get { return ModelItem as FunctionImportScalarProperty; }
        }

        internal override string Name
        {
            get { return ColumnUtils.BuildPropertyDisplay(Property, PropertyType); }
        }

        internal string Property
        {
            get
            {
                // if we don't have a backing SP, then return name of the cached property
                if (ScalarProperty == null)
                {
                    return _property.LocalName.Value;
                }
                else
                {
                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.Name.Target.LocalName.Value;
                    }
                    else
                    {
                        return ScalarProperty.Name.RefName;
                    }
                }
            }
        }

        internal string PropertyType
        {
            get
            {
                // if we don't have a backing SP, then return type of the cached property
                if (ScalarProperty == null)
                {
                    return _property.TypeName;
                }
                else
                {
                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.Name.Target.TypeName;
                    }
                    else
                    {
                        return Resources.MappingDetails_UnknownColumnType;
                    }
                }
            }
        }

        internal bool IsKeyProperty
        {
            get
            {
                // if we don't have a backing SP, then return value from the cached property
                if (ScalarProperty == null)
                {
                    return _property.IsKeyProperty;
                }
                else
                {
                    if (ScalarProperty.Name.Status == BindingStatus.Known)
                    {
                        return ScalarProperty.Name.Target.IsKeyProperty;
                    }
                    return false;
                }
            }
        }

        internal string ColumnName
        {
            get
            {
                if (ScalarProperty != null)
                {
                    return ScalarProperty.ColumnName.Value;
                }
                else
                {
                    if (IsComplexProperty)
                    {
                        // ComplexProperties are not supported in the FunctionImportMapping, showing an error message
                        return Resources.MappingDetails_ErrComplexTypePropertiesNotSupported;
                    }
                    // if there is no ScalarProperty associated then return default name (name of the c-side Property)
                    return Property;
                }
            }
            set
            {
                Debug.Assert(ScalarProperty != null, "This can be called only if there is an associated ScalarProperty");
                if (ScalarProperty != null)
                {
                    // check if anything changed
                    if (String.CompareOrdinal(ColumnName, value) != 0)
                    {
                        if (String.CompareOrdinal(Property, value) == 0)
                        {
                            // if the name of the column is same as the default name then simply delete associated ScalarProperty
                            DeleteModelItem(null);
                        }
                        else
                        {
                            // change the column name
                            var cpc = new CommandProcessorContext(
                                Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                            var cmd = new UpdateDefaultableValueCommand<string>(ScalarProperty.ColumnName, value);

                            var cp = new CommandProcessor(cpc, cmd);
                            cp.Invoke();
                        }
                    }
                }
            }
        }

        internal bool IsComplexProperty
        {
            get { return _property is ComplexConceptualProperty; }
        }

        internal void CreateModelItem(CommandProcessorContext cpc, EditingContext context, string columnName)
        {
            Debug.Assert(context != null, "The context argument cannot be null");
            Debug.Assert(ScalarProperty == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(_property != null, "The _property cannot be null.");
            Debug.Assert(
                MappingFunctionImport.FunctionImport != null && MappingFunctionImport.FunctionImportMapping != null,
                "The parent item isn't set up correctly");

            Context = context;
            var fi = MappingFunctionImport.FunctionImport;
            var fim = MappingFunctionImport.FunctionImportMapping;
            if (_property != null
                && fi != null
                && fim != null)
            {
                // get the ReturnType of the FunctionImport
                EntityType entityType = null;
                ComplexType complexType = null;
                if (fi.IsReturnTypeEntityType)
                {
                    entityType = fi.ReturnTypeAsEntityType.Target;
                }
                else if (fi.IsReturnTypeComplexType
                         && fi.ReturnTypeAsComplexType.Target != null)
                {
                    complexType = fi.ReturnTypeAsComplexType.Target;
                }

                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                }

                // first we need to create a FunctionImportTypeMapping element (either EntityTypeMapping or ComplexTypeMapping)
                var cmd = entityType != null
                              ? new CreateFunctionImportTypeMappingCommand(MappingFunctionImport.FunctionImportMapping, entityType)
                              : new CreateFunctionImportTypeMappingCommand(MappingFunctionImport.FunctionImportMapping, complexType);

                // create the ScalarProperty
                var cmd2 = new CreateFunctionImportScalarPropertyCommand(cmd, _property, columnName);
                // set up our post event to fix up the view model
                cmd2.PostInvokeEvent += (o, eventsArgs) =>
                    {
                        var sp = cmd2.ScalarProperty;
                        Debug.Assert(sp != null, "Didn't get good ScalarProperty out of the command");

                        // fix up our view model (we don't have to add this to the parent's children collection
                        // because we created a placeholder row already for each property)
                        ModelItem = sp;
                    };

                // now make the change
                var cp = new CommandProcessor(cpc, cmd, cmd2);
                cp.Invoke();
            }
        }

        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");

            if (IsModelItemDeleted() == false)
            {
                // first cache the property
                _property = ScalarProperty.Name.Target;

                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeScalarProperty);
                }

                // use the item's delete command
                var deleteCommand = ScalarProperty.GetDeleteCommand();
                deleteCommand.PostInvokeEvent += (o, eventsArgs) => { ModelItem = null; };

                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
