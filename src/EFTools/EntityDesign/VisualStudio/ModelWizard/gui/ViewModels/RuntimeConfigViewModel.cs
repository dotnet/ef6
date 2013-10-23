// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;

    internal class RuntimeConfigViewModel
    {
        private readonly string _helpUrl;
        private readonly string _message;
        private readonly RuntimeConfigState _state;
        private readonly ICollection<EntityFrameworkVersionOption> _entityFrameworkVersions = new List<EntityFrameworkVersionOption>();

        public RuntimeConfigViewModel(
            Version targetNetFrameworkVersion,
            Version installedEntityFrameworkVersion,
            bool isModernProviderAvailable)
        {
            if (targetNetFrameworkVersion == NetFrameworkVersioningHelper.NetFrameworkVersion3_5)
            {
                _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Latest) { Disabled = true });
                _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Version1) { IsDefault = true });

                _message = Resources.RuntimeConfig_Net35;
            }
            else
            {
                if (installedEntityFrameworkVersion != null)
                {
                    if (installedEntityFrameworkVersion < RuntimeVersion.Version6)
                    {
                        _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Latest) { Disabled = true });
                        if (installedEntityFrameworkVersion == RuntimeVersion.Version4)
                        {
                            // EF V4 (i.e. System.Data.Entity) is installed, but when we go through the wizard we
                            // will end up with DbContext templates which will automatically install V5 of the
                            // runtime - so update the options so that the only choice other than V6 is V5
                            installedEntityFrameworkVersion =
                                targetNetFrameworkVersion == NetFrameworkVersioningHelper.NetFrameworkVersion4
                                    ? RuntimeVersion.Version5Net40
                                    : RuntimeVersion.Version5Net45;
                        }
                        _entityFrameworkVersions.Add(
                            new EntityFrameworkVersionOption(installedEntityFrameworkVersion, targetNetFrameworkVersion)
                                {
                                    IsDefault = true
                                });

                        _message = Resources.RuntimeConfig_BelowSixInstalled;
                    }
                    else if (!isModernProviderAvailable)
                    {
                        _entityFrameworkVersions.Add(
                            new EntityFrameworkVersionOption(installedEntityFrameworkVersion)
                                {
                                    Disabled = true,
                                    IsDefault = true
                                });

                        _state = RuntimeConfigState.Error;
                        _message = Resources.RuntimeConfig_SixInstalledButNoProvider;
                        _helpUrl = Resources.RuntimeConfig_LearnProvidersUrl;
                    }
                    else
                    {
                        _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(installedEntityFrameworkVersion) { IsDefault = true });

                        _state = RuntimeConfigState.Skip;
                    }
                }
                else
                {
                    if (isModernProviderAvailable)
                    {
                        _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Latest) { IsDefault = true });
                        _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Version5(targetNetFrameworkVersion)));

                        _message = Resources.RuntimeConfig_TargetingHint;
                        _helpUrl = Resources.RuntimeConfig_LearnTargetingUrl;
                    }
                    else
                    {
                        _entityFrameworkVersions.Add(new EntityFrameworkVersionOption(RuntimeVersion.Latest) { Disabled = true });
                        _entityFrameworkVersions.Add(
                            new EntityFrameworkVersionOption(RuntimeVersion.Version5(targetNetFrameworkVersion))
                                {
                                    IsDefault = true
                                });

                        _message = Resources.RuntimeConfig_NoProvider;
                        _helpUrl = Resources.RuntimeConfig_LearnProvidersUrl;
                    }
                }
            }

            Debug.Assert(_entityFrameworkVersions.Count != 0, "_entityFrameworkVersions is empty.");
            Debug.Assert(_entityFrameworkVersions.Any(v => v.IsDefault), "No element of _entityFrameworkVersions is the default.");
        }

        public RuntimeConfigState State
        {
            get { return _state; }
        }

        public string Message
        {
            get { return _message; }
        }

        public string HelpUrl
        {
            get { return _helpUrl; }
        }

        public IEnumerable<EntityFrameworkVersionOption> EntityFrameworkVersions
        {
            get { return _entityFrameworkVersions; }
        }
    }
}
