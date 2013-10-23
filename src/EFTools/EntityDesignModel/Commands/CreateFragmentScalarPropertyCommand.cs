// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a ScalarProperty that lives in a MappingFragment.  This is different
    ///     than those ScalarProperties that can be added to an EndProperty or Function mapping.
    ///     Example:
    ///     &lt;MappingFragment StoreEntitySet=&quot;RunMetric&quot;&gt;
    ///     &lt;ScalarProperty Name=&quot;id&quot; ColumnName=&quot;id&quot; /&gt;
    ///     &lt;/MappingFragment&gt;
    /// </summary>
    internal class CreateFragmentScalarPropertyCommand : Command
    {
        private readonly MappingFragment _mappingFragment;
        private ScalarProperty _sp;

        internal enum Mode
        {
            None,
            EntityType,
            MappingFragment,
            ComplexProperty
        }

        internal ComplexProperty ComplexProperty { get; set; }
        internal EntityType ConceptualEntityType { get; set; }
        internal Property Property { get; set; }
        internal Property TableColumn { get; set; }
        internal Mode ModeValue { get; set; }

        internal CreateFragmentScalarPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            ModeValue = Mode.None;
        }

        /// <summary>
        ///     Creates a ScalarProperty in a MappingFragment based on the two ends passed in.  This ScalarProperty
        ///     will always be created in the IsTypeOf ETM (key columns are also added to the Default ETM).
        /// </summary>
        /// <param name="conceptualEntityType">A C side entity</param>
        /// <param name="property">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateFragmentScalarPropertyCommand(EntityType conceptualEntityType, Property property, Property tableColumn)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateConceptualProperty(property);
            CommandValidation.ValidateTableColumn(tableColumn);

            ConceptualEntityType = conceptualEntityType;
            Property = property;
            TableColumn = tableColumn;
            ModeValue = Mode.EntityType;
        }

        /// <summary>
        ///     Creates a ScalarProperty in the given MappingFragment.
        /// </summary>
        /// <param name="mappingFragment">The MappingFragment to place this ScalarProperty; cannot be null.</param>
        /// <param name="property">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateFragmentScalarPropertyCommand(MappingFragment mappingFragment, Property property, Property tableColumn)
        {
            CommandValidation.ValidateMappingFragment(mappingFragment);
            CommandValidation.ValidateConceptualProperty(property);
            CommandValidation.ValidateTableColumn(tableColumn);

            _mappingFragment = mappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                ConceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            Property = property;
            TableColumn = tableColumn;

            ModeValue = Mode.MappingFragment;
        }

        /// <summary>
        ///     Creates a ScalarProperty in the given ComplexProperty.
        /// </summary>
        /// <param name="complexProperty">The ComplexProperty to place this ScalarProperty; cannot be null.</param>
        /// <param name="property">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateFragmentScalarPropertyCommand(ComplexProperty complexProperty, Property property, Property tableColumn)
        {
            CommandValidation.ValidateComplexProperty(complexProperty);
            CommandValidation.ValidateConceptualProperty(property);
            CommandValidation.ValidateTableColumn(tableColumn);

            ComplexProperty = complexProperty;
            var mappingFragment = complexProperty.MappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                ConceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            Property = property;
            TableColumn = tableColumn;

            ModeValue = Mode.ComplexProperty;
        }

        /// <summary>
        ///     Creates a ScalarProperty using ComplexProperty from prereq command
        /// </summary>
        /// <param name="prereq"></param>
        /// <param name="property">This must be a valid ComplexConceptualProperty.</param>
        internal CreateFragmentScalarPropertyCommand(CreateFragmentComplexPropertyCommand prereq, Property property, Property tableColumn)
        {
            ValidatePrereqCommand(prereq);
            CommandValidation.ValidateConceptualProperty(property);
            CommandValidation.ValidateTableColumn(tableColumn);

            Property = property;
            TableColumn = tableColumn;
            ModeValue = Mode.ComplexProperty;
            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (ModeValue == Mode.ComplexProperty
                && ComplexProperty == null)
            {
                var prereq = GetPreReqCommand(CreateFragmentComplexPropertyCommand.PrereqId) as CreateFragmentComplexPropertyCommand;
                if (prereq != null)
                {
                    ComplexProperty = prereq.ComplexProperty;
                    CommandValidation.ValidateComplexProperty(ComplexProperty);

                    var mappingFragment = ComplexProperty.MappingFragment;
                    if (mappingFragment != null
                        && mappingFragment.EntityTypeMapping != null)
                    {
                        ConceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
                    }
                }

                Debug.Assert(ComplexProperty != null, "We didn't get a good ComplexProperty out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(
                ModeValue == Mode.EntityType || ModeValue == Mode.MappingFragment || ModeValue == Mode.ComplexProperty,
                "Unknown mode set in CreateFragmentScalarPropertyCommand");

            if (ModeValue == Mode.EntityType)
            {
                // safety check, this should never be hit
                if (ConceptualEntityType == null
                    || Property == null
                    || TableColumn == null)
                {
                    throw new ArgumentNullException();
                }

                _sp = CreateScalarPropertyUsingEntity(
                    cpc,
                    ConceptualEntityType, Property, TableColumn);
            }
            else if (ModeValue == Mode.ComplexProperty)
            {
                // safety check, this should never be hit
                if (ComplexProperty == null
                    || Property == null
                    || TableColumn == null)
                {
                    throw new ArgumentNullException();
                }

                _sp = CreateScalarPropertyUsingComplexProperty(ComplexProperty, Property, TableColumn);
            }
            else
            {
                // safety check, this should never be hit
                if (_mappingFragment == null
                    || Property == null
                    || TableColumn == null)
                {
                    throw new ArgumentNullException();
                }

                _sp = CreateScalarPropertyUsingFragment(_mappingFragment, Property, TableColumn);
            }

            if (_sp.MappingFragment != null
                && _sp.MappingFragment.EntityTypeMapping != null
                && _sp.MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType != null)
            {
                PropagateViewKeysToStorageModel.AddRule(cpc, _sp.MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType);

                // Also add the integrity check to propagate the StoreGeneratedPattern value to the
                // S-side (may be altered by property being/not being mapped) unless we are part
                // of an Update Model txn in which case there is no need as the whole artifact has
                // this integrity check applied by UpdateModelFromDatabaseCommand
                if (EfiTransactionOriginator.UpdateModelFromDatabaseId != cpc.OriginatorId
                    && _sp.Name != null
                    && _sp.Name.Target != null)
                {
                    var cProp = _sp.Name.Target as ConceptualProperty;
                    Debug.Assert(
                        cProp != null,
                        " ScalarProperty should have Name target with type ConceptualProperty, instead got type "
                        + _sp.Name.Target.GetType().FullName);
                    if (cProp != null)
                    {
                        PropagateStoreGeneratedPatternToStorageModel.AddRule(cpc, cProp, true);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the ScalarProperty created by this command
        /// </summary>
        internal ScalarProperty ScalarProperty
        {
            get
            {
                // we store the return from the create call in _sp, but there is a chance that
                // post-processing in an integry check will have moved this scalar property to another ETM,
                // if this is case, go find the new one
                if (_sp != null
                    && _sp.XObject == null)
                {
                    Debug.Assert(ConceptualEntityType != null, "ConceptualEntityType should not be null");
                    Debug.Assert(TableColumn != null, "TableColumn should not be null");

                    if (ConceptualEntityType != null
                        && TableColumn != null)
                    {
                        var sp = ModelHelper.FindFragmentScalarProperty(
                            ConceptualEntityType,
                            TableColumn);

                        Debug.Assert(sp != null && sp.XObject != null, "could not find underlying ScalarProperty");
                        if (sp != null
                            && sp.XObject != null)
                        {
                            _sp = sp;
                        }
                    }
                }

                return _sp;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ScalarProperty CreateScalarPropertyUsingEntity(
            CommandProcessorContext cpc, EntityType conceptualEntityType, Property entityProperty, Property tableColumn)
        {
            // the S-Side entity
            var storageEntityType = tableColumn.Parent as EntityType;
            Debug.Assert(storageEntityType != null, "tableColumn.Parent should be an EntityType");

            // get the fragment to use
            var mappingFragment = ModelHelper.FindMappingFragment(cpc, conceptualEntityType, tableColumn.EntityType, true);
            Debug.Assert(mappingFragment != null, "Failed to create the MappingFragment to house this ScalarProperty");
            if (mappingFragment == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // now go do the real work
            var sp = CreateScalarPropertyUsingFragment(mappingFragment, entityProperty, tableColumn);

            // enforce our mapping rules
            EnforceEntitySetMappingRules.AddRule(cpc, sp);

            return sp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ScalarProperty CreateScalarPropertyUsingFragment(
            MappingFragment mappingFragment, Property entityProperty, Property tableColumn)
        {
            // make sure that we don't already have one
            var sp = mappingFragment.FindScalarProperty(entityProperty, tableColumn);
            if (sp == null)
            {
                sp = CreateNewScalarProperty(mappingFragment, entityProperty, tableColumn);
                mappingFragment.AddScalarProperty(sp);
            }
            return sp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ScalarProperty CreateScalarPropertyUsingComplexProperty(
            ComplexProperty complexProperty, Property entityProperty, Property tableColumn)
        {
            // make sure that we don't already have one
            var sp = complexProperty.FindScalarProperty(entityProperty, tableColumn);
            if (sp == null)
            {
                sp = CreateNewScalarProperty(complexProperty, entityProperty, tableColumn);
                complexProperty.AddScalarProperty(sp);
            }
            return sp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ScalarProperty CreateNewScalarProperty(EFElement parent, Property entityProperty, Property tableColumn)
        {
            // actually create it in the XLinq tree
            var sp = new ScalarProperty(parent, null);
            sp.Name.SetRefName(entityProperty);
            sp.ColumnName.SetRefName(tableColumn);

            XmlModelHelper.NormalizeAndResolve(sp);

            if (sp == null)
            {
                throw new ItemCreationFailureException();
            }

            Debug.Assert(sp.Name.Target != null && sp.Name.Target.LocalName.Value == sp.Name.RefName, "Broken entity property resolution");
            Debug.Assert(
                sp.ColumnName.Target != null && sp.ColumnName.Target.LocalName.Value == sp.ColumnName.RefName, "Broken column resolution");

            return sp;
        }
    }
}
