namespace BadMappingModel
{
    public class Office
    {
        public int OfficeId { get; set; }
        public string Name { get; set; }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
    }

    public class OnSiteEmployee : Employee
    {
        public Office Office { get; set; }
    }

    public class OffSiteEmployee : Employee
    {
        public string SiteName { get; set; }
    }
}