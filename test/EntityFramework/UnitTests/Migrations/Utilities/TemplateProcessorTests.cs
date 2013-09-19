// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections.Generic;
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
