namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.CodeDom.Compiler;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating;
    using Microsoft.VisualStudio.TextTemplating.VSHost;
    using Moq;
    using Xunit;

    public class TextTemplatingHostTests
    {
        [Fact]
        public void StandardAssemblyReferences_returns_references()
        {
            Assert.Equal(new[] { "System" }, new TextTemplatingHost().StandardAssemblyReferences);
        }

        [Fact]
        public void StandardImports_returns_imports()
        {
            Assert.Equal(new[] { "System" }, new TextTemplatingHost().StandardImports);
        }

        [Fact]
        public void GetHostOption_returns_null()
        {
            Assert.Null(new TextTemplatingHost().GetHostOption("CacheAssemblies"));
        }

        [Fact]
        public void LogErrors_is_noop_when_no_callback()
        {
            var host = new TextTemplatingHost();
            host.LogErrors(Mock.Of<CompilerErrorCollection>());
        }

        [Fact]
        public void LogErrors_delegates_when_callback()
        {
            var callback = new Mock<ITextTemplatingCallback>();
            var host = new Mock<TextTemplatingHost>() { CallBase = true };
            host.SetupGet(h => h.Callback).Returns(callback.Object);
            var errors = new CompilerErrorCollection
                {
                    new CompilerError { IsWarning = true, ErrorText = "Error1", Line = 1, Column = 42 }
                };

            host.Object.LogErrors(errors);

            callback.Verify(c => c.ErrorCallback(true, "Error1", 1, 42));
        }

        [Fact]
        public void ProvideTemplatingAppDomain_returns_current_domain()
        {
            Assert.Same(
                AppDomain.CurrentDomain,
                new TextTemplatingHost().ProvideTemplatingAppDomain("Content1"));
        }

        [Fact]
        public void ResolveAssemblyReference_resolves_simple_reference()
        {
            Assert.Equal(
                GetType().Assembly.Location,
                new TextTemplatingHost().ResolveAssemblyReference(GetType().Assembly.GetName().Name));
        }

        [Fact]
        public void ResolveAssemblyReference_resolves_qualified_reference()
        {
            Assert.Equal(
                GetType().Assembly.Location,
                new TextTemplatingHost().ResolveAssemblyReference(GetType().Assembly.FullName));
        }

        [Fact]
        public void SetFileExtension_is_noop_when_no_callback()
        {
            var host = new TextTemplatingHost();
            host.SetFileExtension(".out");
        }

        [Fact]
        public void SetFileExtension_delegates_when_callback()
        {
            var callback = new Mock<ITextTemplatingCallback>();
            var host = new Mock<TextTemplatingHost>() { CallBase = true };
            host.SetupGet(h => h.Callback).Returns(callback.Object);

            host.Object.SetFileExtension(".out");

            callback.Verify(c => c.SetFileExtension(".out"));
        }

        [Fact]
        public void SetOutputEncoding_is_noop_when_no_callback()
        {
            var host = new TextTemplatingHost();
            host.SetOutputEncoding(Encoding.ASCII, true);
        }

        [Fact]
        public void SetOutputEncoding_delegates_when_callback()
        {
            var callback = new Mock<ITextTemplatingCallback>();
            var host = new Mock<TextTemplatingHost>() { CallBase = true };
            host.SetupGet(h => h.Callback).Returns(callback.Object);

            host.Object.SetOutputEncoding(Encoding.ASCII, true);

            callback.Verify(c => c.SetOutputEncoding(Encoding.ASCII, true));
        }

        [Fact]
        public void CreateSession_returns_session()
        {
            Assert.IsType<TextTemplatingSession>(new TextTemplatingHost().CreateSession());
        }

        [Fact]
        public void ProcessTemplate_returns_result()
        {
            Assert.Equal("Result", new TextTemplatingHost().ProcessTemplate("Dummy.tt", "<#= \"Result\" #>"));
        }
    }
}
