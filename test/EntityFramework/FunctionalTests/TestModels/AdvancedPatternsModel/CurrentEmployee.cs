namespace AdvancedPatternsModel
{
    public class CurrentEmployee : Employee
    {
        public CurrentEmployee Manager { get; set; }
        public decimal LeaveBalance { get; set; }
        public Office Office { get; set; }
    }
}
