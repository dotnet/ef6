// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if SQLSERVER
namespace System.Data.Entity.SqlServer.Utilities
#elif SQLSERVERCOMPACT
namespace System.Data.Entity.SqlServerCompact.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal static class ByteExtensions
    {
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            DebugCheck.NotNull(bytes);

            var stringBuilder = new StringBuilder();

            foreach (var @byte in bytes)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }
    }
}
