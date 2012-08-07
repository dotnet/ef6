// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     List of all the built in types
    /// </summary>
    public enum BuiltInTypeKind
    {
        /// <summary>
        ///     Association Type Kind
        /// </summary>
        AssociationEndMember = 0,

        /// <summary>
        ///     AssociationSetEnd Kind
        /// </summary>
        AssociationSetEnd,

        /// <summary>
        ///     AssociationSet Kind
        /// </summary>
        AssociationSet,

        /// <summary>
        ///     Association Type Kind
        /// </summary>
        AssociationType,

        /// <summary>
        ///     EntitySetBase Kind
        /// </summary>
        EntitySetBase,

        /// <summary>
        ///     Entity Type Base Kind
        /// </summary>
        EntityTypeBase,

        /// <summary>
        ///     Collection Type Kind
        /// </summary>
        CollectionType,

        /// <summary>
        ///     Collection Kind
        /// </summary>
        CollectionKind,

        /// <summary>
        ///     Complex Type Kind
        /// </summary>
        ComplexType,

        /// <summary>
        ///     Documentation Kind
        /// </summary>
        Documentation,

        /// <summary>
        ///     DeleteAction Type Kind
        /// </summary>
        OperationAction,

        /// <summary>
        ///     Edm Type Kind
        /// </summary>
        EdmType,

        /// <summary>
        ///     Entity Container Kind
        /// </summary>
        EntityContainer,

        /// <summary>
        ///     Entity Set Kind
        /// </summary>
        EntitySet,

        /// <summary>
        ///     Entity Type Kind
        /// </summary>
        EntityType,

        /// <summary>
        ///     Enumeration Type Kind
        /// </summary>
        EnumType,

        /// <summary>
        ///     Enum Member Kind
        /// </summary>
        EnumMember,

        /// <summary>
        ///     Facet Kind
        /// </summary>
        Facet,

        /// <summary>
        ///     EdmFunction Kind
        /// </summary>
        EdmFunction,

        /// <summary>
        ///     Function Parameter Kind
        /// </summary>
        FunctionParameter,

        /// <summary>
        ///     Global Item Type Kind
        /// </summary>
        GlobalItem,

        /// <summary>
        ///     Metadata Property Kind
        /// </summary>
        MetadataProperty,

        /// <summary>
        ///     Navigation Property Kind
        /// </summary>
        NavigationProperty,

        /// <summary>
        ///     Metadata Item Type Kind
        /// </summary>
        MetadataItem,

        /// <summary>
        ///     EdmMember Type Kind
        /// </summary>
        EdmMember,

        /// <summary>
        ///     Parameter Mode Kind
        /// </summary>
        ParameterMode,

        /// <summary>
        ///     Primitive Type Kind
        /// </summary>
        PrimitiveType,

        /// <summary>
        ///     Primitive Type Kind Kind
        /// </summary>
        PrimitiveTypeKind,

        /// <summary>
        ///     EdmProperty Type Kind
        /// </summary>
        EdmProperty,

        /// <summary>
        ///     ProviderManifest Type Kind
        /// </summary>
        ProviderManifest,

        /// <summary>
        ///     Referential Constraint Type Kind
        /// </summary>
        ReferentialConstraint,

        /// <summary>
        ///     Ref Type Kind
        /// </summary>
        RefType,

        /// <summary>
        ///     RelationshipEnd Type Kind
        /// </summary>
        RelationshipEndMember,

        /// <summary>
        ///     Relationship Multiplicity Type Kind
        /// </summary>
        RelationshipMultiplicity,

        /// <summary>
        ///     Relationship Set Type Kind
        /// </summary>
        RelationshipSet,

        /// <summary>
        ///     Relationship Type
        /// </summary>
        RelationshipType,

        /// <summary>
        ///     Row Type Kind
        /// </summary>
        RowType,

        /// <summary>
        ///     Simple Type Kind
        /// </summary>
        SimpleType,

        /// <summary>
        ///     Structural Type Kind
        /// </summary>
        StructuralType,

        /// <summary>
        ///     Type Information Kind
        /// </summary>
        TypeUsage,

        //
        //If you add anything below this, make sure you update the variable NumBuiltInTypes in EdmConstants
        //
    }
}
