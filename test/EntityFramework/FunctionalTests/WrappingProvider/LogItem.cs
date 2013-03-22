// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;

    public class LogItem
    {
        public LogItem(string method, DbConnection connection, object details)
        {
            Method = method;
            Connection = connection == null ? "<null>" : connection.ConnectionString;
            RawDetails = details;
            Details = details == null ? "<null>" : details.ToString();
        }

        public string Method { get; set; }
        public string Connection { get; set; }
        public string Details { get; set; }
        public object RawDetails { get; set; }
    }
}
