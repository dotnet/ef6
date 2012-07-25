// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Linq;
    using Moq;
    using Xunit;

    public class DbExpressionExtensionsTests
    {
        [Fact]
        public void GetLeafNodes_returns_just_the_given_expression_if_it_is_not_of_requested_kind()
        {
            var mockExpression = CreateMockExpression(DbExpressionKind.Multiply);

            var result = mockExpression.Object.GetLeafNodes(DbExpressionKind.Not, e => Enumerable.Empty<DbExpression>());

            Assert.Equal(new[] { mockExpression.Object }, result);
        }

        [Fact]
        public void GetLeafNodes_flatterns_a_given_tree()
        {
            var mockRoot = CreateMockExpression(DbExpressionKind.Multiply);
            var mockChildOne = CreateMockExpression(DbExpressionKind.Multiply);
            var mockChildTwo = CreateMockExpression(DbExpressionKind.Not);
            var mockChildThree = CreateMockExpression(DbExpressionKind.Not);
            var mockChildFour = CreateMockExpression(DbExpressionKind.Not);

            var result = mockRoot.Object.GetLeafNodes(
                DbExpressionKind.Multiply,
                e =>
                    {
                        if (e == mockRoot.Object)
                        {
                            return new[] { mockChildOne.Object, mockChildTwo.Object };
                        }
                        if (e == mockChildOne.Object)
                        {
                            return new[] { mockChildThree.Object, mockChildFour.Object };
                        }
                        return Enumerable.Empty<DbExpression>();
                    }).ToList();

            Assert.Equal(new[] { mockChildThree.Object, mockChildFour.Object, mockChildTwo.Object }, result);
        }

        private static Mock<DbExpression> CreateMockExpression(DbExpressionKind kind)
        {
            var mockRoot = new Mock<DbExpression>();
            mockRoot.Setup(m => m.ExpressionKind).Returns(kind);
            
            return mockRoot;
        }
    }
}