// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class MultiplicityListConverter : FixedListConverter<string>
    {
        protected override void PopulateMapping()
        {
            AddMapping(ModelConstants.Multiplicity_Many, Resources.PropertyWindow_Value_MultiplicityMany);
            AddMapping(ModelConstants.Multiplicity_One, Resources.PropertyWindow_Value_MultiplicityOne);
            AddMapping(ModelConstants.Multiplicity_ZeroOrOne, Resources.PropertyWindow_Value_MultiplicityZeroOrOne);
        }
    }
}
