// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        // <summary>
        // Is this a structured type?
        // Note: Structured, in this context means structured outside the server.
        // UDTs for instance, are considered to be scalar types - all WinFS types,
        // would by this argument, be scalar types.
        // </summary>
        // <param name="type"> The type to check </param>
        // <returns> true, if the type is a structured type </returns>
        internal static bool IsStructuredType(md.TypeUsage type)
        {
            return (md.TypeSemantics.IsReferenceType(type) ||
                    md.TypeSemantics.IsRowType(type) ||
                    md.TypeSemantics.IsEntityType(type) ||
                    md.TypeSemantics.IsRelationshipType(type) ||
                    (md.TypeSemantics.IsComplexType(type)));
        }

        // <summary>
        // Is this type a collection type?
        // </summary>
        // <param name="type"> the current type </param>
        // <returns> true, if this is a collection type </returns>
        internal static bool IsCollectionType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsCollectionType(type);
        }

        // <summary>
        // Is this type an enum type?
        // </summary>
        // <param name="type"> the current type </param>
        // <returns> true, if this is an enum type </returns>
        internal static bool IsEnumerationType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsEnumerationType(type);
        }

        // <summary>
        // Create a new collection type based on the supplied element type
        // </summary>
        // <param name="elementType"> element type of the collection </param>
        // <returns> the new collection type </returns>
        internal static md.TypeUsage CreateCollectionType(md.TypeUsage elementType)
        {
            return TypeHelpers.CreateCollectionTypeUsage(elementType);
        }
    }
}
