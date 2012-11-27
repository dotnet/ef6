// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    ///     Basic Visitor Design Pattern support for ColumnMap hierarchy;
    ///     This visitor class allows you to return results; it's useful for operations
    ///     that copy or manipulate the hierarchy.
    /// </summary>
    /// <typeparam name="TArgType"> </typeparam>
    /// <typeparam name="TResultType"> </typeparam>
    internal abstract class ColumnMapVisitorWithResults<TResultType, TArgType>
    {
        #region EntityIdentity handling

        protected EntityIdentity VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
        {
            var dei = entityIdentity as DiscriminatedEntityIdentity;
            if (null != dei)
            {
                return VisitEntityIdentity(dei, arg);
            }
            else
            {
                return VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
            }
        }

        protected virtual EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
        {
            return entityIdentity;
        }

        protected virtual EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
        {
            return entityIdentity;
        }

        #endregion

        #region Visitor methods

        internal abstract TResultType Visit(ComplexTypeColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(EntityColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(SimplePolymorphicColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(RecordColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(RefColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(ScalarColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(SimpleCollectionColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(VarRefColumnMap columnMap, TArgType arg);

        internal abstract TResultType Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg);

        #endregion
    }
}
