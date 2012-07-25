// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     Indicates which Database Metadata concept is represented by a given item.
    /// </summary>
    internal enum DbItemKind
    {
        /// <summary>
        ///     Database Kind
        /// </summary>
        Database = 0,

        /// <summary>
        ///     Schema Kind
        /// </summary>
        Schema = 1,

        /// <summary>
        ///     Foreign Key Constraint Kind
        /// </summary>
        ForeignKeyConstraint = 2,

        /// <summary>
        ///     Function Kind
        /// </summary>
        Function = 3,

        /// <summary>
        ///     Function Parameter Kind
        /// </summary>
        FunctionParameter = 4,

        /// <summary>
        ///     Function Return or Parameter Type Kind
        /// </summary>
        FunctionType = 5,

        /// <summary>
        ///     Row Column Kind
        /// </summary>
        RowColumn = 6,

        /// <summary>
        ///     Table Kind
        /// </summary>
        Table = 7,

        /// <summary>
        ///     Table Column Kind
        /// </summary>
        TableColumn = 8,

        /// <summary>
        ///     Primitive Facets Kind
        /// </summary>
        PrimitiveTypeFacets = 9,
    }
}
