// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class StoreGeneratedPatternExtensions
    {
        public static DatabaseGeneratedOption ToDatabaseGeneratedOption(this StoreGeneratedPattern storeGeneratedPattern)
        {
            switch (storeGeneratedPattern)
            {
                case StoreGeneratedPattern.Identity:
                    return DatabaseGeneratedOption.Identity;

                case StoreGeneratedPattern.Computed:
                    return DatabaseGeneratedOption.Computed;
            }

            return DatabaseGeneratedOption.None;
        }
    }
}
