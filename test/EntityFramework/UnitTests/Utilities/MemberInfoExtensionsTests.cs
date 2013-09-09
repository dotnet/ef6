// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class MemberInfoExtensionsTests
    {
        public class GetValue
        {
            [Fact]
            public void GetValue_returns_value_of_static_property()
            {
                Assert.Equal(
                    "People Are People",
                    typeof(FakeForGetValue).GetProperty("Property").GetValue());
            }

            [Fact]
            public void GetValue_returns_value_of_static_field()
            {
                Assert.Equal(
                    "People Are People",
                    typeof(FakeForGetValue).GetField("Field").GetValue());
            }

            public class FakeForGetValue
            {
                public static readonly string Field = "People Are People";

                public static string Property
                {
                    get { return Field; }
                }
            }
        }

        public class GetCustomAttributes
        {
            [Fact]
            public void Can_return_attributes_from_types()
            {
                Assert.IsType<LawyerAttribute>(typeof(BetterCall).GetCustomAttributes<LawyerAttribute>(inherit: true).Single());
                Assert.IsType<LawyerAttribute>(typeof(BetterCall).GetCustomAttributes<LawyerAttribute>(inherit: false).Single());
                Assert.IsType<LaundererAttribute>(typeof(BetterCall).GetCustomAttributes<LaundererAttribute>(inherit: true).Single());
                Assert.Null(typeof(BetterCall).GetCustomAttributes<LaundererAttribute>(inherit: false).FirstOrDefault());

                Assert.Equal(
                    new[] { "LaundererAttribute", "LawyerAttribute" },
                    typeof(BetterCall).GetCustomAttributes<Attribute>(inherit: true).Select(a => a.GetType().Name).OrderBy(n => n));
            }

            [Fact]
            public void Can_return_attributes_from_properties()
            {
                var property = typeof(BetterCall).GetProperty("AmountLaundered");

                Assert.IsType<CarWashAttribute>(property.GetCustomAttributes<CarWashAttribute>(inherit: true).Single());
                Assert.IsType<CarWashAttribute>(property.GetCustomAttributes<CarWashAttribute>(inherit: false).Single());

                Assert.IsType<LazerTagAttribute>(property.GetCustomAttributes<LazerTagAttribute>(inherit: true).Single());
                Assert.Null(property.GetCustomAttributes<LazerTagAttribute>(inherit: false).FirstOrDefault());

                Assert.Equal(
                    new[] { "CarWashAttribute", "LazerTagAttribute" },
                    property.GetCustomAttributes<Attribute>(inherit: true).Select(a => a.GetType().Name).OrderBy(n => n));
            }

            [Fact]
            public void Can_return_attributes_from_methods()
            {
                var method = typeof(BetterCall).GetDeclaredMethod("Launder");

                Assert.IsType<CarWashAttribute>(method.GetCustomAttributes<CarWashAttribute>(inherit: true).Single());
                Assert.IsType<CarWashAttribute>(method.GetCustomAttributes<CarWashAttribute>(inherit: false).Single());

                Assert.IsType<LazerTagAttribute>(method.GetCustomAttributes<LazerTagAttribute>(inherit: true).Single());
                Assert.Null(method.GetCustomAttributes<LazerTagAttribute>(inherit: false).FirstOrDefault());

                Assert.Equal(
                    new[] { "CarWashAttribute", "LazerTagAttribute" },
                    method.GetCustomAttributes<Attribute>(inherit: true).Select(a => a.GetType().Name).OrderBy(n => n));
            }

            [Lawyer]
            public class BetterCall : Saul
            {
                [CarWash]
                public override decimal AmountLaundered { get; set; }

                [CarWash]
                public override void Launder(decimal dollars)
                {
                }
            }

            [Launderer]
            public class Saul
            {
                [LazerTag]
                public virtual decimal AmountLaundered { get; set; }

                [LazerTag]
                public virtual void Launder(decimal dollars)
                {
                }
            }

            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
            public class CarWashAttribute : Attribute
            {
            }

            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
            public class LazerTagAttribute : Attribute
            {
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public class LawyerAttribute : Attribute
            {
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            public class LaundererAttribute : Attribute
            {
            }
        }
    }
}
