namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    internal partial class SiteInfoMf
    {
        public SiteInfoMf()
        {
        }

        public SiteInfoMf(int? zone, string environment)
        {
            Zone = zone;
            Environment = environment;
        }
    }
}