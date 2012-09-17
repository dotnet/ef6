// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Design.PluralizationServices;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Convention to set the entity set name to be a pluralized version of the entity type name.
    /// </summary>
    public class PluralizingEntitySetNameConvention : IEdmConvention<EdmEntitySet>
    {
        private static readonly PluralizationService _pluralizationService
            = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));

        public void Apply(EdmEntitySet edmDataModelItem, EdmModel model)
        {
            if (edmDataModelItem.GetConfiguration() == null)
            {
                edmDataModelItem.Name
                    = model.GetEntitySets()
                        .Except(new[] { edmDataModelItem })
                        .UniquifyName(_pluralizationService.Pluralize(edmDataModelItem.Name));
            }
        }
    }
}
