// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    ///     A PropertyRef class encapsulates a reference to one or more properties of
    ///     a complex instance - a record type, a complex type or an entity type.
    ///     A PropertyRef may be of the following kinds.
    ///     - a simple property reference (just a reference to a simple property)
    ///     - a typeid reference - applies only to entitytype and complextypes
    ///     - an entitysetid reference - applies only to ref and entity types
    ///     - a nested property reference (a reference to a nested property - a.b)
    ///     - an "all" property reference (all properties)
    /// </summary>
    internal abstract class PropertyRef
    {
        /// <summary>
        ///     Create a nested property ref, with "p" as the prefix.
        ///     The best way to think of this function as follows.
        ///     Consider a type T where "this" describes a property X on T. Now
        ///     consider a new type S, where "p" is a property of S and is of type T.
        ///     This function creates a PropertyRef that describes the same property X
        ///     from S.p instead
        /// </summary>
        /// <param name="p"> the property to prefix with </param>
        /// <returns> the nested property reference </returns>
        internal virtual PropertyRef CreateNestedPropertyRef(PropertyRef p)
        {
            return new NestedPropertyRef(p, this);
        }

        /// <summary>
        ///     Create a nested property ref for a simple property. Delegates to the function
        ///     above
        /// </summary>
        /// <param name="p"> the simple property </param>
        /// <returns> a nestedPropertyRef </returns>
        internal PropertyRef CreateNestedPropertyRef(md.EdmMember p)
        {
            return CreateNestedPropertyRef(new SimplePropertyRef(p));
        }

        /// <summary>
        ///     Creates a nested property ref for a rel-property. Delegates to the function above
        /// </summary>
        /// <param name="p"> the rel-property </param>
        /// <returns> a nested property ref </returns>
        internal PropertyRef CreateNestedPropertyRef(RelProperty p)
        {
            return CreateNestedPropertyRef(new RelPropertyRef(p));
        }

        /// <summary>
        ///     The tostring method for easy debuggability
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            return "";
        }
    }
}
