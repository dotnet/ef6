// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    // <summary>
    // Represents an referential constraint on a relationship
    // </summary>
    internal sealed class ReferentialConstraint : SchemaElement
    {
        private const char KEY_DELIMITER = ' ';
        private ReferentialConstraintRoleElement _principalRole;
        private ReferentialConstraintRoleElement _dependentRole;

        // <summary>
        // construct a Referential constraint
        // </summary>
        public ReferentialConstraint(Relationship relationship)
            : base(relationship)
        {
        }

        // <summary>
        // Validate this referential constraint
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Validate()
        {
            base.Validate();
            _principalRole.Validate();
            _dependentRole.Validate();

            if (ReadyForFurtherValidation(_principalRole)
                && ReadyForFurtherValidation(_dependentRole))
            {
                // Validate the to end and from end of the referential constraint
                var principalRoleEnd = _principalRole.End;
                var dependentRoleEnd = _dependentRole.End;

                bool isPrinicipalRoleKeyProperty, isDependentRoleKeyProperty;
                bool areAllPrinicipalRolePropertiesNullable, areAllDependentRolePropertiesNullable;
                bool isDependentRolePropertiesSubsetofKeyProperties, isPrinicipalRolePropertiesSubsetofKeyProperties;
                bool isAnyPrinicipalRolePropertyNullable, isAnyDependentRolePropertyNullable;

                // Validate the role name to be different
                if (_principalRole.Name
                    == _dependentRole.Name)
                {
                    AddError(
                        ErrorCode.SameRoleReferredInReferentialConstraint,
                        EdmSchemaErrorSeverity.Error,
                        Strings.SameRoleReferredInReferentialConstraint(ParentElement.Name));
                }

                // Resolve all the property in the ToProperty attribute. Also checks whether this is nullable or not and 
                // whether the properties are the keys for the type in the ToRole
                IsKeyProperty(
                    _dependentRole, dependentRoleEnd.Type,
                    out isPrinicipalRoleKeyProperty,
                    out areAllDependentRolePropertiesNullable,
                    out isAnyDependentRolePropertyNullable,
                    out isDependentRolePropertiesSubsetofKeyProperties);

                // Resolve all the property in the ToProperty attribute. Also checks whether this is nullable or not and 
                // whether the properties are the keys for the type in the ToRole
                IsKeyProperty(
                    _principalRole, principalRoleEnd.Type,
                    out isDependentRoleKeyProperty,
                    out areAllPrinicipalRolePropertiesNullable,
                    out isAnyPrinicipalRolePropertyNullable,
                    out isPrinicipalRolePropertiesSubsetofKeyProperties);

                Debug.Assert(_principalRole.RoleProperties.Count != 0, "There should be some ref properties in Principal Role");
                Debug.Assert(_dependentRole.RoleProperties.Count != 0, "There should be some ref properties in Dependent Role");

                // The properties in the PrincipalRole must be the key of the Entity type referred to by the principal role
                if (!isDependentRoleKeyProperty)
                {
                    AddError(
                        ErrorCode.InvalidPropertyInRelationshipConstraint,
                        EdmSchemaErrorSeverity.Error,
                        Strings.InvalidFromPropertyInRelationshipConstraint(
                            PrincipalRole.Name, principalRoleEnd.Type.FQName, ParentElement.FQName));
                }
                else
                {
                    var v1Behavior = Schema.SchemaVersion <= XmlConstants.EdmVersionForV1_1;

                    // Determine expected multiplicities
                    var expectedPrincipalMultiplicity = (v1Behavior
                                                             ? areAllPrinicipalRolePropertiesNullable
                                                             : isAnyPrinicipalRolePropertyNullable)
                                                            ? RelationshipMultiplicity.ZeroOrOne
                                                            : RelationshipMultiplicity.One;
                    var expectedDependentMultiplicity = (v1Behavior
                                                             ? areAllDependentRolePropertiesNullable
                                                             : isAnyDependentRolePropertyNullable)
                                                            ? RelationshipMultiplicity.ZeroOrOne
                                                            : RelationshipMultiplicity.Many;
                    principalRoleEnd.Multiplicity = principalRoleEnd.Multiplicity ?? expectedPrincipalMultiplicity;
                    dependentRoleEnd.Multiplicity = dependentRoleEnd.Multiplicity ?? expectedDependentMultiplicity;

                    // Since the FromProperty must be the key of the FromRole, the FromRole cannot be '*' as multiplicity
                    // Also the lower bound of multiplicity of FromRole can be zero if and only if all the properties in 
                    // ToProperties are nullable
                    // for v2+
                    if (principalRoleEnd.Multiplicity
                        == RelationshipMultiplicity.Many)
                    {
                        AddError(
                            ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            Strings.InvalidMultiplicityFromRoleUpperBoundMustBeOne(_principalRole.Name, ParentElement.Name));
                    }
                    else if (areAllDependentRolePropertiesNullable
                             && principalRoleEnd.Multiplicity == RelationshipMultiplicity.One)
                    {
                        var message = Strings.InvalidMultiplicityFromRoleToPropertyNullableV1(_principalRole.Name, ParentElement.Name);
                        AddError(
                            ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            message);
                    }
                    else if ((
                                 (v1Behavior && !areAllDependentRolePropertiesNullable) ||
                                 (!v1Behavior && !isAnyDependentRolePropertyNullable)
                             )
                             && principalRoleEnd.Multiplicity != RelationshipMultiplicity.One)
                    {
                        string message;
                        if (v1Behavior)
                        {
                            message = Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV1(_principalRole.Name, ParentElement.Name);
                        }
                        else
                        {
                            message = Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV2(_principalRole.Name, ParentElement.Name);
                        }
                        AddError(
                            ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            message);
                    }

                    // If the ToProperties form the key of the type in ToRole, then the upper bound of the multiplicity 
                    // of the ToRole must be '1'. The lower bound must always be zero since there can be entries in the from
                    // column which are not related to child columns.
                    if (dependentRoleEnd.Multiplicity == RelationshipMultiplicity.One
                        && Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
                    {
                        AddError(
                            ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            Strings.InvalidMultiplicityToRoleLowerBoundMustBeZero(_dependentRole.Name, ParentElement.Name));
                    }

                    // Need to constrain the dependent role in CSDL to Key properties if this is not a IsForeignKey
                    // relationship.
                    if ((!isDependentRolePropertiesSubsetofKeyProperties)
                        &&
                        (!ParentElement.IsForeignKey)
                        &&
                        (Schema.DataModel == SchemaDataModelOption.EntityDataModel))
                    {
                        AddError(
                            ErrorCode.InvalidPropertyInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            Strings.InvalidToPropertyInRelationshipConstraint(
                                DependentRole.Name, dependentRoleEnd.Type.FQName, ParentElement.FQName));
                    }

                    // If the ToProperty is a key property, then the upper bound must be 1 i.e. every parent (from property) can 
                    // have exactly one child
                    if (isPrinicipalRoleKeyProperty)
                    {
                        if (dependentRoleEnd.Multiplicity
                            == RelationshipMultiplicity.Many)
                        {
                            AddError(
                                ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                EdmSchemaErrorSeverity.Error,
                                Strings.InvalidMultiplicityToRoleUpperBoundMustBeOne(dependentRoleEnd.Name, ParentElement.Name));
                        }
                    }
                    // if the ToProperty is not the key, then the upper bound must be many i.e every parent (from property) can
                    // be related to many childs
                    else if (dependentRoleEnd.Multiplicity
                             != RelationshipMultiplicity.Many)
                    {
                        AddError(
                            ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            Strings.InvalidMultiplicityToRoleUpperBoundMustBeMany(dependentRoleEnd.Name, ParentElement.Name));
                    }

                    if (_dependentRole.RoleProperties.Count
                        != _principalRole.RoleProperties.Count)
                    {
                        AddError(
                            ErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            Strings.MismatchNumberOfPropertiesinRelationshipConstraint);
                    }
                    else
                    {
                        for (var i = 0; i < _dependentRole.RoleProperties.Count; i++)
                        {
                            if (_dependentRole.RoleProperties[i].Property.Type
                                != _principalRole.RoleProperties[i].Property.Type)
                            {
                                AddError(
                                    ErrorCode.TypeMismatchRelationshipConstraint,
                                    EdmSchemaErrorSeverity.Error,
                                    Strings.TypeMismatchRelationshipConstraint(
                                        _dependentRole.RoleProperties[i].Name,
                                        _dependentRole.End.Type.Identity,
                                        _principalRole.RoleProperties[i].Name,
                                        _principalRole.End.Type.Identity,
                                        ParentElement.Name
                                        ));
                            }
                        }
                    }
                }
            }
        }

        private static bool ReadyForFurtherValidation(ReferentialConstraintRoleElement role)
        {
            if (role == null)
            {
                return false;
            }

            if (role.End == null)
            {
                return false;
            }

            if (role.RoleProperties.Count == 0)
            {
                return false;
            }

            foreach (var propRef in role.RoleProperties)
            {
                if (propRef.Property == null)
                {
                    return false;
                }
            }

            return true;
        }

        // <summary>
        // Resolves the given property names to the property in the item
        // Also checks whether the properties form the key for the given type and whether all the properties are nullable or not
        // </summary>
        private static void IsKeyProperty(
            ReferentialConstraintRoleElement roleElement, SchemaEntityType itemType,
            out bool isKeyProperty,
            out bool areAllPropertiesNullable,
            out bool isAnyPropertyNullable,
            out bool isSubsetOfKeyProperties)
        {
            isKeyProperty = true;
            areAllPropertiesNullable = true;
            isAnyPropertyNullable = false;
            isSubsetOfKeyProperties = true;

            if (itemType.KeyProperties.Count
                != roleElement.RoleProperties.Count)
            {
                isKeyProperty = false;
            }

            // Checking that ToProperties must be the key properties in the entity type referred by the ToRole
            for (var i = 0; i < roleElement.RoleProperties.Count; i++)
            {
                // Once we find that the properties in the constraint are not a subset of the
                // Key, one need not search for it every time
                if (isSubsetOfKeyProperties)
                {
                    var foundKeyProperty = false;

                    // All properties that are defined in ToProperties must be the key property on the entity type
                    for (var j = 0; j < itemType.KeyProperties.Count; j++)
                    {
                        if (itemType.KeyProperties[j].Property
                            == roleElement.RoleProperties[i].Property)
                        {
                            foundKeyProperty = true;
                            break;
                        }
                    }

                    if (!foundKeyProperty)
                    {
                        isKeyProperty = false;
                        isSubsetOfKeyProperties = false;
                    }
                }

                areAllPropertiesNullable &= roleElement.RoleProperties[i].Property.Nullable;
                isAnyPropertyNullable |= roleElement.RoleProperties[i].Property.Nullable;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            return false;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.PrincipalRole))
            {
                HandleReferentialConstraintPrincipalRoleElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.DependentRole))
            {
                HandleReferentialConstraintDependentRoleElement(reader);
                return true;
            }

            return false;
        }

        internal void HandleReferentialConstraintPrincipalRoleElement(XmlReader reader)
        {
            _principalRole = new ReferentialConstraintRoleElement(this);
            _principalRole.Parse(reader);
        }

        internal void HandleReferentialConstraintDependentRoleElement(XmlReader reader)
        {
            _dependentRole = new ReferentialConstraintRoleElement(this);
            _dependentRole.Parse(reader);
        }

        internal override void ResolveTopLevelNames()
        {
            _dependentRole.ResolveTopLevelNames();

            _principalRole.ResolveTopLevelNames();
        }

        // <summary>
        // The parent element as an IRelationship
        // </summary>
        internal new IRelationship ParentElement
        {
            get { return (IRelationship)(base.ParentElement); }
        }

        internal ReferentialConstraintRoleElement PrincipalRole
        {
            get { return _principalRole; }
        }

        internal ReferentialConstraintRoleElement DependentRole
        {
            get { return _dependentRole; }
        }
    }
}
