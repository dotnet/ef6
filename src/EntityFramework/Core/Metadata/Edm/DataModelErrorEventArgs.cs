// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// Information about an error that occurred processing an Entity Framework model.
    /// </summary>
    [Serializable]
    public class DataModelErrorEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets an optional value indicating which property of the source item caused the event to be raised.
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        ///     Gets an optional descriptive message the describes the error that is being raised.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary>
        ///     Gets a value indicating the <see cref="MetadataItem" /> that caused the event to be raised.
        /// </summary>
        public MetadataItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        [NonSerialized]
        private MetadataItem _item;
    }
}
