// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using Microsoft.VisualStudio.ComponentModelHost;
    using Moq;
    using Xunit;

    public class IComponentModelExtensionsTests
    {
        [Fact]
        public void GetService_calls_generic_version()
        {
            var componentModelMock = new Mock<IComponentModel>();
            var componentModel = componentModelMock.Object;

            componentModel.GetService(typeof(string));

            componentModelMock.Verify(cm => cm.GetService<string>(), Times.Once());
        }
    }
}
