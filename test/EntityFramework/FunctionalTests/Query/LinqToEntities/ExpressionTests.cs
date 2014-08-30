// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class ExpressionTests : FunctionalTestBase
    {
        [Fact]
        public void Expression_from_variable()
        {
            Expression<Func<SimpleModel.Product, bool>> testExpression = p => p.Id == 1;

            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(testExpression);

                Assert.DoesNotThrow(() => 
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_member()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Member);

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_property()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Property);

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Delegate());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.DelegateWithParameter(1));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_method()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Method());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.MethodWithVariable());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_static_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.MethodWithParameter(1));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }



        [Fact]
        public void Expression_from_instance_member()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Member);

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_property()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Property);

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Delegate());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.DelegateWithParameter(1));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_method()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Method());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.MethodWithVariable());

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_from_instance_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.MethodWithParameter(1));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }



        [Fact]
        public void Expression_embedded_variable()
        {
            using (var context = new SimpleModelContext())
            {
                Expression<Func<Product, bool>> testExpression = p => p.Id == 1;
                var products = context.Products
                    .Where(p => context.Products.Where(testExpression)
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_member()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.Member)
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_property()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.Property)
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.Delegate())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.DelegateWithParameter(1))
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_method()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.Method())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.MethodWithVariable())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_static_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => context.Products.Where(StaticExpressions.MethodWithParameter(1))
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }


        [Fact]
        public void Expression_embedded_instance_member()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.Member)
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_property()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.Property)
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.Delegate())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.DelegateWithParameter(1))
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_method()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.Method())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.MethodWithVariable())
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }

        [Fact]
        public void Expression_embedded_instance_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => context.Products.Where(testInstance.MethodWithParameter(1))
                        .Contains(p));

                Assert.DoesNotThrow(() =>
                    products.ToList());
            }
        }



        private static class StaticExpressions
        {
            public static readonly Expression<Func<Product, bool>> Member = (p => p.Id == 1);

            public static Expression<Func<Product, bool>> Property { get { return (p => p.Id == 1); } }


            public static readonly Func<Expression<Func<Product, bool>>> Delegate = () => (p => p.Id == 1);
            public static readonly Func<int, Expression<Func<Product, bool>>> DelegateWithParameter = i => (p => p.Id == i);


            public static Expression<Func<Product, bool>> Method()
            {
                return p => p.Id == 1;
            }

            public static Expression<Func<Product, bool>> MethodWithVariable()
            {
                var x = 1;
                return p => p.Id == x;
            }

            public static Expression<Func<Product, bool>> MethodWithParameter(int x)
            {
                return p => p.Id == x;
            }
        }


        private class InstanceExpressions
        {
            public readonly Expression<Func<Product, bool>> Member = (p => p.Id == 1);

            public Expression<Func<Product, bool>> Property { get { return (p => p.Id == 1); } }


            public readonly Func<Expression<Func<Product, bool>>> Delegate = () => (p => p.Id == 1);
            public readonly Func<int, Expression<Func<Product, bool>>> DelegateWithParameter = i => (p => p.Id == i);


            public Expression<Func<Product, bool>> Method()
            {
                return p => p.Id == 1;
            }

            public Expression<Func<Product, bool>> MethodWithVariable()
            {
                var i = 1;
                return p => p.Id == i;
            }

            public Expression<Func<Product, bool>> MethodWithParameter(int i)
            {
                return p => p.Id == i;
            }
        }
    }

}
