// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    /// <summary>
    ///     DataModelEventArgs is the base argument type for all events raised by consumers of Entity Data Model (EDM) models.
    /// </summary>
    [Serializable]
    public abstract class DataModelEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets a value indicating the <see cref="DataModelItem" /> that caused the event to be raised.
        /// </summary>
        public DataModelItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        [NonSerialized]
        private DataModelItem _item;
    }
}
