namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    public abstract partial class EmployeeMf
    {
        protected EmployeeMf()
        {
        }

        protected EmployeeMf(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}