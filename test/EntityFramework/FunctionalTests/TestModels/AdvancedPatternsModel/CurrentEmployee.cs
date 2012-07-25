// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace AdvancedPatternsModel
{
    public class CurrentEmployee : Employee
    {
        public CurrentEmployee Manager { get; set; }
        public decimal LeaveBalance { get; set; }
        public Office Office { get; set; }
    }
}
