// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml;

    internal sealed class VersionConverterHandler : MetadataConverterHandler
    {
        private readonly Version _targetSchemaVersion;

        internal VersionConverterHandler(Version targetSchemaVersion)
        {
            _targetSchemaVersion = targetSchemaVersion;
        }

        /// <summary>
        ///     Update the EDMX version in the file
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected override XmlDocument DoHandleConversion(XmlDocument doc)
        {
            doc.DocumentElement.SetAttribute("Version", _targetSchemaVersion.ToString(2)); // only record Major.Minor version
            return doc;
        }
    }
}
