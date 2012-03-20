namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;

    internal sealed class MappingContext
    {
        private readonly ModelConfiguration _modelConfiguration;
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly EdmModel _model;
        private readonly AttributeProvider _attributeProvider;

        public MappingContext(
            ModelConfiguration modelConfiguration,
            ConventionsConfiguration conventionsConfiguration,
            EdmModel model)
        {
            Contract.Requires(modelConfiguration != null);
            Contract.Requires(conventionsConfiguration != null);
            Contract.Requires(model != null);

            _modelConfiguration = modelConfiguration;
            _conventionsConfiguration = conventionsConfiguration;
            _model = model;
            _attributeProvider = new AttributeProvider();
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
    }
}