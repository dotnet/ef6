// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a Condition that lives in a MappingFragment.  This is different
    ///     than those conditions that can be created as children of an AssociationSetMapping.
    ///     Example:
    ///     &lt;MappingFragment StoreEntitySet=&quot;RunMetric&quot;&gt;
    ///     &lt;Condition ColumnName=&quot;runId&quot; IsNull=&quot;false&quot; /&gt;
    ///     &lt;/MappingFragment&gt;
    /// </summary>
    internal class CreateFragmentConditionCommand : Command
    {
        internal EntityType ConceptualEntityType { get; set; }
        internal MappingFragment MappingFragment { get; set; }
        internal Property StorageProperty { get; set; }
        private Condition _condition;
        internal bool? IsNull { get; set; }
        internal string ConditionValue { get; set; }

        internal enum ModeValues
        {
            None,
            EntityType,
            MappingFragment
        }

        internal ModeValues Mode { get; set; }

        internal CreateFragmentConditionCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            Mode = ModeValues.None;
        }

        /// <summary>
        ///     Creates a Condition based on the column passed in.
        ///     Valid combinations are:
        ///     1. Send true or false for isNull, and null for conditionValue
        ///     2. Send null for isNull, and a non-empty string for conditionValue
        ///     3. Send null for isNull, and null for conditionValue
        ///     You cannot send non-null values to both arguments.
        /// </summary>
        /// <param name="conceptualEntityType"></param>
        /// <param name="tableColumn"></param>
        /// <param name="isNull"></param>
        /// <param name="conditionValue"></param>
        internal CreateFragmentConditionCommand(EntityType conceptualEntityType, Property tableColumn, bool? isNull, string conditionValue)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateTableColumn(tableColumn);

            ConceptualEntityType = conceptualEntityType;
            StorageProperty = tableColumn;
            IsNull = isNull;
            ConditionValue = conditionValue;
            Mode = ModeValues.EntityType;
        }

        /// <summary>
        ///     Creates a Condition in the given MappingFragment.
        ///     Valid combinations are:
        ///     1. Send true or false for isNull, and null for conditionValue
        ///     2. Send null for isNull, and a non-empty string for conditionValue
        ///     3. Send null for isNull, and null for conditionValue
        ///     You cannot send non-null values to both arguments.
        /// </summary>
        /// <param name="mappingFragment">The MappingFragment to place this Condition; cannot be null.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        /// <param name="isNull"></param>
        /// <param name="conditionValue"></param>
        internal CreateFragmentConditionCommand(MappingFragment mappingFragment, Property tableColumn, bool? isNull, string conditionValue)
        {
            CommandValidation.ValidateMappingFragment(mappingFragment);
            CommandValidation.ValidateTableColumn(tableColumn);

            MappingFragment = mappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                ConceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            StorageProperty = tableColumn;
            IsNull = isNull;
            ConditionValue = conditionValue;
            Mode = ModeValues.MappingFragment;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "conceptualEntityType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "tableColumn")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "mappingFragment")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(
                Mode == ModeValues.EntityType || Mode == ModeValues.MappingFragment, "Unknown mode set in CreateFragmentConditionCommand");

            if (Mode == ModeValues.EntityType)
            {
                // safety check, this should never be hit
                if (ConceptualEntityType == null
                    || StorageProperty == null)
                {
                    throw new InvalidOperationException("InvokeInternal is called when _conceptualEntityType or _tableColumn is null");
                }

                _condition = CreateConditionUsingEntity(
                    cpc,
                    ConceptualEntityType, StorageProperty, IsNull, ConditionValue);
            }
            else
            {
                // safety check, this should never be hit
                if (MappingFragment == null
                    || StorageProperty == null)
                {
                    throw new InvalidOperationException("InvokeInternal is called when _mappingFragment or _tableColumn is null.");
                }

                _condition = CreateConditionUsingFragment(MappingFragment, StorageProperty, IsNull, ConditionValue);
            }
        }

        /// <summary>
        ///     Returns the Condition created by the command
        /// </summary>
        internal Condition CreatedCondition
        {
            get
            {
                // we store the return from the create call in _condition, but there is a chance that
                // post-processing in an integry check will have moved this condition to another ETM,
                // if this is case, go find the new one
                if (_condition != null
                    && _condition.XObject == null)
                {
                    Debug.Assert(ConceptualEntityType != null, "_conceptualEntityType should not be null");
                    Debug.Assert(StorageProperty != null, "_tableColumn should not be null");

                    if (ConceptualEntityType != null
                        && StorageProperty != null)
                    {
                        var cond = ModelHelper.FindFragmentCondition(
                            ConceptualEntityType,
                            StorageProperty);

                        Debug.Assert(cond != null && cond.XObject != null, "could not find underlying Condition");
                        if (cond != null
                            && cond.XObject != null)
                        {
                            _condition = cond;
                        }
                    }
                }

                return _condition;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Condition CreateConditionUsingEntity(
            CommandProcessorContext cpc, EntityType conceptualEntityType, Property tableColumn, bool? isNull, string conditionValue)
        {
            // first see if we have a Default ETM
            var mappingFragment = ModelHelper.FindMappingFragment(
                cpc, conceptualEntityType, tableColumn.EntityType, EntityTypeMappingKind.Default, false);
            if (mappingFragment == null)
            {
                // if we don't have a default, then find or create an IsTypeOf ETM to put this in
                mappingFragment = ModelHelper.FindMappingFragment(
                    cpc, conceptualEntityType, tableColumn.EntityType, EntityTypeMappingKind.IsTypeOf, true);
            }
            Debug.Assert(mappingFragment != null, "Failed to create the MappingFragment to house this Condition");
            if (mappingFragment == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // create the condition
            var cond = CreateConditionUsingFragment(mappingFragment, tableColumn, isNull, conditionValue);

            // see if any conditions need to move now
            EnforceEntitySetMappingRules.AddRule(cpc, cond);

            return cond;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Condition CreateConditionUsingFragment(
            MappingFragment mappingFragment, Property tableColumn, bool? isNull, string conditionValue)
        {
            var cond = mappingFragment.FindConditionForColumn(tableColumn);
            if (cond == null)
            {
                cond = new Condition(mappingFragment, null);
                cond.ColumnName.SetRefName(tableColumn);
                mappingFragment.AddCondition(cond);

                XmlModelHelper.NormalizeAndResolve(cond);
            }

            if (cond == null)
            {
                throw new ItemCreationFailureException();
            }

            ModelHelper.SetConditionPredicate(cond, isNull, conditionValue);

            return cond;
        }
    }
}
