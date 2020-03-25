// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class TypeSystemTests
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(TypeSystem.GetDefaultMethod);
        }

        [Fact]
        public void IsImplementationOf_identifies_properties_that_are_part_of_an_implicit_interface_implementation()
        {
            Assert.True(typeof(NoMore).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<double>)));
            Assert.False(typeof(NoMore).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<int>)));
            Assert.False(typeof(NoMore).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<string>)));

            Assert.True(typeof(MrNiceGuy).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<int>)));
            Assert.False(typeof(MrNiceGuy).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<string>)));

            Assert.True(typeof(IAlice<double>).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<double>)));
            Assert.False(typeof(IAlice<double>).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<int>)));
            Assert.False(typeof(IAlice<double>).GetDeclaredProperty("Cooper").IsImplementationOf(typeof(IAlice<string>)));
        }

        [Fact] // It's not clear whether this was intentional behavior or not
        public void IsImplementationOf_does_not_identify_properties_that_are_part_of_an_explicit_interface_implementation()
        {
            Assert.False(
                typeof(MrNiceGuy).GetRuntimeProperties()
                    .Single(p => p.PropertyType == typeof(string))
                    .IsImplementationOf(typeof(IAlice<string>)));
        }

        public interface IAlice<T>
        {
            T Cooper { get; set; }
        }

        public class NoMore : MrNiceGuy, IAlice<double>
        {
            public new double Cooper { get; set; }
        }

        public class MrNiceGuy : IAlice<int>, IAlice<string>
        {
            public int Cooper { get; set; }
            string IAlice<string>.Cooper { get; set; }
        }
    }
}
