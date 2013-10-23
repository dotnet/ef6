// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command creates a new complex property and lets you define the name and type of the property.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateComplexPropertyCommand : Command
    {
        internal string Name { get; set; }
        internal EntityType EntityType { get; set; }
        internal ComplexType ComplexType { get; set; }
        private readonly string _typeName;
        private ComplexConceptualProperty _createdProperty;
        private readonly InsertPropertyPosition _insertPosition;

        /// <summary>
        ///     Creates a ComplexProperty in the passed in entity using ComplexType as a type for it.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="type">The type of the property (can be null)</param>
        internal CreateComplexPropertyCommand(string name, EntityType entityType, ComplexType type)
            : this(name, entityType, type, null)
        {
            // nothing.
        }

        internal CreateComplexPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates a ComplexProperty in the passed in entity using ComplexType as a type for it.
        ///     The complex property will be placed in the specified insertPosition parameter.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="type">The type of the property (can be null)</param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        internal CreateComplexPropertyCommand(string name, EntityType entityType, ComplexType type, InsertPropertyPosition insertPosition)
        {
            if (insertPosition != null)
            {
                Debug.Assert(
                    insertPosition.InsertAtProperty != null && insertPosition.InsertAtProperty.EntityType == entityType,
                    "Could not create complex property in the given insertPosition because insertPosition's Entity-Type is not the same as the entity-type which the property will be created in.");
            }

            ValidateString(name);
            CommandValidation.ValidateEntityType(entityType);

            Name = name;
            EntityType = entityType;
            ComplexType = type;
            _insertPosition = insertPosition;
        }

        /// <summary>
        ///     Creates a ComplexProperty in the passed in entity. Use this one if you only have ComplexType ref name.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="typeName">String representing the type of the property (needs to be a ref name to a ComplexType)</param>
        internal CreateComplexPropertyCommand(string name, EntityType entityType, string typeName)
            : this(name, entityType, typeName, null)
        {
            // nothing.
        }

        /// <summary>
        ///     Creates a ComplexProperty in the specified position in the entity.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="typeName">String representing the type of the property (needs to be a ref name to a ComplexType)</param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        internal CreateComplexPropertyCommand(string name, EntityType entityType, string typeName, InsertPropertyPosition insertPosition)
        {
            if (insertPosition != null)
            {
                Debug.Assert(
                    insertPosition.InsertAtProperty != null && insertPosition.InsertAtProperty.EntityType == entityType,
                    "Could not create complex property in the given insertPosition because insertPosition's Entity-Type is not the same as the entity-type which the property will be created in.");
            }

            ValidateString(name);
            CommandValidation.ValidateEntityType(entityType);

            Name = name;
            EntityType = entityType;
            _typeName = typeName;
            _insertPosition = insertPosition;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(EntityType != null, "InvokeInternal is called when EntityType is null.");
            if (EntityType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when EntityType is null");
            }

            // check for uniqueness
            string msg;
            if (!ModelHelper.ValidateEntityPropertyName(EntityType, Name, true, out msg))
            {
                throw new CommandValidationFailedException(msg);
            }

            // create the property
            _createdProperty = new ComplexConceptualProperty(EntityType, null, _insertPosition);

            // set the name and add to the parent entity
            _createdProperty.LocalName.Value = Name;
            if (ComplexType != null)
            {
                _createdProperty.ComplexType.SetRefName(ComplexType);
            }
            else if (!String.IsNullOrEmpty(_typeName))
            {
                // separate this name into the namespace and name parts
                string typeNamespace;
                string typeLocalName;
                EFNormalizableItemDefaults.SeparateRefNameIntoParts(_typeName, out typeNamespace, out typeLocalName);

                // look to see if the referenced complex type exists (it may not if they are pasting across models)
                // just search on local name since two models will have different namespaces probably
                ComplexType type = null;
                var cem = EntityType.EntityModel as ConceptualEntityModel;
                foreach (var c in cem.ComplexTypes())
                {
                    if (string.Compare(typeLocalName, c.LocalName.Value, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        type = c;
                        break;
                    }
                }

                // set the complex type reference
                if (type == null)
                {
                    // if we didn't find the complex type locally, write out the type name - but for the local namespace
                    // this will let the user subsequently copy the complex type and this property will start working again
                    var typeSymbol = new Symbol(cem.Namespace.Value, typeLocalName);
                    _createdProperty.ComplexType.SetXAttributeValue(typeSymbol.ToDisplayString());
                }
                else
                {
                    _createdProperty.ComplexType.SetRefName(type);
                }
            }
            else
            {
                _createdProperty.ComplexType.SetXAttributeValue(Resources.ComplexPropertyUndefinedType);
            }

            // runtime does not support nullable complex properties, need to set it to false since the default is true
            _createdProperty.Nullable.Value = BoolOrNone.FalseValue;
            EntityType.AddProperty(_createdProperty);

            XmlModelHelper.NormalizeAndResolve(_createdProperty);
        }

        /// <summary>
        ///     The Property created by this command
        /// </summary>
        internal ComplexConceptualProperty Property
        {
            get { return _createdProperty; }
        }

        /// <summary>
        ///     Creates a complex property in the passed in entity of the first ComplexType in the model ("Undefined" if there isn't one).
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create this property in</param>
        /// <returns>The new Complex Property</returns>
        internal static ComplexConceptualProperty CreateDefaultProperty(CommandProcessorContext cpc, string name, EntityType entityType)
        {
            var model = entityType.EntityModel as ConceptualEntityModel;
            ComplexType type = null;
            foreach (var complexType in model.ComplexTypes())
            {
                type = complexType;
                break;
            }
            var cpcd = new CreateComplexPropertyCommand(name, entityType, type);

            var cp = new CommandProcessor(cpc, cpcd);
            cp.Invoke();

            return cpcd.Property;
        }

        /// <summary>
        ///     The method will do the following:
        ///     - Creates a complex property with specified typeName in the entity-type.
        ///     - The complex property will be inserted in the specified position.
        ///     - Set the property's facet values.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create this property in</param>
        /// <param name="typeName">The complex property name.</param>
        /// <param name="concurrencyMode">The property concurrencyMode facet value.</param>
        /// <param name="getterAccessModifier">The property getterAccessModifier facet value.</param>
        /// <param name="setterAccessModifier">The property setterAccessModifier facet value.</param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        /// <returns></returns>
        internal static ComplexConceptualProperty CreateComplexProperty(
            CommandProcessorContext cpc, string name, EntityType entityType, string typeName,
            string concurrencyMode, string getterAccessModifier, string setterAccessModifier, InsertPropertyPosition insertPosition)
        {
            var cmd = new CreateComplexPropertyCommand(name, entityType, typeName, insertPosition);
            cmd.PostInvokeEvent += (o, eventsArgs) =>
                {
                    var complexProperty = cmd.Property;
                    Debug.Assert(complexProperty != null, "We didn't get good property out of the command");
                    if (complexProperty != null)
                    {
                        // set ComplexProperty attributes
                        if (!String.IsNullOrEmpty(concurrencyMode))
                        {
                            complexProperty.ConcurrencyMode.Value = concurrencyMode;
                        }
                        if (!String.IsNullOrEmpty(getterAccessModifier))
                        {
                            complexProperty.Getter.Value = getterAccessModifier;
                        }
                        if (!String.IsNullOrEmpty(setterAccessModifier))
                        {
                            complexProperty.Setter.Value = setterAccessModifier;
                        }
                    }
                };
            var cp = new CommandProcessor(cpc, cmd);
            cp.Invoke();
            return cmd.Property;
        }
    }
}
