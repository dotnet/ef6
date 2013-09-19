// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Common;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using NorthwindEFModel;

namespace NorthwindEFModel
{
    public abstract partial class Employee
    {
        public ObjectQuery<NorthwindEFModel.Customer> AllCustomers 
        {
            get
            {
                NorthwindEntities context = new NorthwindEntities();
                var query = from c in context.Customers
                            where c.Orders.Any(o => o.EmployeeID == this.EmployeeID)
                            select c;

                return query as ObjectQuery<NorthwindEFModel.Customer>;
            }
        }
    }
}