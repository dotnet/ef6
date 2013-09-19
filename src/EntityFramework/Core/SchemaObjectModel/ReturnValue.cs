// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    /// <summary>
    /// Summary description for ReturnValue.
    /// </summary>
    internal sealed class ReturnValue<T>
    {
        #region Instance Fields

        private bool _succeeded;
        private T _value;

        #endregion

        internal bool Succeeded
        {
            get { return _succeeded; }
        }

        internal T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _succeeded = true;
            }
        }
    }
}
