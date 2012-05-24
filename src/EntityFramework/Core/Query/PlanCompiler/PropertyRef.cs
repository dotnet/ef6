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
// The PropertyRef class (and its subclasses) represent references to a property
// of a type.
// The PropertyRefList class represents a list of expected properties
// where each property from the type is described as a PropertyRef
//
// These classes are used by the StructuredTypeEliminator module as part of
// eliminating all structured types. The basic idea of this module is that all
// structured types are flattened out into a single level. To avoid a large amount
// of potentially unnecessary information, we try to identify what pieces of information
// are really necessary at each node of the tree. This is where PropertyRef comes in.
// A PropertyRef (and more generally, a PropertyRefList) identifies a list of
// properties, and can be attached to a node/var to indicate that these were the
// only desired properties.
//

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// A PropertyRef class encapsulates a reference to one or more properties of
    /// a complex instance - a record type, a complex type or an entity type.
    /// A PropertyRef may be of the following kinds.
    ///   - a simple property reference (just a reference to a simple property)
    ///   - a typeid reference - applies only to entitytype and complextypes
    ///   - an entitysetid reference - applies only to ref and entity types
    ///   - a nested property reference (a reference to a nested property - a.b)
    ///   - an "all" property reference (all properties)
    /// </summary>
    internal abstract class PropertyRef
    {
        /// <summary>
        /// Create a nested property ref, with "p" as the prefix.
        /// The best way to think of this function as follows.
        /// Consider a type T where "this" describes a property X on T. Now
        /// consider a new type S, where "p" is a property of S and is of type T.
        /// This function creates a PropertyRef that describes the same property X
        /// from S.p instead
        /// </summary>
        /// <param name="p">the property to prefix with</param>
        /// <returns>the nested property reference</returns>
        internal virtual PropertyRef CreateNestedPropertyRef(PropertyRef p)
        {
            return new NestedPropertyRef(p, this);
        }

        /// <summary>
        /// Create a nested property ref for a simple property. Delegates to the function
        /// above
        /// </summary>
        /// <param name="p">the simple property</param>
        /// <returns>a nestedPropertyRef</returns>
        internal PropertyRef CreateNestedPropertyRef(md.EdmMember p)
        {
            return CreateNestedPropertyRef(new SimplePropertyRef(p));
        }

        /// <summary>
        /// Creates a nested property ref for a rel-property. Delegates to the function above
        /// </summary>
        /// <param name="p">the rel-property</param>
        /// <returns>a nested property ref</returns>
        internal PropertyRef CreateNestedPropertyRef(RelProperty p)
        {
            return CreateNestedPropertyRef(new RelPropertyRef(p));
        }

        /// <summary>
        /// The tostring method for easy debuggability
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }
    }
}
