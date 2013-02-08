// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class TransactionHistory
    {
        internal virtual int TransactionID { get; set; }

        public virtual int ProductID { get; set; }
        public virtual int ReferenceOrderID { get; set; }
        public virtual int ReferenceOrderLineID { get; set; }
        public virtual DateTime TransactionDate { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal ActualCost { get; set; }

        internal virtual string TransactionType { get; set; }
        internal RowDetails RowDetails { get; set; }
        internal virtual Product Product { get; set; }
    }
}
