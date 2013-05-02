// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Config;
    using System.IO;

    /// <summary>
    ///     Creates a new instance of <see cref="DbCommandLogger" /> or a type derived from
    ///     <see cref="DbCommandLogger" />.
    /// </summary>
    /// <remarks>
    ///     Delegates of this type are used to create instances derived from <see cref="DbCommandLogger" />
    ///     for use with <see cref="Database.Log" /> to log generated SQL to a <see cref="TextWriter" />.
    ///     The factory is set on <see cref="DbConfiguration.SetCommandLogger" />
    /// </remarks>
    /// <param name="context">
    ///     The <see cref="DbContext" /> for which command will be logged.
    /// </param>
    /// <param name="writer">
    ///     The  <see cref="TextWriter" />
    /// </param>
    /// to which log output will be written.
    /// <returns>
    ///     The <see cref="DbCommandLogger" /> to use.
    /// </returns>
    public delegate DbCommandLogger DbCommandLoggerFactory(DbContext context, TextWriter writer);
}
