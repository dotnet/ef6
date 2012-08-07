// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class AWBuildVersion
    {
        public virtual byte SystemInformationID { get; set; }

        public virtual string Database_Version { get; set; }

        public virtual DateTime VersionDate { get; set; }

        public virtual DateTime ModifiedDate { get; set; }
    }
}
