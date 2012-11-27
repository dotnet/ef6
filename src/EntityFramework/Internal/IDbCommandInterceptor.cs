// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;

    internal interface IDbCommandInterceptor
    {
        bool IsEnabled { get; set; }
        bool Intercept(DbCommand command);

        IEnumerable<InterceptedCommand> Commands { get; }
    }
}
