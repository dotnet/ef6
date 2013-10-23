// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Conceptual Properties have some optional facets that can be set and you use this command to set them.
    /// </summary>
    internal class SetConceptualPropertyFacetsCommand : SetPropertyFacetsCommand
    {
        private readonly string _getterAccessModifier;
        private readonly string _setterAccessModifier;

        /// <summary>
        ///     Sets facets on the passed in property
        /// </summary>
        /// <param name="property">Must be a non-null conceptual property</param>
        /// <param name="theDefault">Optional facet</param>
        /// <param name="concurrencyMode">Optional facet</param>
        /// <param name="getterAccessModifier">Optional facet</param>
        /// <param name="setterAccessModifier">Optional facet</param>
        internal SetConceptualPropertyFacetsCommand(
            Property property, StringOrNone theDefault, string concurrencyMode, string getterAccessModifier,
            string setterAccessModifier, StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength, StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation)
            : base(property, theDefault, maxLength, fixedLength, precision, scale, unicode, collation, concurrencyMode)
        {
            CommandValidation.ValidateConceptualProperty(property);

            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
        }

        /// <summary>
        ///     Sets facets on the property being created by the passed in command.
        /// </summary>
        /// <param name="prereq">Must be non-null command creating the conceptual property</param>
        /// <param name="theDefault">Optional facet</param>
        /// <param name="concurrencyMode">Optional facet</param>
        /// <param name="getterAccessModifier">Optional facet</param>
        /// <param name="setterAccessModifier">Optional facet</param>
        internal SetConceptualPropertyFacetsCommand(
            CreatePropertyCommand prereq, StringOrNone theDefault, string concurrencyMode, string getterAccessModifier,
            string setterAccessModifier, StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength, StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation)
            : base(prereq, theDefault, maxLength, fixedLength, precision, scale, unicode, collation, concurrencyMode)
        {
            _getterAccessModifier = getterAccessModifier;
            _setterAccessModifier = setterAccessModifier;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(Property != null, "InvokeInternal is called when Property is null.");
            if (Property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Property is null");
            }

            if (!String.IsNullOrEmpty(_getterAccessModifier))
            {
                Property.Getter.Value = _getterAccessModifier;
            }

            if (!String.IsNullOrEmpty(_setterAccessModifier))
            {
                Property.Setter.Value = _setterAccessModifier;
            }

            base.InvokeInternal(cpc);
        }
    }
}
