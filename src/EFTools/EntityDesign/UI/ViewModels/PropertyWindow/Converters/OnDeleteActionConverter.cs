// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;

    internal class OnDeleteActionConverter : FixedListConverter<string>
    {
        protected override void PopulateMapping()
        {
            AddMapping(ModelConstants.OnDeleteAction_None, ModelConstants.OnDeleteAction_None);
            AddMapping(ModelConstants.OnDeleteAction_Cascade, ModelConstants.OnDeleteAction_Cascade);
        }
    }
}
