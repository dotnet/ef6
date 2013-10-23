// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command creates a new property and lets you define the name, type and nullability of
    ///     the property.  Other related commands are:
    ///     - SetKeyPropertyCommand
    ///     - SetConceptualPropertyFacetsCommand
    ///     - SetStoragePropertyFacetsCommand
    /// </summary>
    internal class CreatePropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreatePropertyCommand";
        private bool _createWithDefaultName;
        private readonly InsertPropertyPosition _insertPosition;

        internal bool CreateWithDefaultName
        {
            get { return _createWithDefaultName; }
            set { _createWithDefaultName = value; }
        }

        internal string Name { get; set; }
        internal string Type { get; set; }
        internal bool? Nullable { get; set; }
        internal EntityType EntityType { get; set; }
        internal bool IsIdProperty { get; set; }
        public Property CreatedProperty { get; protected set; }

        internal CreatePropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates a property in the passed in entity.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="type">The type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreatePropertyCommand(string name, EntityType entityType, string type, bool? nullable)
            : this(name, entityType, type, nullable, null)
        {
        }

        /// <summary>
        ///     Creates a property in the passed in entity.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create the property in</param>
        /// <param name="type">The type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreatePropertyCommand(
            string name, EntityType entityType, string type, bool? nullable, InsertPropertyPosition insertPosition)
            : base(PrereqId)
        {
            ValidateString(name);
            CommandValidation.ValidateEntityType(entityType);
            ValidateString(type);

            Name = name;
            EntityType = entityType;
            Type = type;
            Nullable = nullable;
            _insertPosition = insertPosition;
        }

        /// <summary>
        ///     Creates a property in the entity being created by the passed in command
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="prereqCommand">Must be non-null</param>
        /// <param name="type">The type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreatePropertyCommand(string name, CreateEntityTypeCommand prereqCommand, string type, bool? nullable)
            : base(PrereqId)
        {
            ValidatePrereqCommand(prereqCommand);
            ValidateString(name);
            ValidateString(type);

            Name = name;
            Type = type;
            Nullable = nullable;

            AddPreReqCommand(prereqCommand);
        }

        protected override void ProcessPreReqCommands()
        {
            if (EntityType == null)
            {
                var prereq = GetPreReqCommand(CreateEntityTypeCommand.PrereqId) as CreateEntityTypeCommand;
                if (prereq != null)
                {
                    EntityType = prereq.EntityType;
                    CommandValidation.ValidateEntityType(EntityType);
                }

                Debug.Assert(EntityType != null, "We didn't get a good entity type out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(EntityType != null, "InvokeInternal is called when EntityType is null");
            if (EntityType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when EntityType is null");
            }

            // check for uniqueness
            if (EntityType.LocalName.Value.Equals(Name, StringComparison.Ordinal))
            {
                throw new CommandValidationFailedException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_MemberNameSameAsParent, Name, EntityType.LocalName.Value));
            }

            if (!ModelHelper.IsUniquePropertyName(EntityType, Name, true))
            {
                throw new CommandValidationFailedException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_MemberNameNotUnique, Name, EntityType.LocalName.Value));
            }

            CreatedProperty = CreateProperty();
        }

        // internal for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal Property CreateProperty()
        {
            var isNullable =
                Nullable != null
                    ? Nullable.Value ? BoolOrNone.TrueValue : BoolOrNone.FalseValue
                    : null;

            var property = CreateProperty(EntityType, Name, Type, isNullable, _insertPosition);
            EntityType.AddProperty(property);

            XmlModelHelper.NormalizeAndResolve(property);

            return property;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Property CreateProperty(
            EntityType parentEntity, string name, string type, BoolOrNone isNullable, InsertPropertyPosition insertPosition)
        {
            var conceptualEntity = parentEntity as ConceptualEntityType;
            Debug.Assert(conceptualEntity != null || parentEntity is StorageEntityType, "unexpected entity type");
            var property = conceptualEntity != null
                               ? CreateConceptualProperty(conceptualEntity, name, type, insertPosition)
                               : CreateStorageProperty((StorageEntityType)parentEntity, name, type);

            if (isNullable != null)
            {
                property.Nullable.Value = isNullable;
            }

            return property;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Property CreateConceptualProperty(ConceptualEntityType parentEntity, string name, string type, InsertPropertyPosition insertPosition)
        {
            var property = new ConceptualProperty(parentEntity, null, insertPosition);
            property.LocalName.Value = name;
            property.ChangePropertyType(type);
            return property;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Property CreateStorageProperty(StorageEntityType parentEntity, string name, string type)
        {
            var property = new StorageProperty(parentEntity, null);
            property.LocalName.Value = name;
            property.Type.Value = type;
            return property;
        }

        /// <summary>
        ///     Creates a property in the passed in entity of the default Type (non-nullable String).
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes commited.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new property</param>
        /// <param name="entityType">The entity to create this property in</param>
        /// <returns>The new Property</returns>
        internal static Property CreateDefaultProperty(CommandProcessorContext cpc, string name, EntityType entityType)
        {
            var cpcd = new CreatePropertyCommand(
                name, entityType, ModelConstants.DefaultPropertyType, ModelConstants.DefaultPropertyNullability);
            cpcd._createWithDefaultName = true;

            var cp = new CommandProcessor(cpc, cpcd);
            cp.Invoke();

            return cpcd.CreatedProperty;
        }

        /// <summary>
        ///     Creates a new property in the passed in conceptual entity and optionally sets additional
        ///     facets on the property.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes commited.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the property</param>
        /// <param name="entityType">Must be a conceptual entity</param>
        /// <param name="type">The type to use for this property (cannot be empty)</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        /// <param name="theDefault">Optional: the default value for this property</param>
        /// <param name="concurrencyMode">Optional: the concurrency mode for this property</param>
        /// <param name="getterAccessModifier">Optional: Get access modifier.</param>
        /// <param name="setterAccessModifier">Optional: Set access modifier.</param>
        /// <returns>The new Property</returns>
        internal static Property CreateConceptualProperty(
            CommandProcessorContext cpc, string name, ConceptualEntityType entityType,
            string type, bool? nullable, StringOrNone theDefault, string concurrencyMode, string getterAccessModifier,
            string setterAccessModifier,
            StringOrPrimitive<UInt32> maxLength, bool? fixedLength, StringOrPrimitive<UInt32> precision, StringOrPrimitive<UInt32> scale,
            bool? unicode, StringOrNone collation, string storeGeneratedPattern, InsertPropertyPosition insertPosition)
        {
            CommandValidation.ValidateConceptualEntityType(entityType);

            var cpcd = new CreatePropertyCommand(name, entityType, type, nullable, insertPosition);
            var scp = new SetConceptualPropertyFacetsCommand(
                cpcd, theDefault, concurrencyMode, getterAccessModifier, setterAccessModifier,
                maxLength, DefaultableValueBoolOrNone.GetFromNullableBool(fixedLength), precision, scale,
                DefaultableValueBoolOrNone.GetFromNullableBool(unicode), collation);
            var scpac = new SetConceptualPropertyAnnotationsCommand(cpcd, storeGeneratedPattern);

            var cp = new CommandProcessor(cpc, cpcd, scp, scpac);
            cp.Invoke();

            return cpcd.CreatedProperty;
        }

        /// <summary>
        ///     Creates a new property in the passed in storage entity and optionally sets additional
        ///     facets on the property.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes commited.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the property</param>
        /// <param name="entityType">Must be a storage entity</param>
        /// <param name="type">The type to use for this property (cannot be empty)</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        /// <param name="theDefault">Optional: the default value for this property</param>
        /// <param name="maxLength">Optional facet</param>
        /// <param name="fixedLength">Optional facet</param>
        /// <param name="precision">Optional facet</param>
        /// <param name="scale">Optional facet</param>
        /// <param name="unicode">Optional facet</param>
        /// <param name="collation">Optional facet</param>
        /// <param name="concurrencyMode">Optional: the concurrency mode for this property</param>
        /// <returns>The new Property</returns>
        internal static Property CreateStorageProperty(
            CommandProcessorContext cpc, string name, StorageEntityType entityType,
            string type, bool? nullable, StringOrNone theDefault, StringOrPrimitive<UInt32> maxLength, BoolOrNone fixedLength,
            StringOrPrimitive<UInt32> precision,
            StringOrPrimitive<UInt32> scale, BoolOrNone unicode, StringOrNone collation, string concurrencyMode)
        {
            CommandValidation.ValidateStorageEntityType(entityType);

            var cpcd = new CreatePropertyCommand(name, entityType, type, nullable);
            var ssp = new SetPropertyFacetsCommand(
                cpcd, theDefault, maxLength, fixedLength, precision, scale, unicode, collation, concurrencyMode);

            var cp = new CommandProcessor(cpc, cpcd, ssp);
            cp.Invoke();

            return cpcd.CreatedProperty;
        }

        internal void SetCreatedProperty(Property createdProperty)
        {
            CreatedProperty = createdProperty;
        }
    }
}
