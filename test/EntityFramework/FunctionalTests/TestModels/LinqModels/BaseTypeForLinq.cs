// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    /// <summary>
    /// A base type used for all entities used for DbQuery LINQ tests.
    /// </summary>
    public class BaseTypeForLinq
    {
        /// <summary>
        /// The primary key for all entities used in LINQ tests.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// This method allows to entities to be tested for equality in a non-standard way.
        /// It is non-standard because it doesn't need to be as robust as a general purpose
        /// Equals implementation, and because it should never be used by framework code itself
        /// since this could risk causing framework behavior to change.
        /// </summary>
        /// <param name="other"> The entity to compare. </param>
        /// <returns> True if they represent the same entity; false otherwise. </returns>
        public virtual bool EntityEquals(BaseTypeForLinq other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// A hash code that works in conjunction with the EntityEquals method.
        /// </summary>
        public virtual int EntityHashCode
        {
            get { return Id; }
        }
    }
}
