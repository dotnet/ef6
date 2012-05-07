namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;

    internal class DiscriminatorMapInfo
    {
        internal EntityTypeBase RootEntityType;
        internal bool IncludesSubTypes;
        internal ExplicitDiscriminatorMap DiscriminatorMap;

        internal DiscriminatorMapInfo(EntityTypeBase rootEntityType, bool includesSubTypes, ExplicitDiscriminatorMap discriminatorMap)
        {
            RootEntityType = rootEntityType;
            IncludesSubTypes = includesSubTypes;
            DiscriminatorMap = discriminatorMap;
        }

        /// <summary>
        /// Merge the discriminatorMap info we just found with what we've already found.
        /// 
        /// In practice, if either the current or the new map is from an OfTypeOnly view, we
        /// have to avoid the optimizations.
        /// 
        /// If we have a new map that is a superset of the current map, then we can just swap
        /// the new map for the current one.
        /// 
        /// If the current map is tha super set of the new one ther's nothing to do.
        /// 
        /// (Of course, if neither has changed, then we really don't need to look)
        /// </summary>
        internal void Merge(EntityTypeBase neededRootEntityType, bool includesSubtypes, ExplicitDiscriminatorMap discriminatorMap)
        {
            // If what we've found doesn't exactly match what we are looking for we have more work to do
            if (RootEntityType != neededRootEntityType
                || IncludesSubTypes != includesSubtypes)
            {
                if (!IncludesSubTypes
                    || !includesSubtypes)
                {
                    // If either the original or the new map is from an of-type-only view we can't
                    // merge, we just have to not optimize this case.
                    DiscriminatorMap = null;
                }
                if (TypeSemantics.IsSubTypeOf(RootEntityType, neededRootEntityType))
                {
                    // we're asking for a super type of existing type, and what we had is a proper 
                    // subset of it -we can replace the existing item.
                    RootEntityType = neededRootEntityType;
                    DiscriminatorMap = discriminatorMap;
                }
                if (!TypeSemantics.IsSubTypeOf(neededRootEntityType, RootEntityType))
                {
                    // If either the original or the new map is from an of-type-only view we can't
                    // merge, we just have to not optimize this case.
                    DiscriminatorMap = null;
                }
            }
        }
    }
}