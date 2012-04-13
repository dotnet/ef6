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
