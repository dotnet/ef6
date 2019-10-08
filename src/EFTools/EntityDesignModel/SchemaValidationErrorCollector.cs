// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Xml.Schema;

    /// <summary>
    ///     Simple class to use to count the number of schema validation errors
    /// </summary>
    internal class SchemaValidationErrorCollector
    {
        private int _errorCount;

        internal int ErrorCount
        {
            get { return _errorCount; }
        }

        internal void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            ++_errorCount;
        }
    }
}
