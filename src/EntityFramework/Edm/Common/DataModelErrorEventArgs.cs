namespace System.Data.Entity.Edm.Common
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// </summary>
    [Serializable]
    internal class DataModelErrorEventArgs : DataModelEventArgs
    {
        /// <summary>
        ///     Gets an optional value indicating which property of the source item caused the event to be raised.
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        ///     Gets a value that identifies the specific error that is being raised.
        /// </summary>
        public int ErrorCode { get; internal set; }

        /// <summary>
        ///     Gets an optional descriptive message the describes the error that is being raised.
        /// </summary>
        public string ErrorMessage { get; internal set; }
    }
}
