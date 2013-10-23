// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Xml;
    using Moq;

    internal class Utils
    {
        public static StoreItemCollection CreateStoreItemCollection(string ssdl)
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(SqlProviderServices.Instance);

            IList<EdmSchemaError> errors;

            return StoreItemCollection.Create(
                new[] { XmlReader.Create(new StringReader(ssdl)) },
                null,
                mockResolver.Object,
                out errors);
        }
    }
}
