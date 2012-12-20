// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    internal sealed class EdmModelValidationContext
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        private EdmModel _model;

        public EdmModelValidationContext(bool validateSyntax)
        {
            ValidateSyntax = validateSyntax;
        }

        public bool ValidateSyntax { get; set; }
        public double ValidationContextVersion { get; set; }

        public void RaiseDataModelValidationEvent(DataModelErrorEventArgs error)
        {
            if (OnError != null)
            {
                OnError(this, error);
            }
        }

        public void Validate(EdmModel model)
        {
            DebugCheck.NotNull(model);

            _model = model;

            ValidationContextVersion = model.Version;

            EdmModelValidator.Validate(model, this);
        }

        public EdmModel Model
        {
            get { return _model; }
            set
            {
                DebugCheck.NotNull(value);

                _model = value;
            }
        }

        public void AddError(IMetadataItem item, string propertyName, string errorMessage)
        {
            RaiseDataModelValidationEvent(
                new DataModelErrorEventArgs
                    {
                        ErrorMessage = errorMessage,
                        Item = item,
                        PropertyName = propertyName,
                    }
                );
        }
    }
}
