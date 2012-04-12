namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The ColumnMapCopier clones an entire ColumnMap hierarchy; this is different
    /// than the ColumnMapTranslator, which only copies things that need to be copied.
    /// 
    /// Note that this is a stateless visitor; it uses the visitor's argument for its
    /// state management.
    /// 
    /// The Visitor's argument is a VarMap; anytime a Var is found in the ColumnMap 
    /// hierarchy, it is replaced with the replacement from the VarMap.
    /// 
    /// Note also that previous implementations of this class attempted to avoid re-
    /// processing ColumnMaps by caching the results for each input and returning it.
    /// I wasn't convinced that we were buying much with all that caching, since the 
    /// only ColumnMaps that should be repeated in the hierarchy are simple ones; there 
    /// is about as much object creation either way.  The only reason I see that we 
    /// want to cache these is if we really cared to have only one VarRefColumnMap 
    /// instance for a given Var and be able to use reference equality instead of
    /// comparing the Vars themselves.  I don't believe we're making that guarantee
    /// anywhere else, so I've removed that for now because I don't want the added 
    /// complexity that the caching adds.  If performance analysis indicates there is 
    /// a problem, we can considier addding the cache back in.
    /// </summary>
    internal class ColumnMapCopier : ColumnMapVisitorWithResults<ColumnMap, VarMap>
    {
        #region Constructors

        /// <summary>
        /// Singleton instance for the "public" methods to use;
        /// </summary>
        private static readonly ColumnMapCopier Instance = new ColumnMapCopier();

        /// <summary>
        /// Constructor; no one should use this.
        /// </summary>
        private ColumnMapCopier()
        {
        }

        #endregion

        #region "Public" surface area

        /// <summary>
        /// Return a copy of the column map, replacing all vars with the replacements
        /// found in the replacementVarMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal static ColumnMap Copy(ColumnMap columnMap, VarMap replacementVarMap)
        {
            return columnMap.Accept(Instance, replacementVarMap);
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Returns the var to use in the copy, either the original or the
        /// replacement.  Note that we will follow the chain of replacements, in
        /// case the replacement was also replaced.
        /// </summary>
        /// <param name="originalVar"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        private static Var GetReplacementVar(Var originalVar, VarMap replacementVarMap)
        {
            // SQLBUDT #478509: Follow the chain of mapped vars, don't
            //                  just stop at the first one
            var replacementVar = originalVar;

            while (replacementVarMap.TryGetValue(replacementVar, out originalVar))
            {
                if (originalVar == replacementVar)
                {
                    break;
                }
                replacementVar = originalVar;
            }
            return replacementVar;
        }

        #endregion

        #region Visitor Methods

        #region List handling

        /// <summary>
        /// Copies the List of ColumnMaps or SimpleColumnMaps
        /// </summary>
        /// <typeparam name="TListType"></typeparam>
        /// <param name="tList"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal TListType[] VisitList<TListType>(TListType[] tList, VarMap replacementVarMap)
            where TListType : ColumnMap
        {
            var newTList = new TListType[tList.Length];
            for (var i = 0; i < tList.Length; ++i)
            {
                newTList[i] = (TListType)tList[i].Accept(this, replacementVarMap);
            }
            return newTList;
        }

        #endregion

        #region EntityIdentity handling

        /// <summary>
        /// Copies the DiscriminatedEntityIdentity
        /// </summary>
        /// <param name="entityIdentity"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        protected override EntityIdentity VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, VarMap replacementVarMap)
        {
            var newEntitySetCol = (SimpleColumnMap)entityIdentity.EntitySetColumnMap.Accept(this, replacementVarMap);
            var newKeys = VisitList(entityIdentity.Keys, replacementVarMap);
            return new DiscriminatedEntityIdentity(newEntitySetCol, entityIdentity.EntitySetMap, newKeys);
        }

        /// <summary>
        /// Copies the SimpleEntityIdentity
        /// </summary>
        /// <param name="entityIdentity"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        protected override EntityIdentity VisitEntityIdentity(SimpleEntityIdentity entityIdentity, VarMap replacementVarMap)
        {
            var newKeys = VisitList(entityIdentity.Keys, replacementVarMap);
            return new SimpleEntityIdentity(entityIdentity.EntitySet, newKeys);
        }

        #endregion

        /// <summary>
        /// ComplexTypeColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(ComplexTypeColumnMap columnMap, VarMap replacementVarMap)
        {
            var newNullability = columnMap.NullSentinel;
            if (null != newNullability)
            {
                newNullability = (SimpleColumnMap)newNullability.Accept(this, replacementVarMap);
            }
            var fieldList = VisitList(columnMap.Properties, replacementVarMap);
            return new ComplexTypeColumnMap(columnMap.Type, columnMap.Name, fieldList, newNullability);
        }

        /// <summary>
        /// DiscriminatedCollectionColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(DiscriminatedCollectionColumnMap columnMap, VarMap replacementVarMap)
        {
            var newElementColumnMap = columnMap.Element.Accept(this, replacementVarMap);
            var newDiscriminator = (SimpleColumnMap)columnMap.Discriminator.Accept(this, replacementVarMap);
            var newKeys = VisitList(columnMap.Keys, replacementVarMap);
            var newForeignKeys = VisitList(columnMap.ForeignKeys, replacementVarMap);
            return new DiscriminatedCollectionColumnMap(
                columnMap.Type, columnMap.Name, newElementColumnMap, newKeys, newForeignKeys, newDiscriminator, columnMap.DiscriminatorValue);
        }

        /// <summary>
        /// EntityColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(EntityColumnMap columnMap, VarMap replacementVarMap)
        {
            var newEntityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, replacementVarMap);
            var fieldList = VisitList(columnMap.Properties, replacementVarMap);
            return new EntityColumnMap(columnMap.Type, columnMap.Name, fieldList, newEntityIdentity);
        }

        /// <summary>
        /// SimplePolymorphicColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(SimplePolymorphicColumnMap columnMap, VarMap replacementVarMap)
        {
            var newDiscriminator = (SimpleColumnMap)columnMap.TypeDiscriminator.Accept(this, replacementVarMap);

            var newTypeChoices = new Dictionary<object, TypedColumnMap>(columnMap.TypeChoices.Comparer);
            foreach (var kv in columnMap.TypeChoices)
            {
                var newMap = (TypedColumnMap)kv.Value.Accept(this, replacementVarMap);
                newTypeChoices[kv.Key] = newMap;
            }
            var newBaseFieldList = VisitList(columnMap.Properties, replacementVarMap);
            return new SimplePolymorphicColumnMap(columnMap.Type, columnMap.Name, newBaseFieldList, newDiscriminator, newTypeChoices);
        }

        /// <summary>
        /// MultipleDiscriminatorPolymorphicColumnMap
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ColumnMapCopier")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "MultipleDiscriminatorPolymorphicColumnMap")]
        internal override ColumnMap Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, VarMap replacementVarMap)
        {
            // At this time, we shouldn't ever see this type here; it's for SPROCS which don't use
            // the plan compiler.
            PlanCompiler.Assert(false, "unexpected MultipleDiscriminatorPolymorphicColumnMap in ColumnMapCopier");
            return null;
        }

        /// <summary>
        /// RecordColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(RecordColumnMap columnMap, VarMap replacementVarMap)
        {
            var newNullability = columnMap.NullSentinel;
            if (null != newNullability)
            {
                newNullability = (SimpleColumnMap)newNullability.Accept(this, replacementVarMap);
            }
            var fieldList = VisitList(columnMap.Properties, replacementVarMap);
            return new RecordColumnMap(columnMap.Type, columnMap.Name, fieldList, newNullability);
        }

        /// <summary>
        /// RefColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(RefColumnMap columnMap, VarMap replacementVarMap)
        {
            var newEntityIdentity = VisitEntityIdentity(columnMap.EntityIdentity, replacementVarMap);
            return new RefColumnMap(columnMap.Type, columnMap.Name, newEntityIdentity);
        }

        /// <summary>
        /// ScalarColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(ScalarColumnMap columnMap, VarMap replacementVarMap)
        {
            return new ScalarColumnMap(columnMap.Type, columnMap.Name, columnMap.CommandId, columnMap.ColumnPos);
        }

        /// <summary>
        /// SimpleCollectionColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(SimpleCollectionColumnMap columnMap, VarMap replacementVarMap)
        {
            var newElementColumnMap = columnMap.Element.Accept(this, replacementVarMap);
            var newKeys = VisitList(columnMap.Keys, replacementVarMap);
            var newForeignKeys = VisitList(columnMap.ForeignKeys, replacementVarMap);
            return new SimpleCollectionColumnMap(columnMap.Type, columnMap.Name, newElementColumnMap, newKeys, newForeignKeys);
        }

        /// <summary>
        /// VarRefColumnMap
        /// </summary>
        /// <param name="columnMap"></param>
        /// <param name="replacementVarMap"></param>
        /// <returns></returns>
        internal override ColumnMap Visit(VarRefColumnMap columnMap, VarMap replacementVarMap)
        {
            var replacementVar = GetReplacementVar(columnMap.Var, replacementVarMap);
            return new VarRefColumnMap(columnMap.Type, columnMap.Name, replacementVar);
        }

        #endregion
    }
}
