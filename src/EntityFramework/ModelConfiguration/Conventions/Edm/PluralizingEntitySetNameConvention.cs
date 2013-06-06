// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to set the entity set name to be a pluralized version of the entity type name.
    /// </summary>
    public class PluralizingEntitySetNameConvention : IModelConvention<EntitySet>
    {
        private static readonly IPluralizationService _pluralizationService
            = DbConfiguration.GetService<IPluralizationService>();

        public void Apply(EntitySet edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

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
