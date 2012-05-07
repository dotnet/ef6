namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Basic Visitor Design Pattern support for ColumnMap hierarchy;
    /// 
    /// This visitor class will walk the entire hierarchy, but does not
    /// return results; it's useful for operations such as printing and
    /// searching.
    /// </summary>
    /// <typeparam name="TArgType"></typeparam>
    internal abstract class ColumnMapVisitor<TArgType>
    {
        #region visitor helpers

        /// <summary>
        /// Common List(ColumnMap) code
        /// </summary>
        /// <param name="columnMaps"></param>
        /// <param name="arg"></param>
        protected void VisitList<TListType>(TListType[] columnMaps, TArgType arg)
            where TListType : ColumnMap
        {
            foreach (var columnMap in columnMaps)
            {
                columnMap.Accept(this, arg);
            }
        }

        #endregion

        #region EntityIdentity handling

        protected void VisitEntityIdentity(EntityIdentity entityIdentity, TArgType arg)
        {
            var dei = entityIdentity as DiscriminatedEntityIdentity;
            if (null != dei)
            {
                VisitEntityIdentity(dei, arg);
            }
            else
            {
                VisitEntityIdentity((SimpleEntityIdentity)entityIdentity, arg);
            }
        }

        protected virtual void VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, TArgType arg)
        {
            entityIdentity.EntitySetColumnMap.Accept(this, arg);
            foreach (var columnMap in entityIdentity.Keys)
            {
                columnMap.Accept(this, arg);
            }
        }

        protected virtual void VisitEntityIdentity(SimpleEntityIdentity entityIdentity, TArgType arg)
        {
            foreach (var columnMap in entityIdentity.Keys)
            {
                columnMap.Accept(this, arg);
            }
        }

        #endregion

        #region Visitor methods

        internal virtual void Visit(ComplexTypeColumnMap columnMap, TArgType arg)
        {
            ColumnMap nullSentinel = columnMap.NullSentinel;
            if (null != nullSentinel)
            {
                nullSentinel.Accept(this, arg);
            }
            foreach (var p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(DiscriminatedCollectionColumnMap columnMap, TArgType arg)
        {
            columnMap.Discriminator.Accept(this, arg);
            foreach (var fk in columnMap.ForeignKeys)
            {
                fk.Accept(this, arg);
            }
            foreach (var k in columnMap.Keys)
            {
                k.Accept(this, arg);
            }
            columnMap.Element.Accept(this, arg);
        }

        internal virtual void Visit(EntityColumnMap columnMap, TArgType arg)
        {
            VisitEntityIdentity(columnMap.EntityIdentity, arg);
            foreach (var p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(SimplePolymorphicColumnMap columnMap, TArgType arg)
        {
            columnMap.TypeDiscriminator.Accept(this, arg);
            foreach (ColumnMap cm in columnMap.TypeChoices.Values)
            {
                cm.Accept(this, arg);
            }
            foreach (var p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TArgType arg)
        {
            foreach (var typeDiscriminator in columnMap.TypeDiscriminators)
            {
                typeDiscriminator.Accept(this, arg);
            }
            foreach (var typeColumnMap in columnMap.TypeChoices.Values)
            {
                typeColumnMap.Accept(this, arg);
            }
            foreach (var property in columnMap.Properties)
            {
                property.Accept(this, arg);
            }
        }

        internal virtual void Visit(RecordColumnMap columnMap, TArgType arg)
        {
            ColumnMap nullSentinel = columnMap.NullSentinel;
            if (null != nullSentinel)
            {
                nullSentinel.Accept(this, arg);
            }
            foreach (var p in columnMap.Properties)
            {
                p.Accept(this, arg);
            }
        }

        internal virtual void Visit(RefColumnMap columnMap, TArgType arg)
        {
            VisitEntityIdentity(columnMap.EntityIdentity, arg);
        }

        internal virtual void Visit(ScalarColumnMap columnMap, TArgType arg)
        {
        }

        internal virtual void Visit(SimpleCollectionColumnMap columnMap, TArgType arg)
        {
            foreach (var fk in columnMap.ForeignKeys)
            {
                fk.Accept(this, arg);
            }
            foreach (var k in columnMap.Keys)
            {
                k.Accept(this, arg);
            }
            columnMap.Element.Accept(this, arg);
        }

        internal virtual void Visit(VarRefColumnMap columnMap, TArgType arg)
        {
        }

        #endregion
    }
}
