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
using md = System.Data.Entity.Core.Metadata.Edm;

//
// This module contains a few utility functions that make it easier to operate
// with type metadata
//

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Common;

    internal static class TypeUtils
    {
        /// <summary>
        /// Is this a structured type? 
        /// Note: Structured, in this context means structured outside the server. 
        /// UDTs for instance, are considered to be scalar types - all WinFS types,
        /// would by this argument, be scalar types.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>true, if the type is a structured type</returns>
        internal static bool IsStructuredType(md.TypeUsage type)
        {
            return (md.TypeSemantics.IsReferenceType(type) ||
                    md.TypeSemantics.IsRowType(type) ||
                    md.TypeSemantics.IsEntityType(type) ||
                    md.TypeSemantics.IsRelationshipType(type) ||
                    (md.TypeSemantics.IsComplexType(type)));
        }

        /// <summary>
        /// Is this type a collection type?
        /// </summary>
        /// <param name="type">the current type</param>
        /// <returns>true, if this is a collection type</returns>
        internal static bool IsCollectionType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsCollectionType(type);
        }

        /// <summary>
        /// Is this type an enum type?
        /// </summary>
        /// <param name="type">the current type</param>
        /// <returns>true, if this is an enum type</returns>
        internal static bool IsEnumerationType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsEnumerationType(type);
        }

        /// <summary>
        /// Create a new collection type based on the supplied element type
        /// </summary>
        /// <param name="elementType">element type of the collection</param>
        /// <returns>the new collection type</returns>
        internal static md.TypeUsage CreateCollectionType(md.TypeUsage elementType)
        {
            return TypeHelpers.CreateCollectionTypeUsage(elementType);
        }
    }
}
