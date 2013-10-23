// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    /// <summary>
    ///     This class lets you change aspects of a Navigation Property.
    /// </summary>
    internal class ChangeNavigationPropertyCommand : Command
    {
        private readonly NavigationProperty _property;
        private readonly Association _association;
        private AssociationEnd _end1;
        private AssociationEnd _end2;
        private string _multiplicity;

        /// <summary>
        ///     Creates a command of that can change a Navigation Property
        /// </summary>
        /// <param name="association">The association to use.</param>
        /// <param name="property">The navigation property to change</param>
        internal ChangeNavigationPropertyCommand(NavigationProperty property, Association association)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            _property = property;
            _association = association;
        }

        /// <summary>
        ///     Creates a command of that can change a Navigation Property
        /// </summary>
        /// <param name="property">The navigation property to change</param>
        /// <param name="association">The association to use.</param>
        /// <param name="multiplicity">Changes the end's multiplicity, this may end up adding or removing conditions on any AssociationSetMappings</param>
        internal ChangeNavigationPropertyCommand(NavigationProperty property, Association association, string multiplicity)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (association == null)
            {
                throw new ArgumentNullException("association");
            }
            if (string.IsNullOrEmpty(multiplicity))
            {
                throw new ArgumentNullException("multiplicity");
            }

            _property = property;
            _association = association;
            _multiplicity = multiplicity;
        }

        /// <summary>
        ///     Creates a command of that can swap a Navigation Property association ends.
        /// </summary>
        /// <param name="property">The navigation property to change</param>
        /// <param name="association">The association to use.</param>
        /// <param name="end1">The first point to swap</param>
        /// <param name="end2">The second point to swap</param>
        internal ChangeNavigationPropertyCommand(
            NavigationProperty property, Association association, AssociationEnd end1, AssociationEnd end2)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (association == null)
            {
                throw new ArgumentNullException("association");
            }
            if (end1 == null)
            {
                throw new ArgumentNullException("end1");
            }
            if (end2 == null)
            {
                throw new ArgumentNullException("end2");
            }

            _property = property;
            _association = association;
            _end1 = end1;
            _end2 = end2;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            _property.Relationship.SetRefName(_association);
            _property.Relationship.Rebind();
            Debug.Assert(
                (_property.Relationship.Status == BindingStatus.Known
                 || (_association == null && _property.Relationship.Status == BindingStatus.Undefined)),
                "Rebind for the NavigationProperty failed");

            if (_association != null)
            {
                if (_end1 == null
                    || _end2 == null)
                {
                    var associationEnds = _association.AssociationEnds();
                    Debug.Assert(associationEnds.Count < 3, "AssociationEnds are >= 3");
                    _end1 =
                        associationEnds.Where(
                            r => r.Type != null && r.Type.Status == BindingStatus.Known && r.Type.Target == _property.Parent)
                            .FirstOrDefault();
                    _end2 = associationEnds.Where(
                        r => r.Type != null && r.Type.Status == BindingStatus.Known
                             && ((r.Type.Target != _property.Parent) ||
                                 (r.Type.Target == _property.Parent && r != _end1))).FirstOrDefault();
                }

                // updating association's multiplicity value as opposed to multiplicity itself.
                if (string.IsNullOrEmpty(_multiplicity)
                    && _end2 != null)
                {
                    _multiplicity = _end2.Multiplicity.Value;
                }
            }
            else
            {
                // resets end points when we get a null association
                _end1 = null;
                _end2 = null;
            }

            // rebinds association properties.
            _property.FromRole.SetRefName(_end1);
            _property.FromRole.Rebind();
            _property.ToRole.SetRefName(_end2);
            _property.ToRole.Rebind();

            if (_property.ToRole.Status == BindingStatus.Known
                && string.Compare(_property.ToRole.Target.Multiplicity.Value, _multiplicity, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (_property.Relationship.Status == BindingStatus.Known)
                {
                    var association = _property.Relationship.Target;

                    _property.ToRole.Target.Multiplicity.Value = _multiplicity;

                    if (association != null
                        && association.AssociationSet != null
                        && association.AssociationSet.AssociationSetMapping != null)
                    {
                        EnforceAssociationSetMappingRules.AddRule(cpc, association.AssociationSet.AssociationSetMapping);
                    }
                }
            }
        }
    }
}
