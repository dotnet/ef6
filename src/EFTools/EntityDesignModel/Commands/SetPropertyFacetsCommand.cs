// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Used to create a new property in the storage model.
    /// </summary>
    internal class SetPropertyFacetsCommand : Command
    {
        private Property _property;
        private readonly StringOrNone _theDefault;
        private readonly StringOrPrimitive<UInt32> _maxLength;
        private readonly BoolOrNone _fixedLength;
        private readonly StringOrPrimitive<UInt32> _precision;
        private readonly StringOrPrimitive<UInt32> _scale;
        private readonly BoolOrNone _unicode;
        private readonly StringOrNone _collation;
        private readonly string _concurrencyMode;

        /// <summary>
        ///     Sets facets on the passed in property.
        /// </summary>
        /// <param name="property">Must be a non-null storage property</param>
        /// <param name="theDefault">Optional facet</param>
        /// <param name="maxLength">Optional facet</param>
        /// <param name="fixedLength">Optional facet</param>
        /// <param name="precision">Optional facet</param>
        /// <param name="scale">Optional facet</param>
        /// <param name="unicode">Optional facet</param>
        /// <param name="collation">Optional facet</param>
        /// <param name="concurrencyMode">Optional facet</param>
        internal SetPropertyFacetsCommand(
            Property property, StringOrNone theDefault,
            StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength, StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation, string concurrencyMode)
        {
            CommandValidation.ValidateProperty(property);

            _property = property;
            _theDefault = theDefault;
            _maxLength = maxLength;
            _fixedLength = fixedLength;
            _precision = precision;
            _scale = scale;
            _unicode = unicode;
            _collation = collation;
            _concurrencyMode = concurrencyMode;
        }

        /// <summary>
        ///     Sets facets on the property being created by the passed in command.
        /// </summary>
        /// <param name="prereq">Must be non-null command creating the storage property</param>
        /// <param name="theDefault">Optional facet</param>
        /// <param name="maxLength">Optional facet</param>
        /// <param name="fixedLength">Optional facet</param>
        /// <param name="precision">Optional facet</param>
        /// <param name="scale">Optional facet</param>
        /// <param name="unicode">Optional facet</param>
        /// <param name="collation">Optional facet</param>
        /// <param name="concurrencyMode">Optional facet</param>
        internal SetPropertyFacetsCommand(
            CreatePropertyCommand prereq, StringOrNone theDefault,
            StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength, StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation, string concurrencyMode)
        {
            ValidatePrereqCommand(prereq);

            _theDefault = theDefault;
            _maxLength = maxLength;
            _fixedLength = fixedLength;
            _precision = precision;
            _scale = scale;
            _unicode = unicode;
            _collation = collation;
            _concurrencyMode = concurrencyMode;

            AddPreReqCommand(prereq);
        }

        /// <summary>
        ///     Sets facets on the property being created by the passed in command.
        /// </summary>
        /// <param name="prereq">Must be non-null command creating the storage property</param>
        /// <param name="theDefault">Optional facet</param>
        /// <param name="maxLength">Optional facet</param>
        /// <param name="fixedLength">Optional facet</param>
        /// <param name="precision">Optional facet</param>
        /// <param name="scale">Optional facet</param>
        /// <param name="unicode">Optional facet</param>
        /// <param name="collation">Optional facet</param>
        /// <param name="concurrencyMode">Optional facet</param>
        internal SetPropertyFacetsCommand(
            CreateComplexTypePropertyCommand prereq, StringOrNone theDefault,
            StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength, StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation, string concurrencyMode)
        {
            ValidatePrereqCommand(prereq);

            _theDefault = theDefault;
            _maxLength = maxLength;
            _fixedLength = fixedLength;
            _precision = precision;
            _scale = scale;
            _unicode = unicode;
            _collation = collation;
            _concurrencyMode = concurrencyMode;

            AddPreReqCommand(prereq);
        }

        internal Property Property
        {
            get { return _property; }
        }

        protected override void ProcessPreReqCommands()
        {
            if (_property == null)
            {
                // The pre requisite command could be either createPropertyCommand or createComplexTypePropertyCommand
                var createPropertyCommand = GetPreReqCommand(CreatePropertyCommand.PrereqId) as CreatePropertyCommand;
                if (createPropertyCommand != null)
                {
                    _property = createPropertyCommand.CreatedProperty;
                }
                else
                {
                    // check if the prereq command is type of createcomplextypepropertycommand.
                    var createComplexTypePropertyCommand =
                        GetPreReqCommand(CreateComplexTypePropertyCommand.PrereqId) as CreateComplexTypePropertyCommand;
                    if (createComplexTypePropertyCommand != null)
                    {
                        _property = createComplexTypePropertyCommand.Property;
                    }
                }
                Debug.Assert(_property != null, "We didn't get a good property out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _property is null.");
            }

            // if values are set to NoneValue then explictly remove the attribute,
            // if values are set to null then leave the attribute as is
            if (_theDefault != null)
            {
                _property.DefaultValue.Value = (_theDefault.Equals(StringOrNone.NoneValue) ? null : _theDefault);
            }
            if (_maxLength != null)
            {
                _property.MaxLength.Value = (_maxLength.Equals(DefaultableValueUIntOrNone.NoneValue) ? null : _maxLength);
            }
            if (_fixedLength != null)
            {
                _property.FixedLength.Value = (_fixedLength.Equals(BoolOrNone.NoneValue) ? null : _fixedLength);
            }
            if (_precision != null)
            {
                _property.Precision.Value = (_precision.Equals(DefaultableValueUIntOrNone.NoneValue) ? null : _precision);
            }
            if (_scale != null)
            {
                _property.Scale.Value = (_scale.Equals(DefaultableValueUIntOrNone.NoneValue) ? null : _scale);
            }
            if (_unicode != null)
            {
                _property.Unicode.Value = (_unicode.Equals(BoolOrNone.NoneValue) ? null : _unicode);
            }
            if (_collation != null)
            {
                _property.Collation.Value = (_collation.Equals(StringOrNone.NoneValue) ? null : _collation);
            }
            if (_concurrencyMode != null)
            {
                _property.ConcurrencyMode.Value = _concurrencyMode;
            }
        }
    }
}
