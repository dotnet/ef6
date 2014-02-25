// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;

    internal class ConventionsTypeFinder
    {
        readonly ConventionsTypeFilter _conventionsTypeFilter;
        readonly ConventionsTypeActivator _conventionsTypeActivator;

        public ConventionsTypeFinder()
            :this(new ConventionsTypeFilter(),new ConventionsTypeActivator())
        {
        }

        public ConventionsTypeFinder(ConventionsTypeFilter conventionsTypeFilter,ConventionsTypeActivator conventionsTypeActivator)
        {
            DebugCheck.NotNull(conventionsTypeFilter);
            DebugCheck.NotNull(conventionsTypeActivator);

            _conventionsTypeFilter = conventionsTypeFilter;
            _conventionsTypeActivator = conventionsTypeActivator;
        }

        public void AddConventions(IEnumerable<Type> types, Action<IConvention> addFunction )
        {
            DebugCheck.NotNull(types);
            DebugCheck.NotNull(addFunction);

            foreach (var type in types)
            {
                if (_conventionsTypeFilter.IsConvention(type))
                {
                    addFunction(_conventionsTypeActivator.Activate(type));
                }
            }
        }
    }
}
