// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.ObjectModel;
    using Xunit;

    public class ItemCollectionTests
    {
        [Fact]
        public void InternalGetItems_calls_GenericGetItems()
        {
            Assert.IsType<ReadOnlyCollection<GlobalItem>>(new AnItemCollection().InternalGetItems(typeof(GlobalItem)));
        }

        public class AnItemCollection : ItemCollection
        {
        }
    }
}
