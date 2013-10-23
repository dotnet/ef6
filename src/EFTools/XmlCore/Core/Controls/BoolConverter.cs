// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Controls
{
    using Microsoft.Data.Tools.XmlDesignerBase.Core.Controls;

    /// <summary>
    ///     type converter for list of bool values
    /// </summary>
    internal class BoolConverter : FixedListConverter<bool>
    {
        protected override void PopulateMapping()
        {
            AddMapping(true, ControlsResources.PropertyWindow_Value_True);
            AddMapping(false, ControlsResources.PropertyWindow_Value_False);
        }
    }
}
