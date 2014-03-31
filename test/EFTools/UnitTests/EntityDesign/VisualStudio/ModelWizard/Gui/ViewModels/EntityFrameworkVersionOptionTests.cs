// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.ViewModels
{
    using System;
    using Xunit;

    public class EntityFrameworkVersionOptionTests
    {
        [Fact]
        public void Ctor_sets_name_and_version()
        {
            var version = new Version(4, 3, 0, 0);
            var option = new EntityFrameworkVersionOption(version);

            Assert.Equal(
                string.Format(Resources.EntityFrameworkVersionName, new Version(version.Major, version.Minor)),
                option.Name);
            Assert.Same(version, option.Version);
        }

        [Fact]
        public void Ctor_sets_6_x_name_for_EF6()
        {
            var version = new Version(6, 0, 0, 0);
            var option = new EntityFrameworkVersionOption(version);

            Assert.Equal(
                string.Format(Resources.EntityFrameworkVersionName, "6.x"),
                option.Name);
            Assert.Same(version, option.Version);
        }

    }
}
