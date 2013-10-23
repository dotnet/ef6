// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal abstract class ExplorerProperty : EntityDesignExplorerEFElement
    {
        // Note: we have to keep the _isKeyProperty state in the ViewModel
        // for the following reason. When (e.g.) all the properties of an
        // ExplorerConceptualEntityType are deleted the TypedChildList.Remove()
        // method is called for every property in that EntityType. But this
        // happens _after_ those properties have been deleted from the underlying
        // model. The Remove() method depends on the ExplorerPropertyComparer class
        // which depends on the IsKeyProperty member. If IsKeyProperty just uses
        // the underlying model Property.IsKeyProperty method then we get false
        // when we should get true. This means that the BinarySearch done by
        // the Remove() method gets confused and we end up not deleting 
        // ExplorerProperty's that should be deleted.
        protected bool _isKeyProperty;

        public ExplorerProperty(EditingContext context, Property property, ExplorerEFElement parent)
            : base(context, property, parent)
        {
            if (null != property)
            {
                _isKeyProperty = property.IsKeyProperty;
            }
        }

        public override bool IsKeyProperty
        {
            get { return _isKeyProperty; }
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }

        internal override void OnModelPropertyChanged(string modelPropName)
        {
            if (modelPropName == IsKeyPropertyID)
            {
                // reset the _isKeyProperty field
                var prop = ModelItem as Property;
                Debug.Assert(
                    null != prop, "Received OnModelPropertyChanged event on an " + typeof(ExplorerProperty).Name +
                                  " node with no corresponding Property model item");
                if (null != prop)
                {
                    _isKeyProperty = prop.IsKeyProperty;
                }
            }

            base.OnModelPropertyChanged(modelPropName);
        }
    }
}
