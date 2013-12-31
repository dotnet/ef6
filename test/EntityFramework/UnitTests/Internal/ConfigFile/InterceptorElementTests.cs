// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class InterceptorElementTests : TestBase
    {
        [Fact]
        public void TypeName_can_be_accessed()
        {
            var element = new InterceptorElement(1);
            Assert.Equal("", element.TypeName);

            element.TypeName = "Vicious";
            Assert.Equal("Vicious", element.TypeName);
        }

        [Fact]
        public void Parameters_can_be_accessed()
        {
            var element = new InterceptorElement(1);
            Assert.Empty(element.Parameters);

            var arg1 = element.Parameters.NewElement();
            arg1.ValueString = "A";

            var arg2 = element.Parameters.NewElement();
            arg2.ValueString = "A";
            arg2.TypeName = "System.Int32";

            var args = element.Parameters.OfType<ParameterElement>().ToList();

            Assert.Equal(2, args.Count);
            Assert.Same(arg1, args[0]);
            Assert.Same(arg2, args[1]);
        }

        [Fact]
        public void Interceptor_is_created_for_given_type()
        {
            Assert.IsType<Perfect>(new InterceptorElement(1) { TypeName = typeof(Perfect).AssemblyQualifiedName }.CreateInterceptor());

            var element = new InterceptorElement(1) { TypeName = typeof(Day).AssemblyQualifiedName };
            
            var arg = element.Parameters.NewElement();
            arg.ValueString = "1";
            
            arg = element.Parameters.NewElement();
            arg.ValueString = "2";
            arg.TypeName = "System.Int32";

            var interceptor = (Day)element.CreateInterceptor();
            Assert.Equal("1", interceptor.Such);
            Assert.Equal(2, interceptor.A);
        }

        public class Perfect : IDbCommandTreeInterceptor
        {
            public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
            {
            }
        }

        private class Day : IDbInterceptor
        {
            public Day(string such, int a)
            {
                Such = such;
                A = a;
            }

            public string Such { get; set; }
            public int A { get; set; }
        }

        [Fact]
        public void Exception_is_thrown_if_type_could_not_be_loaded()
        {
            var element = new InterceptorElement(1) { TypeName = "HanginRound" };

            Assert.Equal(
                Strings.InterceptorTypeNotFound("HanginRound"),
                Assert.Throws<InvalidOperationException>(() => element.CreateInterceptor()).Message);
        }

        [Fact]
        public void Exception_is_thrown_for_type_that_does_not_implement_IDbInterceptor()
        {
            var element = new InterceptorElement(1) { TypeName = typeof(WalkOnTheWildSide).AssemblyQualifiedName };

            Assert.Equal(
                Strings.InterceptorTypeNotInterceptor(typeof(WalkOnTheWildSide).AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateInterceptor()).Message);
        }

        public class WalkOnTheWildSide
        {
        }

        [Fact]
        public void Exception_is_thrown_if_constructor_arguments_dont_match_expected_parameters()
        {
            var element = new InterceptorElement(1) { TypeName = typeof(SatelliteOfLove).AssemblyQualifiedName };

            Assert.Equal(
                Strings.InterceptorTypeNotFound(typeof(SatelliteOfLove).AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateInterceptor()).Message);

            var arg = element.Parameters.NewElement();
            arg.ValueString = "1";
            arg.TypeName = "System.Int32";

            Assert.Equal(
                Strings.InterceptorTypeNotFound(typeof(SatelliteOfLove).AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateInterceptor()).Message);
        }

        public class SatelliteOfLove : IDbInterceptor
        {
            public SatelliteOfLove(string wayUpToMars)
            {
            }
        }
    }
}
