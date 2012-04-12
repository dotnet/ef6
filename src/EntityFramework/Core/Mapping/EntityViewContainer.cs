namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Base class for the type created at design time to store the generated views.
    /// </summary>
    public abstract class EntityViewContainer
    {
        #region Constructors

        #endregion

        #region fields

        #endregion

        #region properties

        /// <summary>
        /// Returns the cached dictionary of (ExtentName,EsqlView)
        /// </summary>
        internal IEnumerable<KeyValuePair<string, string>> ExtentViews
        {
            get
            {
                for (var i = 0; i < ViewCount; i++)
                {
                    yield return GetViewAt(i);
                }
            }
        }

        protected abstract KeyValuePair<string, string> GetViewAt(int index);

        public string EdmEntityContainerName { get; set; }

        public string StoreEntityContainerName { get; set; }

        public string HashOverMappingClosure { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverAll")]
        public string HashOverAllExtentViews { get; set; }

        public int ViewCount { get; protected set; }

        #endregion
    }
}
