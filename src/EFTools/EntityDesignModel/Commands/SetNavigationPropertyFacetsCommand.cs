// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Navigation Properties have some optional facets that can be set and you use this command to set them.
    /// </summary>
    internal class SetNavigationPropertyFacetsCommand : Command
    {
        private readonly NavigationProperty _property;
        private readonly string _getterAccessModifier;
        private readonly string _setterAccessModifier;

        /// <summary>
        ///     Sets facets on the passed in property
        /// </summary>
        /// <param name="property">Must be a non-null conceptual property</param>
        /// <param name="getterAccessModifier">Optional facet</param>
        /// <param name="setterAccessModifier">Optional facet</param>
        internal SetNavigationPropertyFacetsCommand(NavigationProperty property, string getterAccessModifier, string setterAccessModifier)
        {
            CommandValidation.ValidateNavigationProperty(property);
            _property = property;
            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_property != null, "InvokeInternal is called when _property is null.");
            if (_property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _property is null.");
            }

            if (!String.IsNullOrEmpty(_getterAccessModifier))
            {
                _property.Getter.Value = _getterAccessModifier;
            }

            if (!String.IsNullOrEmpty(_setterAccessModifier))
            {
                _property.Setter.Value = _setterAccessModifier;
            }
        }
    }
}
