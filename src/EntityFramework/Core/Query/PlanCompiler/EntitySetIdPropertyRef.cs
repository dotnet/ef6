// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    // <summary>
    // An EntitySetId propertyref represents the EntitySetId property for
    // an entity type or a ref type.
    // As with TypeId, this class is a singleton instance
    // </summary>
    internal class EntitySetIdPropertyRef : PropertyRef
    {
        private EntitySetIdPropertyRef()
        {
        }

        // <summary>
        // Gets the singleton instance
        // </summary>
        internal static EntitySetIdPropertyRef Instance = new EntitySetIdPropertyRef();

        public override string ToString()
        {
            return "ENTITYSETID";
        }
    }
}
