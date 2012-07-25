// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     Indicates which Entity Data Model (EDM) concept is represented by a given item.
    /// </summary>
    internal enum EdmItemKind
    {
        /// <summary>
        ///     Association End Kind
        /// </summary>
        AssociationEnd = 0,

        /// <summary>
        ///     Association Set Kind
        /// </summary>
        AssociationSet = 1,

        /// <summary>
        ///     Association Type Kind
        /// </summary>
        AssociationType = 2,

        /// <summary>
        ///     Collection Type Kind
        /// </summary>
        CollectionType = 3,

        /// <summary>
        ///     Complex Type Kind
        /// </summary>
        ComplexType = 4,

        /// <summary>
        ///     Entity Container Kind
        /// </summary>
        EntityContainer = 5,

        /// <summary>
        ///     Entity Set Kind
        /// </summary>
        EntitySet = 6,

        /// <summary>
        ///     Entity Type Kind
        /// </summary>
        EntityType = 7,

        /// <summary>
        ///     Function Group Kind
        /// </summary>
        FunctionGroup = 8,

        /// <summary>
        ///     Function Overload Kind
        /// </summary>
        FunctionOverload = 9,

        /// <summary>
        ///     Function Import Kind
        /// </summary>
        FunctionImport = 10,

        /// <summary>
        ///     Function Parameter Kind
        /// </summary>
        FunctionParameter = 11,

        /// <summary>
        ///     Navigation Property Kind
        /// </summary>
        NavigationProperty = 12,

        /// <summary>
        ///     EdmProperty Type Kind
        /// </summary>
        Property = 13,

        /// <summary>
        ///     Association Constraint Type Kind
        /// </summary>
        AssociationConstraint = 14,

        /// <summary>
        ///     Ref Type Kind
        /// </summary>
        RefType = 15,

        /// <summary>
        ///     Row Column Kind
        /// </summary>
        RowColumn = 16,

        /// <summary>
        ///     Row Type Kind
        /// </summary>
        RowType = 17,

        /// <summary>
        ///     Type Reference Kind
        /// </summary>
        TypeReference = 18,

        /// <summary>
        ///     Model Kind
        /// </summary>
        Model = 19,

        /// <summary>
        ///     Namespace Kind
        /// </summary>
        Namespace = 20,

        /// <summary>
        ///     Primitive Facets Kind
        /// </summary>
        PrimitiveFacets = 21,

        /// <summary>
        ///     Primitive Type Kind
        /// </summary>
        PrimitiveType = 22,

        /// <summary>
        ///     Enum Type Kind
        /// </summary>
        EnumType = 23,

        /// <summary>
        ///     Enum Type Member Kind
        /// </summary>
        EnumTypeMember = 24
    }
}
