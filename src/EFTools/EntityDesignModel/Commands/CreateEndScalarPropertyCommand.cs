// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEndScalarPropertyCommand : Command
    {
        private readonly AssociationSetMapping _associationSetMapping;
        private readonly AssociationSetEnd _associationSetEnd;
        private EndProperty _endProperty;
        private Property _entityProperty;
        private Property _tableColumn;
        private bool _enforceConstraints;
        private ScalarProperty _created;

        #region Core Constructors

        private CreateEndScalarPropertyCommand(
            AssociationSetMapping associationSetMapping, AssociationSetEnd associationSetEnd, Property entityProperty, Property tableColumn,
            bool enforceConstraints)
        {
            Initialize(entityProperty, tableColumn, enforceConstraints);
            CommandValidation.ValidateAssociationSetMapping(associationSetMapping);
            CommandValidation.ValidateAssociationSetEnd(associationSetEnd);
            _associationSetMapping = associationSetMapping;
            _associationSetEnd = associationSetEnd;
        }

        /// <summary>
        ///     Creates a ScalarProperty in the given EndProperty.
        /// </summary>
        /// <param name="end">The EndProperty to place this ScalarProperty; cannot be null.</param>
        /// <param name="entityProperty">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        /// <param name="enforceConstraints">If true checks/updates conditions on association mappings and referential constraints</param>
        internal CreateEndScalarPropertyCommand(
            EndProperty endProperty, Property entityProperty, Property tableColumn, bool enforceConstraints)
        {
            Initialize(entityProperty, tableColumn, enforceConstraints);
            CommandValidation.ValidateEndProperty(endProperty);
            _endProperty = endProperty;
        }

        internal CreateEndScalarPropertyCommand(
            CreateEndPropertyCommand prereq, Property entityProperty, Property tableColumn, bool enforceConstraints)
        {
            Initialize(entityProperty, tableColumn, enforceConstraints);
            AddPreReqCommand(prereq);
        }

        #endregion

        /// <summary>
        ///     Creates an EndProperty and then creates the ScalarProperty in that End.
        /// </summary>
        /// <param name="associationSetMapping"></param>
        /// <param name="associationSetEnd"></param>
        /// <param name="entityProperty">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateEndScalarPropertyCommand(
            AssociationSetMapping associationSetMapping, AssociationSetEnd associationSetEnd, Property entityProperty, Property tableColumn)
            : this(associationSetMapping, associationSetEnd, entityProperty, tableColumn, true)
        {
        }

        /// <summary>
        ///     Creates a ScalarProperty in the given EndProperty.
        /// </summary>
        /// <param name="end">The EndProperty to place this ScalarProperty; cannot be null.</param>
        /// <param name="entityProperty">This must be a valid Property from the C-Model.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        /// <returns></returns>
        internal CreateEndScalarPropertyCommand(EndProperty endProperty, Property entityProperty, Property tableColumn)
            : this(endProperty, entityProperty, tableColumn, true)
        {
        }

        internal CreateEndScalarPropertyCommand(CreateEndPropertyCommand prereq, Property entityProperty, Property tableColumn)
            : this(prereq, entityProperty, tableColumn, true)
        {
        }

        private void Initialize(Property entityProperty, Property tableColumn, bool enforceConstraints)
        {
            CommandValidation.ValidateConceptualEntityProperty(entityProperty);
            CommandValidation.ValidateTableColumn(tableColumn);

            _entityProperty = entityProperty;
            _tableColumn = tableColumn;
            _enforceConstraints = enforceConstraints;
        }

        protected override void ProcessPreReqCommands()
        {
            // if we don't have an EndProperty, see if there is a prereq registered; don't try if we have the ASM and ASE
            // since this means that we should create an end later inside Invoke()
            if (_associationSetMapping == null
                && _associationSetEnd == null
                && _endProperty == null)
            {
                var prereq = GetPreReqCommand(CreateEndPropertyCommand.PrereqId) as CreateEndPropertyCommand;
                if (prereq != null)
                {
                    _endProperty = prereq.EndProperty;
                }

                Debug.Assert(_endProperty != null, "We didn't get a good EndProperty out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_endProperty == null)
            {
                var cmd = new CreateEndPropertyCommand(_associationSetMapping, _associationSetEnd);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                _endProperty = cmd.EndProperty;
            }

            Debug.Assert(_endProperty != null, "_endProperty should not be null");
            if (_endProperty == null)
            {
                throw new CannotLocateParentItemException();
            }

            var sp = new ScalarProperty(_endProperty, null);
            sp.Name.SetRefName(_entityProperty);
            sp.ColumnName.SetRefName(_tableColumn);
            _endProperty.AddScalarProperty(sp);

            XmlModelHelper.NormalizeAndResolve(sp);

            if (_enforceConstraints)
            {
                var asm = _endProperty.Parent as AssociationSetMapping;
                Debug.Assert(asm != null, "_endProperty parent is not an AssociationSetMapping");
                EnforceAssociationSetMappingRules.AddRule(cpc, asm);

                var assoc = asm.TypeName.Target;
                Debug.Assert(assoc != null, "_endProperty parent has a null Association");
                if (assoc != null)
                {
                    InferReferentialConstraints.AddRule(cpc, assoc);
                }
            }

            _created = sp;
        }

        internal ScalarProperty ScalarProperty
        {
            get { return _created; }
        }
    }
}
