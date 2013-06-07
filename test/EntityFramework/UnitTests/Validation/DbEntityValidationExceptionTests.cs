// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Moq;
    using Xunit;

    public class DbEntityValidationExceptionTests
    {
        [Fact]
        public void DbEntityValidationException_parameterless_constructor()
        {
            var exception = new DbEntityValidationException();

            Assert.Equal(Strings.DbEntityValidationException_ValidationFailed, exception.Message);
            Assert.False(exception.EntityValidationErrors.Any());
        }

        [Fact]
        public void DbEntityValidationException_exceptionmessage_validationresult_constructor()
        {
            var validationExceptionCtorParamTypes = new[]
                                                        {
                                                            new[] { typeof(string) },
                                                            new[] { typeof(string), typeof(IEnumerable<DbEntityValidationResult>) },
                                                            new[] { typeof(string), typeof(Exception) },
                                                            new[]
                                                                {
                                                                    typeof(string), typeof(IEnumerable<DbEntityValidationResult>),
                                                                    typeof(Exception)
                                                                },
                                                        };

            foreach (var ctorParamTypes in validationExceptionCtorParamTypes)
            {
                TestDbValidationExceptionCtor(ctorParamTypes);
            }
        }

        private void TestDbValidationExceptionCtor(Type[] types)
        {
            Debug.Assert(types.Length > 0 && types.Length <= 3);

            var ctor = typeof(DbEntityValidationException).GetConstructor(types);
            Debug.Assert(ctor != null);

            var maxCtorParams = new object[]
                                    {
                                        "error",
                                        new[]
                                            {
                                                new DbEntityValidationResult(
                                                    new Mock<InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                               {
                                                                                                                   new DbValidationError(
                                                                                                                       "propA",
                                                                                                                       "propA is Invalid"),
                                                                                                                   new DbValidationError(
                                                                                                                       "propB",
                                                                                                                       "propB is Invalid"),
                                                                                                               }),
                                                new DbEntityValidationResult(
                                                    new Mock<InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                               {
                                                                                                                   new DbValidationError(
                                                                                                                       null,
                                                                                                                       "The entity is invalid")
                                                                                                               })
                                            },
                                        new Exception("dummy exception")
                                    };

            var ctorParams = maxCtorParams.Take(types.Length).ToArray();
            // 1st param is always a string, 3rd param is always an Exception, 
            // 2nd param can be either exception or IEnumerable<DbValidationResult> 
            // so it may need to be fixed up.
            if (types.Length == 2
                && types[1] == typeof(Exception))
            {
                ctorParams[1] = maxCtorParams[2];
            }

            var validationException = (DbEntityValidationException)ctor.Invoke(ctorParams);

            foreach (var param in ctorParams)
            {
                if (param is string)
                {
                    Assert.Equal(param, validationException.Message);
                }
                else if (param is Exception)
                {
                    Assert.Equal(param, validationException.InnerException);
                }
                else
                {
                    Debug.Assert(param is DbEntityValidationResult[]);
                    var expected = (DbEntityValidationResult[])maxCtorParams[1];
                    var actual = (DbEntityValidationResult[])param;

                    Assert.Equal(expected.Count(), actual.Count());
                    foreach (var expectedValidationResult in expected)
                    {
                        var actualValidationResult = actual.Single(r => r == expectedValidationResult);
                        Assert.Equal(expectedValidationResult.ValidationErrors.Count, actualValidationResult.ValidationErrors.Count);
                        Assert.Equal(expectedValidationResult.Entry.Entity, actualValidationResult.Entry.Entity);

                        foreach (var expectedValidationError in expectedValidationResult.ValidationErrors)
                        {
                            var actualValidationError = actualValidationResult.ValidationErrors.Single(e => e == expectedValidationError);

                            Assert.Equal(expectedValidationError.PropertyName, actualValidationError.PropertyName);
                            Assert.Equal(expectedValidationError.ErrorMessage, actualValidationError.ErrorMessage);
                        }
                    }
                }
            }
        }

        [Fact]
        public void DbEntityValidationException_serialization_and_deserialization()
        {
            var validationException = new DbEntityValidationException(
                "error",
                new[]
                    {
                        new DbEntityValidationResult(
                            new Mock<InternalEntityEntryForMock<object>>().Object, new[]
                                                                                       {
                                                                                           new DbValidationError(
                                                                                               "propA", "propA is Invalid"),
                                                                                           new DbValidationError(
                                                                                               "propB", "propB is Invalid"),
                                                                                       }),
                        new DbEntityValidationResult(
                            new Mock<InternalEntityEntryForMock<object>>().Object, new[]
                                                                                       {
                                                                                           new DbValidationError(
                                                                                               null, "The entity is invalid")
                                                                                       })
                    },
                new Exception("dummy exception")
                );

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, validationException);
                ms.Position = 0;

                var deserializedException = (DbEntityValidationException)formatter.Deserialize(ms);

                Assert.Equal("error", deserializedException.Message);
                Assert.Equal("dummy exception", deserializedException.InnerException.Message);

                var expected = validationException.EntityValidationErrors.ToArray();
                var actual = deserializedException.EntityValidationErrors.ToArray();

                Assert.Equal(expected.Count(), actual.Count());

                for (var idx = 0; idx < expected.Length; idx++)
                {
                    var expectedValidationResult = expected[idx];
                    var actualValidationResult = actual[idx];

                    // entities are not serialized
                    Assert.Null(actualValidationResult.Entry);
                    Assert.Equal(expectedValidationResult.ValidationErrors.Count, actualValidationResult.ValidationErrors.Count);
                    Assert.False(
                        expectedValidationResult.ValidationErrors.Zip(
                            actualValidationResult.ValidationErrors,
                            (actualValidationError, expectedValidationError) =>
                            actualValidationError.ErrorMessage == expectedValidationError.ErrorMessage &&
                            actualValidationError.PropertyName == expectedValidationError.PropertyName).Any(r => !r));
                }
            }
        }

        [Fact] // CodePlex 1107
        public void Deserialized_exception_can_be_serialized_and_deserialized_again()
        {
            Assert.Equal(
                "Roundabout and roundabout",
                ExceptionHelpers.SerializeAndDeserialize(
                    ExceptionHelpers.SerializeAndDeserialize(
                        new DbEntityValidationException("Roundabout and roundabout"))).Message);
        }
    }
}
