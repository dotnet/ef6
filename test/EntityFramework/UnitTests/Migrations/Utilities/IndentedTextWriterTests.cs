// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.IO;
    using Xunit;

    public class IndentedTextWriterTests
    {
        [Fact] // CodePlex 1603
        public void Extreme_indentation_does_not_overflow_buffer_size()
        {
            var output = new StringWriter();
            var writer = new IndentedTextWriter(output, "   ");
            var totalLength = 0;

            // Run once, filling the cache
            for (var i = 0; i < 500; i++)
            {
                writer.Indent = i;
                Assert.Equal(i * 3, writer.CurrentIndentation().Length);

                writer.WriteLine("OhNo!");
                totalLength += i * 3 + 7;
                Assert.Equal(totalLength, output.ToString().Length);
            }

            // Run again, this time using the cache
            for (var i = 0; i < 500; i++)
            {
                writer.Indent = i;
                Assert.Equal(i * 3, writer.CurrentIndentation().Length);
            }
        }

        [Fact]
        public void Random_access_of_indents_works()
        {
            var writer = new IndentedTextWriter(new StringWriter(), "   ");

            foreach (var i in new[] { 4, 2, 5, -7, 0, 5, -1, 9, 0, 0 })
            {
                writer.Indent = i;
                var j = i < 0 ? 0 : i;
                Assert.Equal(j * 3, writer.CurrentIndentation().Length);
                Assert.Equal(j * 3, writer.CurrentIndentation().Length);
            }
        }
    }
}
