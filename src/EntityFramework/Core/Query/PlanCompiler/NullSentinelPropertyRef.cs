namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// An NullSentinel propertyref represents the NullSentinel property for
    /// a row type.
    /// As with TypeId, this class is a singleton instance
    /// </summary>
    internal class NullSentinelPropertyRef : PropertyRef
    {
        private static readonly NullSentinelPropertyRef s_singleton = new NullSentinelPropertyRef();

        private NullSentinelPropertyRef()
        {
        }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        internal static NullSentinelPropertyRef Instance
        {
            get { return s_singleton; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NULLSENTINEL";
        }
    }
}
