// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    internal class TreeViewSchemaFilterMapperSettings
    {
        internal bool UseOnlyCheckedNodes { get; set; }

        internal static TreeViewSchemaFilterMapperSettings GetDefaultSettings()
        {
            var settings = new TreeViewSchemaFilterMapperSettings();
            settings.UseOnlyCheckedNodes = true;
            return settings;
        }
    }
}
