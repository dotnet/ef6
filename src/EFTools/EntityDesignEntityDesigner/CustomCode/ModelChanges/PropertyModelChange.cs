// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;

    internal abstract class PropertyModelChange : ViewModelChange
    {
        private readonly Property _property;

        protected PropertyModelChange(Property property)
        {
            _property = property;
        }

        public Property Property
        {
            get { return _property; }
        }
    }
}
