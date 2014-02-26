// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ConventionsTypeFilter
    {
        public virtual bool IsConvention(Type conventionType)
        {
            return IsConfigurationConvention(conventionType)
                   || IsConceptualModelConvention(conventionType)
                   || IsConceptualToStoreMappingConvention(conventionType)
                   || IsStoreModelConvention(conventionType);
        }

        public static bool IsConfigurationConvention(Type conventionType)
        {
            return typeof(IConfigurationConvention).IsAssignableFrom(conventionType)
                   || typeof(Convention).IsAssignableFrom(conventionType)
                   || conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<>)).Any()
                   || conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<,>)).Any();
        }

        public static bool IsConceptualModelConvention(Type conventionType)
        {
            return conventionType.GetGenericTypeImplementations(typeof(IConceptualModelConvention<>)).Any();
        }

        public static bool IsStoreModelConvention(Type conventionType)
        {
            return conventionType.GetGenericTypeImplementations(typeof(IStoreModelConvention<>)).Any();
        }

        public static bool IsConceptualToStoreMappingConvention(Type conventionType)
        {
            return typeof(IDbMappingConvention).IsAssignableFrom(conventionType);
        }
    }
}
