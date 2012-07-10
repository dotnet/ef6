namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ObjectQueryTests
    {
        [Fact]
        public void GetEnumerator_calls_ObjectResult_GetEnumerator()
        {
            var objectQueryExecutionPlanMock = new Mock<ObjectQueryExecutionPlan>(
                /*commandDefinition*/ null, /*resultShaperFactory*/ null, /*resultType*/ null, MergeOption.AppendOnly,
                /*singleEntitySet*/ null,/*compiledQueryParameters*/ null);

            var shaperMock = MockHelper.CreateShaperMock<object>();

            var objectResultMock = new Mock<ObjectResult<object>>(MockBehavior.Loose, shaperMock.Object, /*singleEntitySet*/ null, /*resultItemType*/ null);
            objectResultMock.Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<object>().GetEnumerator());
            objectQueryExecutionPlanMock.Setup(m => m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()))
                .Returns(objectResultMock.Object);

            var objectContextMock = new Mock<ObjectContext>(new ObjectQueryExecutionPlanFactory(), new Translator());
            var objectQueryStateMock = new Mock<ObjectQueryState>(typeof(object), objectContextMock.Object, /*parameters:*/ null, /*span:*/ null);
            objectQueryStateMock.Setup(m => m.GetExecutionPlan(It.IsAny<MergeOption?>())).Returns(objectQueryExecutionPlanMock.Object);

            var objectQuery = new ObjectQuery<object>(objectQueryStateMock.Object);

            objectQuery.GetEnumeratorInternal();

            objectResultMock.Verify(m => m.GetEnumerator(), Times.Once());
        }
    }
}
