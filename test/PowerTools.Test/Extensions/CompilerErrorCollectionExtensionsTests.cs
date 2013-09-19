// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using Xunit;
    using System.CodeDom.Compiler;
    using System.Linq;

    public class CompilerErrorCollectionExtensionsTests
    {
        [Fact]
        public void HandleErrors_is_noop_when_no_errors()
        {
            var errors = new CompilerErrorCollection
                {
                    new CompilerError { IsWarning = true }
                };

            errors.HandleErrors("Not used");
        }

        [Fact]
        public void HandleErrors_throws_when_errors()
        {
            var error = new CompilerError { IsWarning = false };
            var errors = new CompilerErrorCollection { error };
            var message = "Some message";

            var ex = Assert.Throws<CompilerErrorException>(
                () => errors.HandleErrors(message));

            Assert.Equal(message, ex.Message);
            Assert.NotNull(ex.Errors);
            Assert.Equal(1, ex.Errors.Count());
            Assert.Same(error, ex.Errors.Single());
        }
    }
}
