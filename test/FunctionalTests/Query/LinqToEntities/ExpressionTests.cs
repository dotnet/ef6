// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleModel;
    using Xunit;

    public class ExpressionTests : FunctionalTestBase
    {
        [Fact]
        public void Expression_from_variable()
        {
            Expression<Func<Product, bool>> testExpression = p => p.Id == 1;

            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(testExpression);

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_member()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Member);

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_property()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Property);

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Delegate());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.DelegateWithParameter(1));

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_method()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.Method());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.MethodWithVariable());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_static_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(StaticExpressions.MethodWithParameter(1));

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_member()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Member);

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_property()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Property);

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_delegate()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Delegate());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_delegate_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.DelegateWithParameter(1));

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_delegate_with_variable_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var foo = 1;

                var products = context.Products.Where(testInstance.DelegateWithParameter(foo));

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_method()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.Method());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_method_with_variable()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.MethodWithVariable());

                products.ToList();
            }
        }

        [Fact]
        public void Expression_from_instance_method_with_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products.Where(testInstance.MethodWithParameter(1));

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
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

                products.ToList();
            }
        }

        [Fact]
        public void Non_expression_embedded_static_delegate_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => p.Id == StaticExpressions.NonExpressionDelegate());

                Assert.Throws<NotSupportedException>(() => products.ToList());
            }
        }

        [Fact]
        public void Non_expression_embedded_static_method_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products
                    .Where(p => p.Id == StaticExpressions.NonExpressionMethod());

                Assert.Throws<NotSupportedException>(() => products.ToList());
            }
        }

        [Fact]
        public void Non_expression_embedded_instance_delegate_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => p.Id == testInstance.NonExpressionDelegate());

                Assert.Throws<NotSupportedException>(() => products.ToList());
            }
        }

        [Fact]
        public void Non_expression_embedded_instance_method_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var testInstance = new InstanceExpressions();
                var products = context.Products
                    .Where(p => p.Id == testInstance.NonExpressionMethod());

                Assert.Throws<NotSupportedException>(() => products.ToList());
            }
        }

        private static class StaticExpressions
        {
            public static readonly Expression<Func<Product, bool>> Member = (p => p.Id == 1);

            public static Expression<Func<Product, bool>> Property
            {
                get { return (p => p.Id == 1); }
            }

            public static readonly Func<Expression<Func<Product, bool>>> Delegate = () => (p => p.Id == 1);
            public static readonly Func<int, Expression<Func<Product, bool>>> DelegateWithParameter = i => (p => p.Id == i);

            public static readonly Func<int> NonExpressionDelegate = () => 1;

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

            public static int NonExpressionMethod()
            {
                return 1;
            }
        }

        private class InstanceExpressions
        {
            public readonly Expression<Func<Product, bool>> Member = (p => p.Id == 1);

            public Expression<Func<Product, bool>> Property
            {
                get { return (p => p.Id == 1); }
            }

            public readonly Func<Expression<Func<Product, bool>>> Delegate = () => (p => p.Id == 1);
            public readonly Func<int, Expression<Func<Product, bool>>> DelegateWithParameter = i => (p => p.Id == i);

            public readonly Func<int> NonExpressionDelegate = () => 1;

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

            public int NonExpressionMethod()
            {
                return 1;
            }
        }
    }
}
