// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.Collections.Generic;

    public class SalesReason
    {
        public virtual int SalesReasonID { get; set; }

        public virtual string Name { get; set; }

        public virtual string ReasonType { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    }
}
