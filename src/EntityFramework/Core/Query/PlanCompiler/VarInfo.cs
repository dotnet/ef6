//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...
// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Information about a Var and its replacement
    /// </summary>
    internal abstract class VarInfo
    {
        /// <summary>
        /// Gets <see cref="VarInfoKind"/> for this <see cref="VarInfo"/>.
        /// </summary>
        internal abstract VarInfoKind Kind { get; }

        /// <summary>
        /// Get the list of new Vars introduced by this VarInfo
        /// </summary>
        internal virtual List<Var> NewVars
        {
            get { return null; }
        }
    }
}
