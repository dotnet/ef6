namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The VarInfo map maintains a mapping from Vars to their corresponding VarInfo
    /// It is logically a Dictionary
    /// </summary>
    internal class VarInfoMap
    {
        private readonly Dictionary<Var, VarInfo> m_map;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal VarInfoMap()
        {
            m_map = new Dictionary<Var, VarInfo>();
        }

        /// <summary>
        /// Create a new VarInfo for a structured type Var
        /// </summary>
        /// <param name="v">The structured type Var</param>
        /// <param name="newType">"Mapped" type for v</param>
        /// <param name="newVars">List of vars corresponding to v</param>
        /// <param name="newProperties">Flattened Properties </param>
        /// <param name="newVarsIncludeNullSentinelVar">Do the new vars include a var that represents a null sentinel either for this type or for any nested type</param>
        /// <returns>the VarInfo</returns>
        internal VarInfo CreateStructuredVarInfo(
            Var v, RowType newType, List<Var> newVars, List<EdmProperty> newProperties, bool newVarsIncludeNullSentinelVar)
        {
            VarInfo varInfo = new StructuredVarInfo(newType, newVars, newProperties, newVarsIncludeNullSentinelVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Create a new VarInfo for a structured type Var where the newVars cannot include a null sentinel
        /// </summary>
        /// <param name="v">The structured type Var</param>
        /// <param name="newType">"Mapped" type for v</param>
        /// <param name="newVars">List of vars corresponding to v</param>
        /// <param name="newProperties">Flattened Properties </param>
        internal VarInfo CreateStructuredVarInfo(Var v, RowType newType, List<Var> newVars, List<EdmProperty> newProperties)
        {
            return CreateStructuredVarInfo(v, newType, newVars, newProperties, false);
        }

        /// <summary>
        /// Create a VarInfo for a collection typed Var
        /// </summary>
        /// <param name="v">The collection-typed Var</param>
        /// <param name="newVar">the new Var</param>
        /// <returns>the VarInfo</returns>
        internal VarInfo CreateCollectionVarInfo(Var v, Var newVar)
        {
            VarInfo varInfo = new CollectionVarInfo(newVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Creates a var info for var variables of primitive or enum type.
        /// </summary>
        /// <param name="v">Current variable of primitive or enum type.</param>
        /// <param name="newVar">The new variable replacing <paramref name="v"/>.</param>
        /// <returns><see cref="PrimitiveTypeVarInfo"/> for <paramref name="v"/>.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal VarInfo CreatePrimitiveTypeVarInfo(Var v, Var newVar)
        {
            Debug.Assert(v != null, "v != null");
            Debug.Assert(newVar != null, "newVar != null");

            PlanCompiler.Assert(TypeSemantics.IsScalarType(v.Type), "The current variable should be of primitive or enum type.");
            PlanCompiler.Assert(TypeSemantics.IsScalarType(newVar.Type), "The new variable should be of primitive or enum type.");

            VarInfo varInfo = new PrimitiveTypeVarInfo(newVar);
            m_map.Add(v, varInfo);
            return varInfo;
        }

        /// <summary>
        /// Return the VarInfo for the specified var (if one exists, of course)
        /// </summary>
        /// <param name="v">The Var</param>
        /// <param name="varInfo">the corresponding VarInfo</param>
        /// <returns></returns>
        internal bool TryGetVarInfo(Var v, out VarInfo varInfo)
        {
            return m_map.TryGetValue(v, out varInfo);
        }
    }
}
