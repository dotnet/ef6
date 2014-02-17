// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Xunit;

    public class ModelObjectItemWizardTests
    {
        [Fact]
        public void ShouldAddProjectItem_returns_true_for_ModelFirst()
        {
            Assert.True(
                new ModelObjectItemWizard(
                    new ModelBuilderSettings { GenerationOption = ModelGenerationOption.EmptyModel })
                    .ShouldAddProjectItem("FakeProjectItemName"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_true_for_DatabaseFirst()
        {
            Assert.True(
                new ModelObjectItemWizard(
                    new ModelBuilderSettings { GenerationOption = ModelGenerationOption.GenerateFromDatabase })
                    .ShouldAddProjectItem("FakeProjectItemName"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_false_for_EmptyModelCodeFirst()
        {
            Assert.False(
                new ModelObjectItemWizard(
                    new ModelBuilderSettings { GenerationOption = ModelGenerationOption.EmptyModelCodeFirst })
                    .ShouldAddProjectItem("FakeProjectItemName"));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_false_for_CodeFirstFromDatabase()
        {
            Assert.False(
                new ModelObjectItemWizard(
                    new ModelBuilderSettings { GenerationOption = ModelGenerationOption.CodeFirstFromDatabase })
                    .ShouldAddProjectItem("FakeProjectItemName"));
        }
    }
}
