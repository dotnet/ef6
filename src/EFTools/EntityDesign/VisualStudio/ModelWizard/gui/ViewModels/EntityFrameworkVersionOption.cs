// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.ViewModels
{
    using System;
    using System.Diagnostics;

    internal class EntityFrameworkVersionOption
    {
        public EntityFrameworkVersionOption(Version entityFrameworkVersion, Version targetNetFrameworkVersion = null)
        {
            Debug.Assert(entityFrameworkVersion != null, "entityFrameworkVersion is null.");

            Name = RuntimeVersion.GetName(entityFrameworkVersion, targetNetFrameworkVersion);
            Version = entityFrameworkVersion;
        }

        public string Name { get; set; }
        public Version Version { get; set; }
        public bool Disabled { get; set; }
        public bool IsDefault { get; set; }
    }
}
