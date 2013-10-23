// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Linq;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;

    internal class EnumUnderlyingTypeConverter : FixedListConverter<string>
    {
        protected override void PopulateMapping()
        {
            foreach (var primType in ModelHelper.UnderlyingEnumTypes.Select(t => t.Name))
            {
                AddMapping(primType, primType);
            }
        }
    }
}
