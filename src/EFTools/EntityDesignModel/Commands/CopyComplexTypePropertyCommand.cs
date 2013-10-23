// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CopyComplexTypePropertyCommand : Command
    {
        private readonly PropertyClipboardFormat _clipboardProperty;
        private readonly ComplexType _parentComplexType;
        private Property _createdProperty;

        /// <summary>
        ///     Creates a copy of Property from a Clipboard format in the specified ComplexType
        /// </summary>
        /// <param name="parentComplexType"></param>
        /// <param name="clipboardProperty"></param>
        /// <returns></returns>
        internal CopyComplexTypePropertyCommand(PropertyClipboardFormat clipboardProperty, ComplexType parentComplexType)
        {
            CommandValidation.ValidateComplexType(parentComplexType);
            _clipboardProperty = clipboardProperty;
            _parentComplexType = parentComplexType;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // get unique name for the property
            var propertyName = ModelHelper.GetUniqueName(typeof(ConceptualProperty), _parentComplexType, _clipboardProperty.PropertyName);

            if (!_clipboardProperty.IsComplexProperty)
            {
                // scalar property case
                var cmd = new CreateComplexTypePropertyCommand(
                    propertyName, _parentComplexType, _clipboardProperty.PropertyType, _clipboardProperty.IsNullable);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                _createdProperty = cmd.Property;
            }
            else
            {
                // complex property case
                // first try to find ComplexType by it's name
                var complexTypeNormalizedName = EFNormalizableItemDefaults.DefaultNameNormalizerForEDM(
                    _parentComplexType, _clipboardProperty.PropertyType);
                var items = _parentComplexType.Artifact.ArtifactSet.GetSymbolList(complexTypeNormalizedName.Symbol);
                ComplexType complexType = null;
                foreach (var efElement in items)
                {
                    // the GetSymbolList() method might return more than one element so choose the first ComplexType
                    complexType = efElement as ComplexType;
                    if (complexType != null)
                    {
                        break;
                    }
                }
                if (complexType != null)
                {
                    // if the ComplexType is found, simply use the create command
                    var cmd = new CreateComplexTypePropertyCommand(propertyName, _parentComplexType, complexType, false);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    _createdProperty = cmd.Property;
                }
                else
                {
                    // in this case we're going to create ComplexProperty with unresolved type
                    var complexProperty = new ComplexConceptualProperty(_parentComplexType, null);
                    complexProperty.ComplexType.SetXAttributeValue(_clipboardProperty.PropertyType);
                    // set the name and add to the parent entity
                    complexProperty.LocalName.Value = propertyName;
                    _parentComplexType.AddProperty(complexProperty);

                    // set other attributes of the property
                    complexProperty.Nullable.Value = BoolOrNone.FalseValue;

                    XmlModelHelper.NormalizeAndResolve(complexProperty);
                    Debug.Assert(
                        complexProperty.ComplexType.Status != BindingStatus.Known,
                        "Why didn't we find the ComplexType in the ArtifactSet previously?");
                    _createdProperty = complexProperty;
                }
            }

            // safety check
            Debug.Assert(_createdProperty != null, "We didn't get good Property out of the command");
            if (_createdProperty != null)
            {
                // set Property attributes
                var cmd2 = new SetConceptualPropertyFacetsCommand(
                    _createdProperty, _clipboardProperty.Default,
                    _clipboardProperty.ConcurrencyMode, _clipboardProperty.GetterAccessModifier, _clipboardProperty.SetterAccessModifier,
                    _clipboardProperty.MaxLength,
                    DefaultableValueBoolOrNone.GetFromNullableBool(_clipboardProperty.FixedLength), _clipboardProperty.Precision,
                    _clipboardProperty.Scale,
                    DefaultableValueBoolOrNone.GetFromNullableBool(_clipboardProperty.Unicode), _clipboardProperty.Collation);
                CommandProcessor.InvokeSingleCommand(cpc, cmd2);
            }
        }

        internal Property Property
        {
            get { return _createdProperty; }
        }
    }
}
