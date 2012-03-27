namespace System.Data.Entity.Migrations
{
    // An alias is required because Error, Strings, IEnumerableExtensions etc. are defined in EntityFramework.dll and EntityFramework.PowerShell.dll
    extern alias powershell;
    using System.Collections.Generic;
    using powershell::System.Data.Entity.Migrations.Utilities;
    using Xunit;

    public class TemplateProcessorTests
    {
        [Fact]
        public void Process_replaces_tokens()
        {
            var input = "The $animal$ goes $sound$.";
            var tokens = new Dictionary<string, string>
                    {
                        { "animal", "cow" },
                        { "sound", "moo" }
                    };

            var output = new TemplateProcessor().Process(input, tokens);

            Assert.Equal(output, "The cow goes moo.");
        }

        [Fact]
        public void Process_handles_missing_tokens()
        {
            var input = "The $animal$ goes $sound$.";
            var tokens = new Dictionary<string, string>
                    {
                        { "animal", "cow" }
                    };

            var output = new TemplateProcessor().Process(input, tokens);

            Assert.Equal(output, "The cow goes .");
        }
    }
}