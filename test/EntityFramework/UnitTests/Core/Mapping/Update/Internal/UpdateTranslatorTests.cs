// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class UpdateTranslatorTests
    {
        public class Update
        {
            [Fact]
            public void Propagates_server_gen_values_and_returns_entities_affected()
            {
                var updateTranslatorMock = new Mock<UpdateTranslator>
                                               {
                                                   CallBase = true
                                               };

                var serverGenValuesPropagatedCount = 0;
                var generatedValue = new object();
                var mockPropagatorResult = new Mock<PropagatorResult>
                                               {
                                                   CallBase = true
                                               };
                mockPropagatorResult.Setup(m => m.SetServerGenValue(It.IsAny<object>()))
                    .Callback<object>(
                        o =>
                            {
                                serverGenValuesPropagatedCount++;
                                Assert.Same(generatedValue, o);
                            });

                var updateCommandMock = new Mock<UpdateCommand>(
                    updateTranslatorMock.Object,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0));

                updateCommandMock.Setup(
                    m => m.Execute(
                        It.IsAny<Dictionary<int, object>>(),
                        It.IsAny<List<KeyValuePair<PropagatorResult, object>>>(),
                        It.IsAny<IDbCommandInterceptor>()))
                    .Returns(
                        (Dictionary<int, object> identifierValues,
                            List<KeyValuePair<PropagatorResult, object>> generatedValues,
                            IDbCommandInterceptor interceptor) =>
                            {
                                generatedValues.Add(
                                    new KeyValuePair<PropagatorResult, object>(mockPropagatorResult.Object, generatedValue));
                                return 1;
                            });

                updateTranslatorMock.Protected().Setup<IEnumerable<UpdateCommand>>("ProduceCommands").Returns(
                    new[] { updateCommandMock.Object });

                var entitiesAffected = updateTranslatorMock.Object.Update();

                Assert.Equal(0, entitiesAffected);
                Assert.Equal(1, serverGenValuesPropagatedCount);
            }

            [Fact]
            public void Wraps_exceptions()
            {
                var updateTranslatorMock = new Mock<UpdateTranslator>
                                               {
                                                   CallBase = true
                                               };

                var dbException = new Mock<DbException>("Exception message").Object;

                var updateCommandMock = new Mock<UpdateCommand>(
                    updateTranslatorMock.Object,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0));

                updateCommandMock.Setup(
                    m => m.Execute(
                        It.IsAny<Dictionary<int, object>>(),
                        It.IsAny<List<KeyValuePair<PropagatorResult, object>>>(),
                        It.IsAny<IDbCommandInterceptor>()))
                    .Returns(() => { throw dbException; });

                var objectStateManager = new Mock<ObjectStateManager>
                                             {
                                                 CallBase = true
                                             }.Object;
                var objectStateEntryMock = new Mock<ObjectStateEntry>(objectStateManager, /*entitySet:*/null, EntityState.Unchanged);

                updateCommandMock.Setup(m => m.GetStateEntries(It.IsAny<UpdateTranslator>()))
                    .Returns(new[] { objectStateEntryMock.Object });

                new List<KeyValuePair<PropagatorResult, object>>();
                updateTranslatorMock.Protected().Setup<IEnumerable<UpdateCommand>>("ProduceCommands").Returns(
                    new[] { updateCommandMock.Object });

                var exception = Assert.Throws<UpdateException>(() => updateTranslatorMock.Object.Update());
                Assert.Equal(Strings.Update_GeneralExecutionException, exception.Message);
                Assert.Same(dbException, exception.InnerException);
                Assert.Same(objectStateEntryMock.Object, exception.StateEntries.Single());
            }
        }

        public class UpdateAsync
        {
            [Fact]
            private void Propagates_server_gen_values_and_returns_entities_affected()
            {
                var updateTranslatorMock = new Mock<UpdateTranslator>
                                               {
                                                   CallBase = true
                                               };

                var serverGenValuesPropagatedCount = 0;
                var generatedValue = new object();
                var mockPropagatorResult = new Mock<PropagatorResult>
                                               {
                                                   CallBase = true
                                               };
                mockPropagatorResult.Setup(m => m.SetServerGenValue(It.IsAny<object>()))
                    .Callback<object>(
                        o =>
                            {
                                serverGenValuesPropagatedCount++;
                                Assert.Same(generatedValue, o);
                            });

                var updateCommandMock = new Mock<UpdateCommand>(
                    updateTranslatorMock.Object,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0));

                updateCommandMock.Setup(
                    m => m.ExecuteAsync(
                        It.IsAny<Dictionary<int, object>>(),
                        It.IsAny<List<KeyValuePair<PropagatorResult, object>>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(
                        (Dictionary<int, object> identifierValues,
                            List<KeyValuePair<PropagatorResult, object>> generatedValues,
                            CancellationToken cancellationToken) =>
                            {
                                generatedValues.Add(
                                    new KeyValuePair<PropagatorResult, object>(mockPropagatorResult.Object, generatedValue));
                                return Task.FromResult(1L);
                            });

                updateTranslatorMock.Protected().Setup<IEnumerable<UpdateCommand>>("ProduceCommands").Returns(
                    new[] { updateCommandMock.Object });

                var entitiesAffected = updateTranslatorMock.Object.UpdateAsync(CancellationToken.None).Result;

                Assert.Equal(0, entitiesAffected);
                Assert.Equal(1, serverGenValuesPropagatedCount);
            }

            [Fact]
            public void Wraps_exceptions()
            {
                var updateTranslatorMock = new Mock<UpdateTranslator>
                                               {
                                                   CallBase = true
                                               };

                var dbException = new Mock<DbException>("Exception message").Object;

                var updateCommandMock = new Mock<UpdateCommand>(
                    updateTranslatorMock.Object,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0));
                updateCommandMock.Setup(
                    m => m.ExecuteAsync(
                        It.IsAny<Dictionary<int, object>>(),
                        It.IsAny<List<KeyValuePair<PropagatorResult, object>>>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(
                        (Dictionary<int, object> identifierValues,
                            List<KeyValuePair<PropagatorResult, object>> generatedValues,
                            CancellationToken cancellationToken) =>
                            { throw dbException; });

                var objectStateManager = new Mock<ObjectStateManager>
                                             {
                                                 CallBase = true
                                             }.Object;
                var objectStateEntryMock = new Mock<ObjectStateEntry>(objectStateManager, /*entitySet:*/null, EntityState.Unchanged);

                updateCommandMock.Setup(m => m.GetStateEntries(It.IsAny<UpdateTranslator>()))
                    .Returns(new[] { objectStateEntryMock.Object });

                new List<KeyValuePair<PropagatorResult, object>>();
                updateTranslatorMock.Protected().Setup<IEnumerable<UpdateCommand>>("ProduceCommands").Returns(
                    new[] { updateCommandMock.Object });

                var exception =
                    Assert.Throws<AggregateException>(() => updateTranslatorMock.Object.UpdateAsync(CancellationToken.None).Result);
                Assert.IsType<UpdateException>(exception.InnerException);
                Assert.Equal(Strings.Update_GeneralExecutionException, exception.InnerException.Message);
                Assert.Same(dbException, exception.InnerException.InnerException);
                Assert.Same(objectStateEntryMock.Object, ((UpdateException)exception.InnerException).StateEntries.Single());
            }
        }
    }
}
