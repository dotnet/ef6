// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CSharp;
    using Xunit;

    public class ForwardingProxyTests
    {
        public class DummyType : MarshalByRefObject
        {
            private readonly string _value;

            public DummyType(string value)
            {
                _value = value;
            }

            public string GetValue()
            {
                return _value;
            }
        }

        [Fact]
        public void Invoke_forwards_messages_to_target()
        {
            var domain = AppDomain.CreateDomain("ForwardingProxyTests", null, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                var target = domain.CreateInstanceAndUnwrap(
                    typeof(DummyType).Assembly().FullName,
                    typeof(DummyType).FullName,
                    false,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    new[] { "Value1" },
                    null,
                    null);

                var forwardingProxy = new ForwardingProxy<DummyType>(target);
                var proxy = forwardingProxy.GetTransparentProxy();

                Assert.NotEqual(target, proxy);
                Assert.Equal("Value1", proxy.GetValue());
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        internal class LooseDummyType : MarshalByRefObject
        {
            public string GetValue()
            {
                throw new NotImplementedException();
            }
        }

        private const string Source = @"
namespace System.Data.Entity.Infrastructure.Design
{
    using System;

    internal class ForwardingProxyTests
    {
        internal class LooseDummyType : MarshalByRefObject
        {
            private string GetValue()
            {
                return ""FromRemote"";
            }
        }
    }
}
";

        [Fact]
        public void Invoke_can_forward_messages_to_unrelated_types()
        {
            using (var compiler = new CSharpCodeProvider())
            {
                var results = compiler.CompileAssemblyFromSource(new CompilerParameters(), Source);
                if (results.Errors.HasErrors)
                {
                    Debug.Fail(results.Errors.Cast<CompilerError>().FirstOrDefault(e => !e.IsWarning).ToString());
                }

                var domain = AppDomain.CreateDomain(
                    "ForwardingProxyTests",
                    null,
                    new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(results.PathToAssembly) });
                try
                {
                    var target = domain.CreateInstanceAndUnwrap(
                        results.CompiledAssembly.FullName,
                        typeof(LooseDummyType).FullName);
                    var forwardingProxy = new ForwardingProxy<LooseDummyType>(target);
                    var proxy = forwardingProxy.GetTransparentProxy();

                    Assert.Equal("FromRemote", proxy.GetValue());
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
        }
    }
}
