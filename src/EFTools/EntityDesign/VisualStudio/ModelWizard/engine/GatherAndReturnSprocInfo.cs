// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    // <summary>
    //     Helper class to pass information to background thread in ProgressDialog
    // </summary>
    internal class GatherAndReturnSprocInfo
    {
        private readonly IList<EntityStoreSchemaFilterEntry> _newFunctionEntries;
        private readonly ModelBuilderSettings _modelBuilderSettings;

        internal GatherAndReturnSprocInfo(IList<EntityStoreSchemaFilterEntry> newFunctionEntries, ModelBuilderSettings modelBuilderSettings)
        {
            Debug.Assert(null != newFunctionEntries, "newFunctionEntries should not be null");
            Debug.Assert(null != modelBuilderSettings, "modelBuilderSettings should not be null");
            _newFunctionEntries = newFunctionEntries;
            _modelBuilderSettings = modelBuilderSettings;
        }

        internal IList<EntityStoreSchemaFilterEntry> NewFunctionEntries
        {
            get { return _newFunctionEntries; }
        }

        internal ModelBuilderSettings ModelBuilderSettings
        {
            get { return _modelBuilderSettings; }
        }
    }
}
