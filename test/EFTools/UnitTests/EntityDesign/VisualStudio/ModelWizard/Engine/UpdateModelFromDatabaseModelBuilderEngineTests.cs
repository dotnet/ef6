// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class UpdateModelFromDatabaseModelBuilderEngineTests
    {
        public class UpdateDesignerInfoTests
        {
            private class UpdateModelFromDatabaseModelBuilderEngineFake
                : UpdateModelFromDatabaseModelBuilderEngine
            {
                internal void UpdateDesignerInfoInvoker(EdmxHelper edmxHelper, ModelBuilderSettings settings)
                {
                    UpdateDesignerInfo(edmxHelper, settings);
                }
            }

            [Fact]
            public void UpdateDesignerInfo_updates_no_properties_in_designer_section()
            {
                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                new UpdateModelFromDatabaseModelBuilderEngineFake()
                    .UpdateDesignerInfoInvoker(mockEdmxHelper.Object, new ModelBuilderSettings());

                mockEdmxHelper
                    .Verify(h => h.UpdateDesignerOptionProperty(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            }
        }
    }
}
