// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace SimpleModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class CustomerForLinq : BaseTypeForLinq
    {
        public string Region { get; set; }
        public string CompanyName { get; set; }
        public ICollection<OrderForLinq> Orders { get; set; }

        public override bool EntityEquals(BaseTypeForLinq other)
        {
            Debug.Assert(other is CustomerForLinq, "Expected other side to already have been checked to be the correct type.");

            var otherCustomer = (CustomerForLinq)other;
            bool customersEqual = Orders == null ?
                                  otherCustomer.Orders == null :
                                  Orders.SequenceEqual(otherCustomer.Orders, new BaseTypeForLinqComparer());

            return base.EntityEquals(other) && customersEqual;
        }

        public override int EntityHashCode
        {
            get
            {
                int hash = base.EntityHashCode;
                if (Orders != null)
                {
                    foreach (var item in Orders)
                    {
                        hash = 37 * hash + item.EntityHashCode;
                    }
                }
                return hash;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(String.Format(CultureInfo.InvariantCulture, "ID: {0}, Region: {1}, CompanyName: {2}", Id, Region, CompanyName));
            if (Orders != null)
            {
                foreach (var order in Orders)
                {
                    builder.Append("  ").AppendLine(order.ToString());
                }
            }
            return builder.ToString();
        }
    }
}
