// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class InferReferentialConstraints : IIntegrityCheck
    {
        private readonly CommandProcessorContext _context;
        private readonly Association _association;

        internal InferReferentialConstraints(CommandProcessorContext context, Association association)
        {
            _context = context;
            _association = association;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as InferReferentialConstraints;
            if (typedOtherCheck != null
                && typedOtherCheck._association == _association)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This method will first determine whether a referential constraint is needed for the
        ///     passed in Association.  If yes, it will create one (or recreate as needed).  If no, it
        ///     will delete one if it exists.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="association">This is only valid for C-Side associations.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Invoke()
        {
            Debug.Assert(_association != null, "The Association reference is null");

            // if the association was deleted in this transaction, just return since we won't need to process it
            if (_association == null
                || _association.XObject == null)
            {
                return;
            }

            // if foreign keys are supported in this EF version, then we skip all processing here. 
            if (EdmFeatureManager.GetForeignKeysInModelFeatureState(_association.Artifact.SchemaVersion).IsEnabled())
            {
                return;
            }

            Debug.Assert(_association.EntityModel.IsCSDL, "Inferring ref constraints isn't valid for SSDL associations");
            Debug.Assert(
                _association.AssociationEnds().Count == 2,
                "The association to be processed does not have 2 ends while trying to infer ref constraints");
            Debug.Assert(
                _association.AssociationSet != null,
                "The association being processed does not have a valid AssociationSet while trying to infer ref constraints");

            // some local aliases for readability
            var end1 = _association.AssociationEnds()[0];
            var end2 = _association.AssociationEnds()[1];
            Debug.Assert(end1 != null && end2 != null, "Null end found while trying to infer ref constraints");
            if (end1 == null
                || end2 == null)
            {
                return;
            }

            // regardless, we will remove the constraint
            if (_association.ReferentialConstraint != null)
            {
                DeleteEFElementCommand.DeleteInTransaction(_context, _association.ReferentialConstraint);
            }

            // we will never create a constraint against a self-association
            if (end1.Type.Target == end2.Type.Target)
            {
                return;
            }

            AssociationEnd principal = null;
            AssociationEnd dependent = null;
            ModelHelper.DeterminePrincipalDependentAssociationEnds(
                _association, out principal, out dependent,
                ModelHelper.DeterminePrincipalDependentAssociationEndsScenario.InferReferentialConstraint);

            // We found our principal and dependent ends but we still need to confirm that
            // the AssociationSetMapping contains key properties that are mapped to the same column
            if (principal != null
                && principal.Type.Target != null
                && dependent != null
                && dependent.Type.Target != null)
            {
                var associationSet = _association.AssociationSet;
                if (associationSet != null)
                {
                    var asm = associationSet.AssociationSetMapping;
                    if (asm != null
                        && asm.EndProperties().Count == 2)
                    {
                        // any commonly mapped properties will be loaded into these HashSets
                        var principalPropertyRefs = new HashSet<Property>();
                        var dependentPropertyRefs = new HashSet<Property>();

                        EndProperty dependentEndProperty = null;
                        EndProperty principalEndProperty = null;
                        var endProp1 = asm.EndProperties()[0];
                        var endProp2 = asm.EndProperties()[1];
                        if (endProp1.Name.Target != null)
                        {
                            if (endProp1.Name.Target.Role.Target == dependent)
                            {
                                dependentEndProperty = endProp1;
                                principalEndProperty = endProp2;
                            }
                            else
                            {
                                dependentEndProperty = endProp2;
                                principalEndProperty = endProp1;
                            }
                        }

                        Debug.Assert(
                            dependentEndProperty != null && principalEndProperty != null,
                            "Either dependent or principal EndProperty is null");
                        if (dependentEndProperty != null
                            && principalEndProperty != null)
                        {
                            // for each column that is mapped to a key property on the dependent end, determine if there is a 
                            // key property on the principal end that it is also mapped to. If there is, then we need a
                            // ReferentialConstraint
                            foreach (var dependentScalarProp in dependentEndProperty.ScalarProperties())
                            {
                                var principalScalarProp =
                                    principalEndProperty.ScalarProperties()
                                    .FirstOrDefault(psp => psp.ColumnName.Target == dependentScalarProp.ColumnName.Target);
                                if (principalScalarProp != null)
                                {
                                    principalPropertyRefs.Add(principalScalarProp.Name.Target);
                                    dependentPropertyRefs.Add(dependentScalarProp.Name.Target);
                                }
                            }

                            Debug.Assert(
                                principalPropertyRefs.Count == dependentPropertyRefs.Count,
                                "List of keys are mismatched while trying to create a Ref Constraint");
                            if (principalPropertyRefs.Count > 0
                                && dependentPropertyRefs.Count > 0
                                && principalPropertyRefs.Count == dependentPropertyRefs.Count)
                            {
                                // if the propertyRefs sets have any data in them, add the constraint
                                var cmd = new CreateReferentialConstraintCommand(
                                    principal, dependent, principalPropertyRefs, dependentPropertyRefs);
                                CommandProcessor.InvokeSingleCommand(_context, cmd);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This method infers constraints for all associations that use this entity type.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="entityType">This is only valid for C-Side entities.</param>
        internal static void AddRule(CommandProcessorContext cpc, EntityType entityType)
        {
            Debug.Assert(entityType != null, "entityType should not be null");
            Debug.Assert(entityType.EntityModel.IsCSDL, "entityType should be from C-side");

            var processedAssociations = new HashSet<Association>();

            foreach (var end in entityType.GetAntiDependenciesOfType<AssociationEnd>())
            {
                var association = end.Parent as Association;
                Debug.Assert(association != null, "end.Parent should be an Association");
                if (association != null)
                {
                    if (processedAssociations.Contains(association) == false)
                    {
                        AddRule(cpc, association);
                        processedAssociations.Add(association);
                    }
                }
            }
        }

        /// <summary>
        ///     This method will add an InferReferentialConstraints IntegrityCheck for the passed in
        ///     association.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="association">This is only valid for C-Side associations.</param>
        internal static void AddRule(CommandProcessorContext cpc, Association association)
        {
            Debug.Assert(association != null, "association should not be null");
            Debug.Assert(association.EntityModel.IsCSDL, "association should be from C-side");
            Debug.Assert(
                association.AssociationEnds().Count == 2,
                "association.AssociationEnds().Count(" + association.AssociationEnds().Count + ") should be 2");
            Debug.Assert(association.AssociationSet != null, "association.AssociationSet should not be null");

            IIntegrityCheck check = new InferReferentialConstraints(cpc, association);
            cpc.AddIntegrityCheck(check);
        }
    }
}
