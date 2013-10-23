// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.ViewModels
{
    using System;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Xunit;

    public class RuntimeConfigViewModelTests
    {
        [Fact]
        public void Ctor_initializes_correctly_when_net35()
        {
            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion3_5,
                installedEntityFrameworkVersion: null,
                isModernProviderAvailable: false);

            Assert.Equal(2, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(RuntimeVersion.Latest, first.Version);
            Assert.True(first.Disabled);
            Assert.False(first.IsDefault);

            var last = viewModel.EntityFrameworkVersions.Last();
            Assert.Equal(RuntimeVersion.Version1, last.Version);
            Assert.False(last.Disabled);
            Assert.True(last.IsDefault);

            Assert.Equal(RuntimeConfigState.Normal, viewModel.State);
            Assert.Equal(Resources.RuntimeConfig_Net35, viewModel.Message);
            Assert.Null(viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_installed_version_below_six()
        {
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4;
            var installedEntityFrameworkVersion = new Version(4, 0, 0, 0);
            var isModernProviderAvailable = false;

            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion,
                installedEntityFrameworkVersion,
                isModernProviderAvailable);

            Assert.Equal(2, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(RuntimeVersion.Latest, first.Version);
            Assert.True(first.Disabled);
            Assert.False(first.IsDefault);

            var last = viewModel.EntityFrameworkVersions.Last();
            Assert.Equal(new Version(4, 4, 0, 0), last.Version);
            Assert.False(last.Disabled);
            Assert.True(last.IsDefault);

            Assert.Equal(RuntimeConfigState.Normal, viewModel.State);
            Assert.Equal(Resources.RuntimeConfig_BelowSixInstalled, viewModel.Message);
            Assert.Null(viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_installed_version_six()
        {
            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion4_5,
                installedEntityFrameworkVersion: RuntimeVersion.Version6,
                isModernProviderAvailable: true);

            Assert.Equal(1, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(RuntimeVersion.Version6, first.Version);
            Assert.False(first.Disabled);
            Assert.True(first.IsDefault);

            Assert.Equal(RuntimeConfigState.Skip, viewModel.State);
            Assert.Null(viewModel.Message);
            Assert.Null(viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_installed_version_over_six()
        {
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4_5;
            var installedEntityFrameworkVersion = new Version(7, 0, 0, 0);
            var isModernProviderAvailable = true;

            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion,
                installedEntityFrameworkVersion,
                isModernProviderAvailable);

            Assert.Equal(1, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(installedEntityFrameworkVersion, first.Version);
            Assert.False(first.Disabled);
            Assert.True(first.IsDefault);

            Assert.Equal(RuntimeConfigState.Skip, viewModel.State);
            Assert.Null(viewModel.Message);
            Assert.Null(viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_installed_version_six_but_no_modern_provider()
        {
            var targetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4;
            var installedEntityFrameworkVersion = new Version(7, 0, 0, 0);
            var isModernProviderAvailable = false;

            var viewModel = new RuntimeConfigViewModel(
                targetFrameworkVersion,
                installedEntityFrameworkVersion,
                isModernProviderAvailable);

            Assert.Equal(1, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(installedEntityFrameworkVersion, first.Version);
            Assert.True(first.Disabled);
            Assert.True(first.IsDefault);

            Assert.Equal(RuntimeConfigState.Error, viewModel.State);
            Assert.Equal(Resources.RuntimeConfig_SixInstalledButNoProvider, viewModel.Message);
            Assert.Equal(Resources.RuntimeConfig_LearnProvidersUrl, viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_no_modern_provider()
        {
            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion4,
                installedEntityFrameworkVersion: null,
                isModernProviderAvailable: false);

            Assert.Equal(2, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(RuntimeVersion.Latest, first.Version);
            Assert.True(first.Disabled);
            Assert.False(first.IsDefault);

            var last = viewModel.EntityFrameworkVersions.Last();
            Assert.Equal(RuntimeVersion.Version5Net40, last.Version);
            Assert.False(last.Disabled);
            Assert.True(last.IsDefault);

            Assert.Equal(RuntimeConfigState.Normal, viewModel.State);
            Assert.Equal(Resources.RuntimeConfig_NoProvider, viewModel.Message);
            Assert.Equal(Resources.RuntimeConfig_LearnProvidersUrl, viewModel.HelpUrl);
        }

        [Fact]
        public void Ctor_initializes_correctly_when_modern_provider()
        {
            var viewModel = new RuntimeConfigViewModel(
                targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion4_5,
                installedEntityFrameworkVersion: null,
                isModernProviderAvailable: true);

            Assert.Equal(2, viewModel.EntityFrameworkVersions.Count());

            var first = viewModel.EntityFrameworkVersions.First();
            Assert.Equal(RuntimeVersion.Latest, first.Version);
            Assert.False(first.Disabled);
            Assert.True(first.IsDefault);

            var last = viewModel.EntityFrameworkVersions.Last();
            Assert.Equal(RuntimeVersion.Version5Net45, last.Version);
            Assert.False(last.Disabled);
            Assert.False(last.IsDefault);

            Assert.Equal(RuntimeConfigState.Normal, viewModel.State);
            Assert.Equal(Resources.RuntimeConfig_TargetingHint, viewModel.Message);
            Assert.Equal(Resources.RuntimeConfig_LearnTargetingUrl, viewModel.HelpUrl);
        }
    }
}
