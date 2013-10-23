// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateForeignKeyProperties : IIntegrityCheck
    {
        private readonly CommandProcessorContext _context;
        private readonly Association _association;

        internal CreateForeignKeyProperties(CommandProcessorContext context, Association association)
        {
            _context = context;
            _association = association;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as CreateForeignKeyProperties;
            if (typedOtherCheck != null
                && typedOtherCheck._association == _association)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This method will first determine the principal and dependent ends of the passed in Association.
        ///     It will create a property on the dependent end for each principal key property.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Invoke()
        {
            Debug.Assert(_association != null, "The Association reference is null");

            // if the association was deleted in this transaction, just return since we won't need to process it
            if (_association == null
                || _association.XObject == null)
            {
                return;
            }

            // if foreign keys aren't supported in this EF version, then we skip all processing here. 
            if (!EdmFeatureManager.GetForeignKeysInModelFeatureState(_association.Artifact.SchemaVersion).IsEnabled())
            {
                return;
            }

            Debug.Assert(_association.EntityModel.IsCSDL, "Creating foreign key properties isn't valid for SSDL associations");
            Debug.Assert(
                _association.AssociationEnds().Count == 2,
                "The association to be processed does not have 2 ends while trying to create foreign key properties");
            Debug.Assert(
                _association.AssociationSet != null,
                "The association being processed does not have a valid AssociationSet while trying to create foreign key properties");

            // remove any existing RC
            if (_association.ReferentialConstraint != null)
            {
                DeleteEFElementCommand.DeleteInTransaction(_context, _association.ReferentialConstraint);
            }

            // figure out the principal and dependent ends
            AssociationEnd principal = null;
            AssociationEnd dependent = null;
            ModelHelper.DeterminePrincipalDependentAssociationEnds(
                _association, out principal, out dependent,
                ModelHelper.DeterminePrincipalDependentAssociationEndsScenario.CreateForeignKeyProperties);

            if (principal != null
                && principal.Type.Target != null
                && dependent != null
                && dependent.Type.Target != null)
            {
                // many-to-many associations don't need foreign key properties
                if (principal.Multiplicity.Value == ModelConstants.Multiplicity_Many
                    && dependent.Multiplicity.Value == ModelConstants.Multiplicity_Many)
                {
                    return;
                }

                var principalPropertyRefs = new HashSet<Property>();
                var dependentPropertyRefs = new HashSet<Property>();

                // add properties to the dependent side

                IEnumerable<Property> pkeys;
                var cet = principal.Type.Target as ConceptualEntityType;
                if (cet != null)
                {
                    // the principal is a c-side entity
                    pkeys = cet.ResolvableTopMostBaseType.ResolvableKeys;
                }
                else
                {
                    // the principal is an s-side entity
                    pkeys = principal.Type.Target.ResolvableKeys;
                }

                foreach (var pkey in pkeys)
                {
                    // build up the foreign key name, add an '_' if the resulting name wouldn't be camel-case
                    // e.g. 
                    //  Order and Id become "OrderId"
                    //  order and id become "order_id"
                    //
                    // get a unique name for this new property
                    var fkeyName = string.Format(
                        CultureInfo.CurrentCulture, "{0}{1}{2}",
                        principal.Type.Target.LocalName.Value,
                        (char.IsUpper(pkey.LocalName.Value, 0) ? "" : "_"),
                        pkey.LocalName.Value);
                    fkeyName = ModelHelper.GetUniqueName(typeof(Property), dependent.Type.Target, fkeyName);

                    // tweak the properties; we are using the copy/paste process since we have to 
                    // copy all facets of the pk and that code does this already
                    var pcf = new PropertyClipboardFormat(pkey);
                    pcf.PropertyName = fkeyName;
                    pcf.IsKeyProperty = false;
                    pcf.IsNullable = (principal.Multiplicity.Value == ModelConstants.Multiplicity_ZeroOrOne ? true : false);
                    pcf.StoreGeneratedPattern = string.Empty;
                    pcf.GetterAccessModifier = string.Empty;
                    pcf.SetterAccessModifier = string.Empty;

                    // create the new property
                    var cmd = new CopyPropertyCommand(pcf, dependent.Type.Target);
                    CommandProcessor.InvokeSingleCommand(_context, cmd);
                    var fkey = cmd.Property;

                    // build up our list of keys
                    Debug.Assert(fkey != null, "CreateForeignKeyProperties was not able to create a foreign key");
                    if (fkey != null)
                    {
                        principalPropertyRefs.Add(pkey);
                        dependentPropertyRefs.Add(fkey);
                    }
                }

                // create the new RC
                Debug.Assert(
                    principalPropertyRefs.Count == dependentPropertyRefs.Count,
                    "List of keys are mismatched while trying to create a Ref Constraint");
                if (principalPropertyRefs.Count > 0
                    && dependentPropertyRefs.Count > 0
                    && principalPropertyRefs.Count == dependentPropertyRefs.Count)
                {
                    var cmd = new CreateReferentialConstraintCommand(principal, dependent, principalPropertyRefs, dependentPropertyRefs);
                    CommandProcessor.InvokeSingleCommand(_context, cmd);
                }
            }
        }

        /// <summary>
        ///     This method will add an CreateForeignKeyProperties IntegrityCheck for the passed in
        ///     association.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="association">This is only valid for C-Side associations.</param>
        internal static void AddRule(CommandProcessorContext cpc, Association association)
        {
            Debug.Assert(association != null, "association should not be null");
            Debug.Assert(association.EntityModel.IsCSDL, "association should not be from C-side");
            Debug.Assert(
                association.AssociationEnds().Count == 2,
                "association.AssociationEnds().Count(" + association.AssociationEnds().Count + ") should be 2");
            Debug.Assert(association.AssociationSet != null, "association.AssociationSet should not be null");

            IIntegrityCheck check = new CreateForeignKeyProperties(cpc, association);
            cpc.AddIntegrityCheck(check);
        }
    }
}
