// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using SimpleModel;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    [DbModelBuilderVersion(DbModelBuilderVersion.Latest)]
    public class SimpleModelContextLegacy : SimpleModelContext
    {
        public SimpleModelContextLegacy()
        {
        }
    }
}
