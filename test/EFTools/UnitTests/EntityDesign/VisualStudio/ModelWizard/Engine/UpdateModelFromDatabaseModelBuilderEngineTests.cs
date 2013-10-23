// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
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
                #region not important

                public UpdateModelFromDatabaseModelBuilderEngineFake()
                    : base(new Mock<ModelBuilderEngineHostContext>().Object,
                        new ModelBuilderSettings(),
                        new Uri("http://My.Fake.Uri"))
                {
                }

                protected override void AddErrors(IEnumerable<EdmSchemaError> errors)
                {
                    throw new NotImplementedException();
                }

                internal override IEnumerable<EdmSchemaError> Errors
                {
                    get { throw new NotImplementedException(); }
                }

                protected override Uri Uri
                {
                    get { throw new NotImplementedException(); }
                }

                internal override XDocument XDocument
                {
                    get { throw new NotImplementedException(); }
                }

                #endregion

                internal void UpdateDesignerInfoInvoker(EdmxHelper edmxHelper)
                {
                    UpdateDesignerInfo(edmxHelper);
                }
            }

            [Fact]
            public void UpdateDesignerInfo_updates_no_properties_in_designer_section()
            {
                var mockEdmxHelper = new Mock<EdmxHelper>(new XDocument());
                new UpdateModelFromDatabaseModelBuilderEngineFake().UpdateDesignerInfoInvoker(mockEdmxHelper.Object);

                mockEdmxHelper
                    .Verify(h => h.UpdateDesignerOptionProperty(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            }
        }
    }
}
