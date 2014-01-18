// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

    // <summary>
    //     This class provides the view level information for a condition in a mapping fragment.
    // </summary>
    [TreeGridDesignerRootBranch(typeof(ConditionBranch))]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ValueColumn), Order = 3)]
    internal class MappingCondition : MappingEntityMappingRoot
    {
        internal static readonly MappingLovEFElement LovOperatorIsPlaceHolder = new MappingLovEFElement(Resources.MappingDetails_OperatorIs);

        internal static readonly MappingLovEFElement LovOperatorEqualsPlaceHolder =
            new MappingLovEFElement(Resources.MappingDetails_OperatorEquals);

        internal static readonly MappingLovEFElement LovValueNullPlaceHolder = new MappingLovEFElement(Resources.MappingDetails_ValueNull);

        internal static readonly MappingLovEFElement LovValueNotNullPlaceHolder =
            new MappingLovEFElement(Resources.MappingDetails_ValueNotNull);

        private string _modelItemColumnName;

        public MappingCondition(EditingContext context, Condition condition, MappingEFElement parent)
            : base(context, condition, parent)
        {
        }

        internal Condition Condition
        {
            get
            {
                if (IsModelItemDeleted())
                {
                    // if the model item we are pointing to doesn't have an XLinq node, clear it out
                    ModelItem = null;

                    Debug.Assert(string.IsNullOrEmpty(_modelItemColumnName) == false, "We are missing column name information");
                    Debug.Assert(MappingStorageEntityType != null, "We are missing our parent MappingStorageEntityType");
                    Debug.Assert(
                        MappingStorageEntityType != null && MappingStorageEntityType.StorageEntityType != null,
                        "Our parent MappingStorageEntityType is not mapped to a table");

                    if (string.IsNullOrEmpty(_modelItemColumnName) == false
                        && MappingStorageEntityType != null
                        && MappingStorageEntityType.StorageEntityType != null)
                    {
                        // the underlying EFObject was deleted, probably because it got moved to a different ETM
                        // go find it again
                        //
                        // Let's say that that you have a 3 part hierarchy, Parent (P), Child (C), GrandChild (GC).  
                        // GC has a condition on column X.  C has a condition on column Y.  Now the user goes and 
                        // adds a condition on column X to entity C.  This will cause C's ETM to change (since it now
                        // has a condition also used by a child entity) and the first condition is moved.  This 
                        // accessor will "reconnect" itself to the new model item.
                        //
                        EntityType table = MappingStorageEntityType.StorageEntityType;

                        var tableColumn = table.GetFirstNamedChildByLocalName(_modelItemColumnName) as Property;
                        Debug.Assert(tableColumn != null, "Failed looking up table column for Condition.");

                        if (tableColumn != null)
                        {
                            var condFound = ModelHelper.FindFragmentCondition(
                                MappingConceptualEntityType.ConceptualEntityType,
                                tableColumn);

                            Debug.Assert(
                                ((condFound == null) || (condFound != null && condFound.XObject != null)),
                                "The found Condition has a null XObject pointer");

                            if (condFound != null
                                &&
                                condFound.XObject != null)
                            {
                                ModelItem = condFound;
                            }
                        }
                    }
                }

                var cond = ModelItem as Condition;
                if (cond != null)
                {
                    Debug.Assert(cond.ColumnName.Status == BindingStatus.Known, "Why are we mapping an unresolved condition?");

                    // this MappingCondition was created based on an existing Condition,
                    // store off this information in case we need to re-attach our ModelItem later
                    if (cond.ColumnName.Status == BindingStatus.Known)
                    {
                        _modelItemColumnName = cond.ColumnName.Target.LocalName.Value;
                    }
                    else
                    {
                        _modelItemColumnName = cond.ColumnName.RefName;
                    }
                }

                return cond;
            }
        }

        internal override string Name
        {
            get
            {
                if (Condition != null)
                {
                    var isFirst = MappingStorageEntityType.Children.Count > 0
                                  && MappingStorageEntityType.Children[0] == this;

                    if (isFirst)
                    {
                        return string.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ConditionLine1, ColumnName);
                    }
                    else
                    {
                        return string.Format(CultureInfo.CurrentCulture, Resources.MappingDetails_ConditionLine2, ColumnName);
                    }
                }

                return string.Empty;
            }
        }

        internal string ColumnName
        {
            get
            {
                if (Condition != null)
                {
                    if (Condition.ColumnName.Status == BindingStatus.Known)
                    {
                        return Condition.ColumnName.Target.LocalName.Value;
                    }
                    else
                    {
                        return Condition.ColumnName.RefName;
                    }
                }

                return string.Empty;
            }
            set
            {
                var newColumnName = value;

                if (Condition != null
                    &&
                    string.IsNullOrEmpty(newColumnName) == false)
                {
                    // is it different than what we have already?
                    if (string.CompareOrdinal(ColumnName, newColumnName) != 0)
                    {
                        var tableColumn =
                            MappingStorageEntityType.StorageEntityType.GetFirstNamedChildByLocalName(newColumnName) as Property;
                        Debug.Assert(tableColumn != null, "tableColumn should not be null");

                        var cpc = new CommandProcessorContext(
                            Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeConditionColumn);
                        var cmd = new ChangeConditionColumnCommand(Condition, tableColumn);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
                else
                {
                    // if we don't have a condition, there is nothing to set this into;
                    // create the condition first
                    throw new InvalidOperationException();
                }
            }
        }

        internal MappingLovEFElement Operator
        {
            get
            {
                if (Condition != null)
                {
                    if (string.IsNullOrEmpty(Condition.IsNull.Value) == false)
                    {
                        return LovOperatorIsPlaceHolder;
                    }
                    else
                    {
                        return LovOperatorEqualsPlaceHolder;
                    }
                }

                return LovBlankPlaceHolder;
            }
            set
            {
                Debug.Assert(
                    value == LovOperatorIsPlaceHolder ||
                    value == LovOperatorEqualsPlaceHolder ||
                    value == LovValueNullPlaceHolder ||
                    value == LovValueNotNullPlaceHolder, "Operator only takes 4 possible values - you sent incorrect value " +
                                                         (value == null ? "NULL" : value.ToString()));

                var newOperator = value;

                if (Condition != null
                    || newOperator == LovBlankPlaceHolder)
                {
                    // is it different than what we have already?
                    if (Operator != newOperator)
                    {
                        bool? isNull = null;
                        var conditionValue = string.Empty;

                        if (newOperator == LovOperatorIsPlaceHolder)
                        {
                            // if are changing to an 'Is', then default the value column to false
                            isNull = false;
                        }
                        else
                        {
                            // if are changing to an '=', then default the value column to empty string
                            conditionValue = String.Empty;
                        }

                        var cpc = new CommandProcessorContext(
                            Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeConditionValue);
                        var cmd = new ChangeConditionPredicateCommand(Condition, isNull, conditionValue);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
                else
                {
                    // if we don't have a condition, there is nothing to set this into;
                    // create the condition first
                    throw new InvalidOperationException();
                }
            }
        }

        internal bool IsNullCondition
        {
            get
            {
                var isNullCondition = false;
                if (Condition != null)
                {
                    isNullCondition = String.IsNullOrEmpty(Condition.IsNull.Value) == false;
                }
                return isNullCondition;
            }
        }

        internal bool IsValueEmptyString
        {
            get
            {
                var isValueEmptyString = false;
                if (Condition != null)
                {
                    isValueEmptyString = !IsNullCondition && string.IsNullOrEmpty(Condition.Value.Value);
                }
                return isValueEmptyString;
            }
        }

        internal string Value
        {
            get
            {
                if (Condition != null)
                {
                    // is this an IsNull condition?
                    if (IsNullCondition)
                    {
                        // return the correct display value, "Null" or "Not Null"
                        if (string.CompareOrdinal(Condition.IsNull.Value, Condition.IsNullConstant) == 0)
                        {
                            return Resources.MappingDetails_ValueNull;
                        }
                        else
                        {
                            return Resources.MappingDetails_ValueNotNull;
                        }
                    }
                    else
                    {
                        // its a value condition, return the value if we have one
                        if (string.IsNullOrEmpty(Condition.Value.Value))
                        {
                            return Resources.MappingDetails_ValueEmptyString;
                        }
                        else
                        {
                            return Condition.Value.Value;
                        }
                    }
                }

                return string.Empty;
            }
            set
            {
                var newValue = value;

                if (Condition != null)
                {
                    // is it different than what we have already?
                    if (string.CompareOrdinal(Value, newValue) != 0 || IsValueEmptyString)
                    {
                        bool? isNull = null;
                        var conditionValue = string.Empty;

                        if (Operator == LovOperatorIsPlaceHolder)
                        {
                            // this is an "Is" condition, so set the correct boolean value
                            isNull = (string.CompareOrdinal(newValue, Resources.MappingDetails_ValueNull) == 0);
                        }
                        else
                        {
                            // this is an "=" condition
                            conditionValue = newValue;
                        }

                        var cpc = new CommandProcessorContext(
                            Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeConditionValue);
                        var cmd = new ChangeConditionPredicateCommand(Condition, isNull, conditionValue);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
                else
                {
                    // if we don't have a condition, there is nothing to set this into;
                    // create the condition first
                    throw new InvalidOperationException();
                }
            }
        }

        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            if (type == ListOfValuesCollection.FirstColumn)
            {
                var properties = new List<Property>();

                // fill our list initially with all columns
                properties.AddRange(MappingStorageEntityType.StorageEntityType.Properties());

                // filter the list down to those that aren't used in conditions. We don't filter based on properties
                // because it is useful in a TPH scenario to have the property mapped as well as used in a condition
                foreach (var child in MappingStorageEntityType.Children)
                {
                    var mc = child as MappingCondition;
                    Debug.Assert(mc != null, "expected child to be of type MappingCondition, instead got type " + child.GetType().FullName);
                    if (mc.Condition != null
                        &&
                        mc.Condition.ColumnName.Target != null
                        &&
                        properties.Contains(mc.Condition.ColumnName.Target))
                    {
                        properties.Remove(mc.Condition.ColumnName.Target);
                    }
                }

                if (Condition != null)
                {
                    // add the row at the top that the user can click on to remove the item
                    lov.Add(LovDeletePlaceHolder, LovDeletePlaceHolder.DisplayName);
                }

                if (properties.Count == 0)
                {
                    if (Condition == null)
                    {
                        lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
                    }
                }
                else
                {
                    // add those remaining in our list
                    foreach (var column in properties)
                    {
                        lov.Add(new MappingLovEFElement(column, column.LocalName.Value), column.LocalName.Value);
                    }
                }

                return lov;
            }
            else if (type == ListOfValuesCollection.SecondColumn)
            {
                lov.Add(LovOperatorIsPlaceHolder, LovOperatorIsPlaceHolder.DisplayName);
                lov.Add(LovOperatorEqualsPlaceHolder, LovOperatorEqualsPlaceHolder.DisplayName);
                return lov;
            }
            else if (type == ListOfValuesCollection.ThirdColumn)
            {
                lov.Add(LovValueNullPlaceHolder, LovValueNullPlaceHolder.DisplayName);
                lov.Add(LovValueNotNullPlaceHolder, LovValueNotNullPlaceHolder.DisplayName);
                return lov;
            }
            else
            {
                Debug.Fail("Unsupported lov type was sent");
            }

            return base.GetListOfValues(type);
        }

        internal override void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
            Debug.Assert(context != null, "context must not be null");
            Debug.Assert(Condition == null, "Don't call this method if we already have a ModelItem");
            Debug.Assert(MappingStorageEntityType.StorageEntityType != null, "The parent item isn't set up correctly");
            Debug.Assert(underlyingModelItem != null, "underlyingModelItem must not be null");

            var tableColumn = underlyingModelItem as Property;
            Debug.Assert(
                tableColumn != null, "underlyingModelItem must be of type Property, actual type = " + underlyingModelItem.GetType().FullName);

            // store this off in case we have recover the condition later (if it moves to another ETM on us)
            _modelItemColumnName = tableColumn.LocalName.Value;

            Context = context;

            // local shortcuts
            EntityType entityType = MappingConceptualEntityType.ConceptualEntityType;

            // create a context if we weren't passed one
            if (cpc == null)
            {
                cpc = new CommandProcessorContext(
                    Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_CreateCondition);
            }

            // use empty string as a default condition value
            var cmd = new CreateFragmentConditionCommand(entityType, tableColumn, null, String.Empty);

            // set up our post event to fix up the view model
            cmd.PostInvokeEvent += (o, eventsArgs) =>
                {
                    var cond = cmd.CreatedCondition;
                    Debug.Assert(cond != null, "cmd failed to create Condition");

                    // fix up our view model
                    ModelItem = cond;
                    Parent.AddChild(this);
                };

            try
            {
                // now make the change
                var cp = new CommandProcessor(cpc, cmd);
                cp.Invoke();
            }
            catch
            {
                ModelItem = null;
                Parent.RemoveChild(this);

                throw;
            }
        }

        // <summary>
        //     NOTE: We don't call this.Parent.RemoveChild(this) because this is always called from the MappingEFElement.Delete() method
        //     which will remove this item from the parent.
        // </summary>
        internal override void DeleteModelItem(CommandProcessorContext cpc)
        {
            Debug.Assert(ModelItem != null, "We are trying to delete a null ModelItem");
            if (IsModelItemDeleted() == false)
            {
                // create a context if we weren't passed one
                if (cpc == null)
                {
                    cpc = new CommandProcessorContext(
                        Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_DeleteCondition);
                }

                // use the item's delete command
                var deleteCommand = Condition.GetDeleteCommand();
                deleteCommand.PostInvokeEvent += (o, eventsArgs) => { ModelItem = null; };

                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteCommand);
            }

            // if IsModelItemDeleted == true, just make sure that it is null anyways
            ModelItem = null;
        }
    }
}
