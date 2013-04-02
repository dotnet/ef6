// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;

    internal sealed class MappingContext
    {
        private readonly ModelConfiguration _modelConfiguration;
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly EdmModel _model;
        private readonly AttributeProvider _attributeProvider;
        private readonly DbModelBuilderVersion _modelBuilderVersion;

        public MappingContext(
            ModelConfiguration modelConfiguration,
            ConventionsConfiguration conventionsConfiguration,
            EdmModel model,
            DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
        {
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(conventionsConfiguration);
            DebugCheck.NotNull(model);

            _modelConfiguration = modelConfiguration;
            _conventionsConfiguration = conventionsConfiguration;
            _model = model;
            _modelBuilderVersion = modelBuilderVersion;
        }

        public ModelConfiguration ModelConfiguration
        {
            get { return _modelConfiguration; }
        }

        public ConventionsConfiguration ConventionsConfiguration
        {
            get { return _conventionsConfiguration; }
        }

        public EdmModel Model
        {
            get { return _model; }
        }

        public AttributeProvider AttributeProvider
        {
            get { return _attributeProvider; }
        }

        public DbModelBuilderVersion ModelBuilderVersion
        {
            get { return _modelBuilderVersion; }
        }
    }
}
