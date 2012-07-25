// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System;

    public class DatabaseLog
    {
        public virtual int DatabaseLogID { get; set; }

        public virtual DateTime PostTime { get; set; }

        public virtual string DatabaseUser { get; set; }

        public virtual string Event { get; set; }

        public virtual string Schema { get; set; }

        public virtual string Object { get; set; }

        public virtual string TSQL { get; set; }

        public virtual string XmlEvent { get; set; }
    }
}