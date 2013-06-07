// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Entity.Migrations.Utilities;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     This extends StringWriter primarily to add the ability to add an indent
    ///     to each line that is written out.
    /// </summary>
    internal class SqlWriter : IndentedTextWriter
    {
        /// <summary>
        /// </summary>
        /// <param name="b"> </param>
        public SqlWriter(StringBuilder b)
            : base(new StringWriter(b, Culture), HardTabString)
            // Culture must match what is used by underlying IndentedTextWriter
        {
        }
    }
}
