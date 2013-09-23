// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;

    // <summary>
    // Helper class that extends Tuple to give the Item1 and Item2 properties more meaningful names.
    // </summary>
    internal class EntitySetTypePair : Tuple<EntitySet, Type>
    {
        #region Constructor

        // <summary>
        // Creates a new pair of the given EntitySet and BaseType.
        // </summary>
        public EntitySetTypePair(EntitySet entitySet, Type type)
            : base(entitySet, type)
        {
        }

        #endregion

        #region Properties

        // <summary>
        // The EntitySet part of the pair.
        // </summary>
        public EntitySet EntitySet
        {
            get { return Item1; }
        }

        // <summary>
        // The BaseType part of the pair.
        // </summary>
        public Type BaseType
        {
            get { return Item2; }
        }

        #endregion
    }
}
