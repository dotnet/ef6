// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    public class OrderForLinq : BaseTypeForLinq
    {
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public CustomerForLinq Customer { get; set; }

        public override bool EntityEquals(BaseTypeForLinq other)
        {
            Debug.Assert(other is OrderForLinq, "Expected other side to already have been checked to be the correct type.");

            var otherOrder = (OrderForLinq)other;
            var customersEqual = Customer == null ? otherOrder.Customer == null : Customer.Id == otherOrder.Customer.Id;
            return base.EntityEquals(other) && customersEqual;
        }

        public override int EntityHashCode
        {
            get { return Customer == null ? Id : (37 * Id + Customer.Id); }
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "ID: {0}, Total: {1}, OrderDate: {2}", Id, Total, OrderDate);
        }
    }
}
