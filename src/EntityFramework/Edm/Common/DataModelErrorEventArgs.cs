// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class DataModelErrorEventArgs : DataModelEventArgs
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
