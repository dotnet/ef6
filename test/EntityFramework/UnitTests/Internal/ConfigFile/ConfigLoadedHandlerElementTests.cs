// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ConfigLoadedHandlerElementTests : TestBase
    {
        [Fact]
        public void Type_name_can_be_accessed()
        {
            var element = new ConfigLoadedHandlerElement(1);
            Assert.Equal("", element.TypeName);

            element.TypeName = "Vicious";
            Assert.Equal("Vicious", element.TypeName);
        }

        [Fact]
        public void Method_name_can_be_accessed()
        {
            var element = new ConfigLoadedHandlerElement(1);
            Assert.Equal("", element.MethodName);

            element.MethodName = "Andy's Chest";
            Assert.Equal("Andy's Chest", element.MethodName);
        }

        [Fact]
        public void Handler_delegate_is_created_for_given_type_and_method()
        {
            Assert.Equal(
                "Perfect",
                new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "Perfect" }
                    .CreateHandlerDelegate().Method.Name);

            Assert.Equal(
                "Day",
                new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "Day" }
                    .CreateHandlerDelegate().Method.Name);
        }

        public static void Perfect(object sender, DbConfigurationLoadedEventArgs args)
        {
        }

        private static void Day(object sender, DbConfigurationLoadedEventArgs args)
        {
        }

        [Fact]
        public void Exception_is_thrown_if_type_could_not_be_loaded()
        {
            var element = new ConfigLoadedHandlerElement(1) { TypeName = "HanginRound", MethodName = "Perfect" };

            Assert.Equal(
                Strings.ConfigEventTypeNotFound("HanginRound"),
                Assert.Throws<InvalidOperationException>(() => element.CreateHandlerDelegate()).Message);
        }

        [Fact]
        public void Exception_is_thrown_for_method_that_does_not_match_expected_signature()
        {
            var element = new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "WalkOnTheWildSide" };

            Assert.Equal(
                Strings.ConfigEventBadMethod("WalkOnTheWildSide", GetType().AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateHandlerDelegate()).Message);
        }

        public static void WalkOnTheWildSide(object sender)
        {
        }

        private static void WalkOnTheWildSide(object sender, EventArgs args)
        {
        }

        [Fact]
        public void Exception_is_thrown_for_method_that_is_not_static()
        {
            var element = new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "MakeUp" };

            Assert.Equal(
                Strings.ConfigEventBadMethod("MakeUp", GetType().AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateHandlerDelegate()).Message);
        }

        public void MakeUp(object sender, DbConfigurationLoadedEventArgs args)
        {
        }

        [Fact]
        public void Exception_is_thrown_for_method_that_is_missing()
        {
            var element = new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "SatelliteOfLove" };

            Assert.Equal(
                Strings.ConfigEventBadMethod("SatelliteOfLove", GetType().AssemblyQualifiedName),
                Assert.Throws<InvalidOperationException>(() => element.CreateHandlerDelegate()).Message);
        }

        [Fact]
        public void Exception_is_thrown_for_method_that_cannot_be_bound_to_delegate_type()
        {
            var element = new ConfigLoadedHandlerElement(1) { TypeName = GetType().AssemblyQualifiedName, MethodName = "WagonWheel" };

            var exception = Assert.Throws<InvalidOperationException>(() => element.CreateHandlerDelegate());

            Assert.Equal(Strings.ConfigEventCannotBind("WagonWheel", GetType().AssemblyQualifiedName), exception.Message);
            Assert.NotNull(exception.InnerException);
        }

        public static int WagonWheel(object sender, DbConfigurationLoadedEventArgs args)
        {
            return 0;
        }
    }
}
