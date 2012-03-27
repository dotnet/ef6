namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    internal partial class AdvancedPatternsModelFirstContext
    {
        public AdvancedPatternsModelFirstContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }
    }
}