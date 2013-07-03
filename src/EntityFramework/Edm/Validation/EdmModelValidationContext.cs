// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal sealed class EdmModelValidationContext
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        private readonly EdmModel _model;
        private readonly bool _validateSyntax;

        public EdmModelValidationContext(EdmModel model, bool validateSyntax)
        {
            DebugCheck.NotNull(model);

            _model = model;
            _validateSyntax = validateSyntax;
        }

        public bool ValidateSyntax
        {
            get { return _validateSyntax; }
        }

        public EdmModel Model
        {
            get { return _model; }
        }

        public bool IsCSpace
        {
            get { return _model.Containers.First().DataSpace == DataSpace.CSpace; }
        }

        public void AddError(MetadataItem item, string propertyName, string errorMessage)
        {
            DebugCheck.NotNull(item);
            DebugCheck.NotEmpty(errorMessage);

            RaiseDataModelValidationEvent(
                new DataModelErrorEventArgs
                    {
                        ErrorMessage = errorMessage,
                        Item = item,
                        PropertyName = propertyName,
                    }
                );
        }

        private void RaiseDataModelValidationEvent(DataModelErrorEventArgs error)
        {
            DebugCheck.NotNull(error);

            if (OnError != null)
            {
                OnError(this, error);
            }
        }
    }
}
