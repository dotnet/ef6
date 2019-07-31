// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public abstract class DbMemberEntryVerifier<TEntry, TInternalEntry>
        where TInternalEntry : class
    {
        protected abstract TEntry CreateEntry(TInternalEntry internalEntry);
        protected abstract Mock<TInternalEntry> CreateInternalEntryMock();

        public void VerifyGetter<TProperty, TMockProperty>(
            Func<TEntry, TProperty> getterFunc,
            Expression<Func<TInternalEntry, TMockProperty>> mockGetterFunc)
        {
            Assert.NotNull(getterFunc);
            Assert.NotNull(mockGetterFunc);

            var internalEntryMock = CreateInternalEntryMock();
            var entry = CreateEntry(internalEntryMock.Object);

            try
            {
                getterFunc(entry);
            }
            catch (Exception)
            {
            }

            internalEntryMock.Verify(mockGetterFunc, Times.Once());
        }

        public void VerifySetter(Action<TEntry> setter, Action<TInternalEntry> mockSetter)
        {
            Assert.NotNull(setter);
            Assert.NotNull(mockSetter);

            var internalEntryMock = CreateInternalEntryMock();
            var entry = CreateEntry(internalEntryMock.Object);

            try
            {
                setter(entry);
            }
            catch (Exception)
            {
            }

            internalEntryMock.VerifySet(mockSetter, Times.Once());
        }

        public void VerifyMethod(Action<TEntry> methodInvoke, Expression<Action<TInternalEntry>> mockMethodInvoke)
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var internalEntryMock = CreateInternalEntryMock();
            var entry = CreateEntry(internalEntryMock.Object);

            try
            {
                methodInvoke(entry);
            }
            catch (Exception)
            {
            }

            internalEntryMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
