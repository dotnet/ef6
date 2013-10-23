// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal static class EdmxUtils
    {
        public static XmlReader GetEDMXXsdResource(Version schemaVersion)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            var assembly = typeof(EdmxUtils).Assembly;

            if (schemaVersion == EntityFrameworkVersion.Version1)
            {
                return
                    XmlReader.Create(
                        assembly.GetManifestResourceStream("Microsoft.Data.Entity.Design.Model.Microsoft.Data.Entity.Design.Edmx_1.xsd"));
            }
            else if (schemaVersion == EntityFrameworkVersion.Version2)
            {
                return
                    XmlReader.Create(
                        assembly.GetManifestResourceStream("Microsoft.Data.Entity.Design.Model.Microsoft.Data.Entity.Design.Edmx_2.xsd"));
            }
            else
            {
                Debug.Assert(schemaVersion == EntityFrameworkVersion.Version3, "Unrecognized schema version.");

                return
                    XmlReader.Create(
                        assembly.GetManifestResourceStream("Microsoft.Data.Entity.Design.Model.Microsoft.Data.Entity.Design.Edmx_3.xsd"));
            }
        }
    }
}
