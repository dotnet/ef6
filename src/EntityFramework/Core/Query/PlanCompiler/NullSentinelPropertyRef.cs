// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    ///     An NullSentinel propertyref represents the NullSentinel property for
    ///     a row type.
    ///     As with TypeId, this class is a singleton instance
    /// </summary>
    internal class NullSentinelPropertyRef : PropertyRef
    {
        private static readonly NullSentinelPropertyRef _singleton = new NullSentinelPropertyRef();

        private NullSentinelPropertyRef()
        {
        }

        /// <summary>
        ///     Gets the singleton instance
        /// </summary>
        internal static NullSentinelPropertyRef Instance
        {
            get { return _singleton; }
        }

        ///<summary>
        ///</summary>
        ///<returns> </returns>
        public override string ToString()
        {
            return "NULLSENTINEL";
        }
    }
}
