// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CopyPropertyCommand : CopyAnnotatableElementCommand
    {
        private readonly PropertyClipboardFormat _clipboardProperty;
        private readonly EntityType _entityType;
        private readonly ComplexType _complexType;
        private Property _createdProperty;
        private readonly InsertPropertyPosition _insertPosition;

        /// <summary>
        ///     Creates a copy of Property from a Clipboard format in the specified EntityType
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="clipboardProperty"></param>
        /// <returns></returns>
        internal CopyPropertyCommand(PropertyClipboardFormat clipboardProperty, EntityType entity)
            : this(clipboardProperty, entity, null)
        {
        }

        /// <summary>
        ///     Creates a copy of Property from a Clipboard format in the specified EntityType and Position.
        /// </summary>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="clipboardProperty"></param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        /// <returns></returns>
        internal CopyPropertyCommand(PropertyClipboardFormat clipboardProperty, EntityType entity, InsertPropertyPosition insertPosition)
        {
            if (_insertPosition != null)
            {
                Debug.Assert(
                    entity.EntityModel.IsCSDL, "You can only set insertPosition parameter if the EntityType is a ConceptualEntityType.");
                Debug.Assert(
                    insertPosition.InsertAtProperty != null && insertPosition.InsertAtProperty.EntityType == entity,
                    "Could not create complex property in the given insertPosition because insertPosition's Entity-Type is not the same as the entity-type which the property will be created in.");
            }

            CommandValidation.ValidateEntityType(entity);
            _clipboardProperty = clipboardProperty;
            _entityType = entity;
            _insertPosition = insertPosition;
        }

        /// <summary>
        ///     Creates a copy of Property from a Clipboard format in the specified ComplexType
        /// </summary>
        /// <param name="complexType"></param>
        /// <param name="clipboardProperty"></param>
        /// <returns></returns>
        internal CopyPropertyCommand(PropertyClipboardFormat clipboardProperty, ComplexType complexType)
        {
            CommandValidation.ValidateComplexType(complexType);
            _clipboardProperty = clipboardProperty;
            _complexType = complexType;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_entityType != null || _complexType != null, "Undefined parent type");

            if (_entityType != null)
            {
                var propertyName = ModelHelper.GetUniqueConceptualPropertyName(_clipboardProperty.PropertyName, _entityType);

                if (_clipboardProperty.IsComplexProperty)
                {
                    _createdProperty = CreateComplexPropertyCommand.CreateComplexProperty(
                        cpc, propertyName, _entityType, _clipboardProperty.PropertyType,
                        _clipboardProperty.ConcurrencyMode, _clipboardProperty.GetterAccessModifier, _clipboardProperty.SetterAccessModifier,
                        _insertPosition);
                }
                else if (_clipboardProperty.IsConceptualProperty)
                {
                    Debug.Assert(_entityType.EntityModel.IsCSDL, "This should be a c-side Entity");
                    if (_entityType.EntityModel.IsCSDL)
                    {
                        _createdProperty = CreatePropertyCommand.CreateConceptualProperty(
                            cpc, propertyName, _entityType as ConceptualEntityType, _clipboardProperty.PropertyType,
                            _clipboardProperty.IsNullable,
                            _clipboardProperty.Default, _clipboardProperty.ConcurrencyMode, _clipboardProperty.GetterAccessModifier,
                            _clipboardProperty.SetterAccessModifier,
                            _clipboardProperty.MaxLength, _clipboardProperty.FixedLength, _clipboardProperty.Precision,
                            _clipboardProperty.Scale, _clipboardProperty.Unicode, _clipboardProperty.Collation,
                            _clipboardProperty.StoreGeneratedPattern, _insertPosition);
                    }
                }
                else
                {
                    Debug.Assert(_entityType.EntityModel.IsCSDL == false, "This should be a s-side Entity");
                    if (!_entityType.EntityModel.IsCSDL)
                    {
                        _createdProperty = CreatePropertyCommand.CreateStorageProperty(
                            cpc, propertyName, _entityType as StorageEntityType, _clipboardProperty.PropertyType,
                            _clipboardProperty.IsNullable,
                            _clipboardProperty.Default, _clipboardProperty.MaxLength,
                            DefaultableValueBoolOrNone.GetFromNullableBool(_clipboardProperty.FixedLength), _clipboardProperty.Precision,
                            _clipboardProperty.Scale,
                            DefaultableValueBoolOrNone.GetFromNullableBool(_clipboardProperty.Unicode), _clipboardProperty.Collation,
                            _clipboardProperty.ConcurrencyMode);
                    }
                }

                if (_clipboardProperty.IsKeyProperty)
                {
                    var setKey = new SetKeyPropertyCommand(_createdProperty, true);
                    CommandProcessor.InvokeSingleCommand(cpc, setKey);
                }

                AddAnnotations(_clipboardProperty, _createdProperty);
            }
            else
            {
                var cmd = new CopyComplexTypePropertyCommand(_clipboardProperty, _complexType);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                _createdProperty = cmd.Property;
            }
        }

        internal Property Property
        {
            get { return _createdProperty; }
        }
    }
}
