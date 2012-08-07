// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class FragmentQueryKB : KnowledgeBase<DomainConstraint<BoolLiteral, Constant>>
    {
        private BoolExpr<DomainConstraint<BoolLiteral, Constant>> _kbExpression = TrueExpr<DomainConstraint<BoolLiteral, Constant>>.Value;

        internal override void AddFact(BoolExpr<DomainConstraint<BoolLiteral, Constant>> fact)
        {
            base.AddFact(fact);
            _kbExpression = new AndExpr<DomainConstraint<BoolLiteral, Constant>>(_kbExpression, fact);
        }

        internal BoolExpr<DomainConstraint<BoolLiteral, Constant>> KbExpression
        {
            get { return _kbExpression; }
        }

        internal void CreateVariableConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            CreateVariableConstraintsRecursion(extent.ElementType, new MemberPath(extent), domainMap, edmItemCollection);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void CreateAssociationConstraints(EntitySetBase extent, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            var assocSet = extent as AssociationSet;
            if (assocSet != null)
            {
                var assocSetExpr = BoolExpression.CreateLiteral(new RoleBoolean(assocSet), domainMap);

                //Set of Keys for this Association Set
                //need to key on EdmMember and EdmType because A, B subtype of C, can have the same id (EdmMember) that is defined in C.
                var associationkeys = new HashSet<Pair<EdmMember, EntityType>>();

                //foreach end, add each Key
                foreach (var endMember in assocSet.ElementType.AssociationEndMembers)
                {
                    var type = (EntityType)((RefType)endMember.TypeUsage.EdmType).ElementType;
                    type.KeyMembers.All(
                        member => associationkeys.Add(new Pair<EdmMember, EntityType>(member, type)) || true /* prevent early termination */);
                }

                foreach (var end in assocSet.AssociationSetEnds)
                {
                    // construct type condition
                    var derivedTypes = new HashSet<EdmType>();
                    derivedTypes.UnionWith(
                        MetadataHelper.GetTypeAndSubtypesOf(
                            end.CorrespondingAssociationEndMember.TypeUsage.EdmType, edmItemCollection, false));

                    var typeCondition = CreateIsOfTypeCondition(
                        new MemberPath(end.EntitySet),
                        derivedTypes, domainMap);

                    var inRoleExpression = BoolExpression.CreateLiteral(new RoleBoolean(end), domainMap);
                    var inSetExpression = BoolExpression.CreateAnd(
                        BoolExpression.CreateLiteral(new RoleBoolean(end.EntitySet), domainMap),
                        typeCondition);

                    // InRole -> (InSet AND type(Set)=T)
                    AddImplication(inRoleExpression.Tree, inSetExpression.Tree);

                    if (MetadataHelper.IsEveryOtherEndAtLeastOne(assocSet, end.CorrespondingAssociationEndMember))
                    {
                        AddImplication(inSetExpression.Tree, inRoleExpression.Tree);
                    }

                    // Add equivalence between association set an End/Role if necessary.
                    //   Equivalence is added when a given association end's keys subsumes keys for
                    //   all the other association end.

                    // For example: We have Entity Sets A[id1], B[id2, id3] and an association A_B between them.
                    // Ref Constraint A.id1 = B.id2
                    // In this case, the Association Set has Key <id1, id2, id3>
                    // id1 alone can not identify a unique tuple in the Association Set, but <id2, id3> can.
                    // Therefore we add a constraint: InSet(B) <=> InEnd(A_B.B)

                    if (MetadataHelper.DoesEndKeySubsumeAssociationSetKey(
                        assocSet,
                        end.CorrespondingAssociationEndMember,
                        associationkeys))
                    {
                        AddEquivalence(inRoleExpression.Tree, assocSetExpr.Tree);
                    }
                }

                // add rules for referential constraints (borrowed from LeftCellWrapper.cs)
                var assocType = assocSet.ElementType;

                foreach (var constraint in assocType.ReferentialConstraints)
                {
                    var toEndMember = (AssociationEndMember)constraint.ToRole;
                    var toEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, toEndMember);
                    // Check if the keys of the entitySet's are equal to what is specified in the constraint
                    // How annoying that KeyMembers returns EdmMember and not EdmProperty
                    var toProperties = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(constraint.ToProperties);
                    if (Helpers.IsSetEqual(toProperties, toEntitySet.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default))
                    {
                        // Now check that the FromEnd is 1..1 (only then will all the Addresses be present in the assoc set)
                        if (constraint.FromRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One))
                        {
                            // Make sure that the ToEnd is not 0..* because then the schema is broken
                            Debug.Assert(constraint.ToRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many) == false);
                            // Equate the ends
                            var inRoleExpression1 = BoolExpression.CreateLiteral(new RoleBoolean(assocSet.AssociationSetEnds[0]), domainMap);
                            var inRoleExpression2 = BoolExpression.CreateLiteral(new RoleBoolean(assocSet.AssociationSetEnds[1]), domainMap);
                            AddEquivalence(inRoleExpression1.Tree, inRoleExpression2.Tree);
                        }
                    }
                }
            }
        }

        internal void CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(AssociationSet assocSet, MemberDomainMap domainMap)
        {
            var assocType = assocSet.ElementType;
            foreach (var constraint in assocType.ReferentialConstraints)
            {
                var toEndMember = (AssociationEndMember)constraint.ToRole;
                var fromEndMember = (AssociationEndMember)constraint.FromRole;
                var toEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, toEndMember);
                var fromEntitySet = MetadataHelper.GetEntitySetAtEnd(assocSet, fromEndMember);

                // Check if the keys of the entitySet's are equal to what is specified in the constraint
                var toProperties = Helpers.AsSuperTypeList<EdmProperty, EdmMember>(constraint.ToProperties);
                if (Helpers.IsSetEqual(toProperties, toEntitySet.ElementType.KeyMembers, EqualityComparer<EdmMember>.Default))
                {
                    //make sure that the method called with a 1:1 association
                    Debug.Assert(constraint.FromRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One));
                    Debug.Assert(constraint.ToRole.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One));
                    // Create an Equivalence between the two Sets participating in this AssociationSet
                    var fromSetExpression = BoolExpression.CreateLiteral(new RoleBoolean(fromEntitySet), domainMap);
                    var toSetExpression = BoolExpression.CreateLiteral(new RoleBoolean(toEntitySet), domainMap);
                    AddEquivalence(fromSetExpression.Tree, toSetExpression.Tree);
                }
            }
        }

        private void CreateVariableConstraintsRecursion(
            EdmType edmType, MemberPath currentPath, MemberDomainMap domainMap, EdmItemCollection edmItemCollection)
        {
            // Add the types can member have, i.e., its type and its subtypes
            var possibleTypes = new HashSet<EdmType>();
            possibleTypes.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(edmType, edmItemCollection, true));

            foreach (var possibleType in possibleTypes)
            {
                // determine type domain

                var derivedTypes = new HashSet<EdmType>();
                derivedTypes.UnionWith(MetadataHelper.GetTypeAndSubtypesOf(possibleType, edmItemCollection, false));
                if (derivedTypes.Count != 0)
                {
                    var typeCondition = CreateIsOfTypeCondition(currentPath, derivedTypes, domainMap);
                    var typeConditionComplement = BoolExpression.CreateNot(typeCondition);
                    if (false == typeConditionComplement.IsSatisfiable())
                    {
                        continue;
                    }

                    var structuralType = (StructuralType)possibleType;
                    foreach (var childProperty in structuralType.GetDeclaredOnlyMembers<EdmProperty>())
                    {
                        var childPath = new MemberPath(currentPath, childProperty);
                        var isScalar = MetadataHelper.IsNonRefSimpleMember(childProperty);

                        if (domainMap.IsConditionMember(childPath)
                            || domainMap.IsProjectedConditionMember(childPath))
                        {
                            BoolExpression nullCondition;
                            var childDomain = new List<Constant>(domainMap.GetDomain(childPath));
                            if (isScalar)
                            {
                                nullCondition = BoolExpression.CreateLiteral(
                                    new ScalarRestriction(
                                        new MemberProjectedSlot(childPath),
                                        new Domain(Constant.Undefined, childDomain)), domainMap);
                            }
                            else
                            {
                                nullCondition = BoolExpression.CreateLiteral(
                                    new TypeRestriction(
                                        new MemberProjectedSlot(childPath),
                                        new Domain(Constant.Undefined, childDomain)), domainMap);
                            }
                            // Properties not occuring in type are UNDEFINED
                            AddEquivalence(typeConditionComplement.Tree, nullCondition.Tree);
                        }

                        // recurse into complex types
                        if (false == isScalar)
                        {
                            CreateVariableConstraintsRecursion(childPath.EdmType, childPath, domainMap, edmItemCollection);
                        }
                    }
                }
            }
        }

        private static BoolExpression CreateIsOfTypeCondition(
            MemberPath currentPath, IEnumerable<EdmType> derivedTypes, MemberDomainMap domainMap)
        {
            var typeDomain = new Domain(
                derivedTypes.Select(derivedType => (Constant)new TypeConstant(derivedType)), domainMap.GetDomain(currentPath));
            var typeCondition = BoolExpression.CreateLiteral(
                new TypeRestriction(new MemberProjectedSlot(currentPath), typeDomain), domainMap);
            return typeCondition;
        }
    }
}
