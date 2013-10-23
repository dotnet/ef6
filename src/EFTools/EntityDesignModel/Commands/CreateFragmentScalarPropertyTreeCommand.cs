// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a ScalarProperty that lives on any level of a ComplexProperty tree inside the MappingFragment.
    ///     It will create all the ComplexProperties in the middle if necessary.
    /// </summary>
    internal class CreateFragmentScalarPropertyTreeCommand : Command
    {
        private readonly EntityType _conceptualEntityType;
        private readonly MappingFragment _mappingFragment;
        private readonly List<Property> _properties;
        private readonly Property _tableColumn;
        private ScalarProperty _createdProperty;

        private enum Mode
        {
            None,
            EntityType,
            MappingFragment
        }

        private readonly Mode _mode = Mode.None;

        internal CreateFragmentScalarPropertyTreeCommand(EntityType conceptualEntityType, List<Property> properties, Property tableColumn)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateTableColumn(tableColumn);
            Debug.Assert(properties.Count > 0, "Properties list should contain at least one element");

            _conceptualEntityType = conceptualEntityType;
            _properties = properties;
            _tableColumn = tableColumn;
            _mode = Mode.EntityType;
        }

        internal CreateFragmentScalarPropertyTreeCommand(MappingFragment mappingFragment, List<Property> properties, Property tableColumn)
        {
            CommandValidation.ValidateMappingFragment(mappingFragment);
            CommandValidation.ValidateTableColumn(tableColumn);
            Debug.Assert(properties.Count > 0, "Properties list should contain at least one element");

            _mappingFragment = mappingFragment;
            if (mappingFragment != null
                && mappingFragment.EntityTypeMapping != null)
            {
                _conceptualEntityType = mappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType;
            }
            _properties = properties;
            _tableColumn = tableColumn;
            _mode = Mode.MappingFragment;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(
                _mode == Mode.EntityType || _mode == Mode.MappingFragment, "Unknown mode set in CreateFragmentScalarPropertyTreeCommand");

            var cp = new CommandProcessor(cpc);
            CreateFragmentComplexPropertyCommand prereqCmd = null;
            for (var i = 0; i < _properties.Count; i++)
            {
                var property = _properties[i];
                var complexConceptualProperty = property as ComplexConceptualProperty;
                if (complexConceptualProperty != null)
                {
                    Debug.Assert(i < _properties.Count - 1, "Last property shouldn't be ComplexConceptualProperty");
                    CreateFragmentComplexPropertyCommand cmd = null;
                    if (prereqCmd == null)
                    {
                        if (_mode == Mode.EntityType)
                        {
                            cmd = new CreateFragmentComplexPropertyCommand(_conceptualEntityType, complexConceptualProperty, _tableColumn);
                        }
                        else
                        {
                            cmd = new CreateFragmentComplexPropertyCommand(_mappingFragment, complexConceptualProperty);
                        }
                    }
                    else
                    {
                        cmd = new CreateFragmentComplexPropertyCommand(prereqCmd, complexConceptualProperty);
                    }

                    prereqCmd = cmd;
                    cp.EnqueueCommand(cmd);
                }
                else
                {
                    Debug.Assert(i == _properties.Count - 1, "This should be the last property");
                    CreateFragmentScalarPropertyCommand cmd = null;
                    if (prereqCmd == null)
                    {
                        if (_mode == Mode.EntityType)
                        {
                            cmd = new CreateFragmentScalarPropertyCommand(_conceptualEntityType, property, _tableColumn);
                        }
                        else
                        {
                            cmd = new CreateFragmentScalarPropertyCommand(_mappingFragment, property, _tableColumn);
                        }
                    }
                    else
                    {
                        cmd = new CreateFragmentScalarPropertyCommand(prereqCmd, property, _tableColumn);
                    }

                    cp.EnqueueCommand(cmd);
                    cp.Invoke();
                    _createdProperty = cmd.ScalarProperty;
                    return;
                }
            }
        }

        internal ScalarProperty ScalarProperty
        {
            get { return _createdProperty; }
        }
    }
}
