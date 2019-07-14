// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a ComplexProperty that lives in a MappingFragment.apping.
    /// </summary>
    internal class CreateFragmentComplexPropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateFragmentComplexPropertyCommand";

        private EntityType _conceptualEntityType;
        private readonly MappingFragment _mappingFragment;
        private ComplexProperty _parentComplexProperty;
        private readonly ComplexConceptualProperty _property;
        private readonly Property _tableColumn;
        private ComplexProperty _createdProperty;

        private enum Mode
        {
            None,
            EntityType,
            MappingFragment,
            ComplexProperty
        }

        private readonly Mode _mode = Mode.None;

        /// <summary>
        ///     Creates a ComplexProperty in a MappingFragment based on conceptualEntityType passed in.
        /// </summary>
        /// <param name="conceptualEntityType">A C side entity</param>
        /// <param name="property">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateFragmentComplexPropertyCommand(
            EntityType conceptualEntityType, ComplexConceptualProperty property, Property tableColumn)
            : base(PrereqId)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateConceptualProperty(property);
            CommandValidation.ValidateTableColumn(tableColumn);

            _conceptualEntityType = conceptualEntityType;
            _property = property;
            _tableColumn = tableColumn;
            _mode = Mode.EntityType;
        }

        /// <summary>
        ///     Creates a ComplexProperty in the given MappingFragment.
        /// </summary>
        /// <param name="mappingFragment">The MappingFragment to place this ComplexProperty; cannot be null.</param>
        /// <param name="property">This must be a valid ComplexTypeProperty.</param>
        /// <param name="isPartial"></param>
        internal CreateFragmentComplexPropertyCommand(MappingFragment mappingFragment, ComplexConceptualProperty property)
            : base(PrereqId)
        {
            CommandValidation.ValidateMappingFragment(mappingFragment);
            CommandValidation.ValidateConceptualProperty(property);

            _mappingFragment = mappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                _conceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            _property = property;
            _mode = Mode.MappingFragment;
        }

        /// <summary>
        ///     Creates a ComplexProperty in the given ComplexProperty.
        /// </summary>
        /// <param name="complexProperty">The parent ComplexProperty to place this ComplexProperty; cannot be null.</param>
        /// <param name="property">This must be a valid ComplexTypeProperty.</param>
        /// <param name="isPartial"></param>
        internal CreateFragmentComplexPropertyCommand(ComplexProperty complexProperty, ComplexConceptualProperty property)
            : base(PrereqId)
        {
            CommandValidation.ValidateComplexProperty(complexProperty);
            CommandValidation.ValidateConceptualProperty(property);

            _parentComplexProperty = complexProperty;
            var mappingFragment = complexProperty.MappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                _conceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            _property = property;
            _mode = Mode.ComplexProperty;
        }

        /// <summary>
        ///     Creates a ComplexProperty using ComplexProperty from prereq command
        /// </summary>
        /// <param name="prereq"></param>
        /// <param name="property">This must be a valid ComplexConceptualProperty.</param>
        /// <param name="isPartial"></param>
        internal CreateFragmentComplexPropertyCommand(CreateFragmentComplexPropertyCommand prereq, ComplexConceptualProperty property)
            : base(PrereqId)
        {
            ValidatePrereqCommand(prereq);
            CommandValidation.ValidateConceptualProperty(property);

            _property = property;
            _mode = Mode.ComplexProperty;
            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (_mode == Mode.ComplexProperty
                && _parentComplexProperty == null)
            {
                var prereq = GetPreReqCommand(PrereqId) as CreateFragmentComplexPropertyCommand;
                if (prereq != null)
                {
                    _parentComplexProperty = prereq.ComplexProperty;
                    CommandValidation.ValidateComplexProperty(_parentComplexProperty);

                    var mappingFragment = _parentComplexProperty.MappingFragment;
                    if (mappingFragment != null
                        && mappingFragment.EntityTypeMapping != null)
                    {
                        _conceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
                    }
                }

                Debug.Assert(_parentComplexProperty != null, "We didn't get a good ComplexProperty out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(
                _mode == Mode.EntityType || _mode == Mode.MappingFragment || _mode == Mode.ComplexProperty,
                "Unknown mode set in CreateFragmentComplexPropertyCommand");

            if (_mode == Mode.EntityType)
            {
                // safety check, this should never be hit
                if (_conceptualEntityType == null
                    || _property == null
                    || _tableColumn == null)
                {
                    throw new ArgumentNullException();
                }

                _createdProperty = CreateComplexPropertyUsingEntity(
                    cpc,
                    _conceptualEntityType, _property, _tableColumn);
            }
            else if (_mode == Mode.ComplexProperty)
            {
                // safety check, this should never be hit
                if (_parentComplexProperty == null
                    || _property == null)
                {
                    throw new ArgumentNullException();
                }

                _createdProperty = CreateComplexPropertyUsingComplexProperty(_parentComplexProperty, _property);
            }
            else
            {
                // safety check, this should never be hit
                if (_mappingFragment == null
                    || _property == null)
                {
                    throw new ArgumentNullException();
                }

                _createdProperty = CreateComplexPropertyUsingFragment(_mappingFragment, _property);
            }
        }

        /// <summary>
        ///     Returns the ComplexProperty created by this command
        /// </summary>
        internal ComplexProperty ComplexProperty
        {
            get
            {
                // we store the return from the create call in _createdProperty, but there is a chance that
                // post-processing in an integrity check will have moved this complex property to another ETM,
                // if this is case, go find the new one
                if (_createdProperty != null
                    && _createdProperty.XObject == null)
                {
                    Debug.Assert(_conceptualEntityType != null, "_conceptualEntityType should not be null");
                    Debug.Assert(_property != null, "_property should not be null");

                    if (_conceptualEntityType != null
                        && _property != null)
                    {
                        var cp = ModelHelper.FindFragmentComplexProperty(_conceptualEntityType, _property);

                        Debug.Assert(cp != null && cp.XObject != null, "could not find underlying ComplexProperty");

                        if (cp != null
                            && cp.XObject != null)
                        {
                            _createdProperty = cp;
                        }
                    }
                }

                return _createdProperty;
            }
        }

        private static ComplexProperty CreateComplexPropertyUsingEntity(
            CommandProcessorContext cpc, EntityType conceptualEntityType, ComplexConceptualProperty property, Property tableColumn)
        {
            // the S-Side entity
            var storageEntityType = tableColumn.Parent as EntityType;
            Debug.Assert(storageEntityType != null, "tableColumn.Parent should be an EntityType");

            // get the fragment to use
            var mappingFragment = ModelHelper.FindMappingFragment(cpc, conceptualEntityType, tableColumn.EntityType, true);
            Debug.Assert(mappingFragment != null, "Failed to create the MappingFragment to house this ComplexProperty");
            if (mappingFragment == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // now go do the real work
            var cp = CreateComplexPropertyUsingFragment(mappingFragment, property);

            return cp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ComplexProperty CreateComplexPropertyUsingFragment(
            MappingFragment mappingFragment, ComplexConceptualProperty property)
        {
            // make sure that we don't already have one
            var cp = mappingFragment.FindComplexProperty(property);
            if (cp == null)
            {
                cp = CreateNewComplexProperty(mappingFragment, property);
                mappingFragment.AddComplexProperty(cp);
            }
            return cp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ComplexProperty CreateComplexPropertyUsingComplexProperty(
            ComplexProperty parentComplexProperty, ComplexConceptualProperty property)
        {
            // make sure that we don't already have one
            var cp = parentComplexProperty.FindComplexProperty(property);
            if (cp == null)
            {
                cp = CreateNewComplexProperty(parentComplexProperty, property);
                parentComplexProperty.AddComplexProperty(cp);
            }
            return cp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ComplexProperty CreateNewComplexProperty(EFElement parent, ComplexConceptualProperty property)
        {
            // actually create it in the XLinq tree
            var cp = new ComplexProperty(parent, null);
            cp.Name.SetRefName(property);

            XmlModelHelper.NormalizeAndResolve(cp);

            if (cp == null)
            {
                throw new ItemCreationFailureException();
            }

            Debug.Assert(cp.Name.Target != null && cp.Name.Target.LocalName.Value == cp.Name.RefName, "Broken property resolution");

            return cp;
        }
    }
}
