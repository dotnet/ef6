// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    public class WorkOrder
    {
        public int WorkOrderId { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public string Details { get; set; }
    }
}
