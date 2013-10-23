// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command creates a new ComplexType Property and lets you define the name, type and nullability of
    ///     the property.
    /// </summary>
    internal class CreateComplexTypePropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateComplexTypePropertyCommand";

        internal string Name { get; set; }
        internal ComplexType ParentComplexType { get; set; }
        internal ComplexType ComplexType { get; set; }
        internal bool? Nullable { get; set; }
        internal string Type { get; set; }
        private Property _createdProperty;

        /// <summary>
        ///     Creates a property in the passed in ComplexType.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="parentComplexType">The ComplexType to create the property in</param>
        /// <param name="type">The type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreateComplexTypePropertyCommand(string name, ComplexType parentComplexType, string type, bool? nullable)
            : base(PrereqId)
        {
            ValidateString(name);
            CommandValidation.ValidateComplexType(parentComplexType);
            ValidateString(type);

            Name = name;
            ParentComplexType = parentComplexType;
            Type = type;
            Nullable = nullable;
        }

        internal CreateComplexTypePropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates a complex property in the passed in ComplexType.
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="parentComplexType">The ComplexType to create the property in</param>
        /// <param name="complexType">The complex type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreateComplexTypePropertyCommand(string name, ComplexType parentComplexType, ComplexType complexType, bool? nullable)
            : base(PrereqId)
        {
            ValidateString(name);
            CommandValidation.ValidateComplexType(parentComplexType);
            CommandValidation.ValidateComplexType(complexType);

            Name = name;
            ParentComplexType = parentComplexType;
            ComplexType = complexType;
            Nullable = nullable;
        }

        /// <summary>
        ///     Creates a complex property in the complex type being created by the passed in command
        /// </summary>
        /// <param name="name">The name of the new property</param>
        /// <param name="prereqCommand">Instance of CreateComplexTypeCommand; must be non null</param>
        /// <param name="type">The type of the property</param>
        /// <param name="nullable">Flag whether the property is nullable or not</param>
        internal CreateComplexTypePropertyCommand(string name, CreateComplexTypeCommand prereqCommand, string type, bool? nullable)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ComplexType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ParentComplexType")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(ParentComplexType != null, "InvokeInternal is called when ParentComplexType is null.");
            if (ParentComplexType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when ParentComplexType is null");
            }

            Debug.Assert(!(Type == null && ComplexType == null), "InvokeInternal is called when Type or ComplexType is null.");
            if (Type == null
                && ComplexType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Type and ComplexType is null");
            }

            // check for uniqueness
            string msg;
            if (!ModelHelper.ValidateComplexTypePropertyName(ParentComplexType, Name, true, out msg))
            {
                throw new CommandValidationFailedException(msg);
            }

            // check for ComplexType circular definition
            if (ComplexType != null)
            {
                if (ModelHelper.ContainsCircularComplexTypeDefinition(ParentComplexType, ComplexType))
                {
                    throw new CommandValidationFailedException(
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.Error_CircularComplexTypeDefinitionOnAdd, ComplexType.LocalName.Value));
                }
            }
            // create the property
            Property property = null;
            if (Type != null)
            {
                var conceptualProperty = new ConceptualProperty(ParentComplexType, null);
                conceptualProperty.ChangePropertyType(Type);
                property = conceptualProperty;
            }
            else
            {
                var complexProperty = new ComplexConceptualProperty(ParentComplexType, null);
                complexProperty.ComplexType.SetRefName(ComplexType);
                property = complexProperty;
            }
            Debug.Assert(property != null, "property should not be null");
            if (property == null)
            {
                throw new ItemCreationFailureException();
            }

            // set the name and add to the parent entity
            property.LocalName.Value = Name;
            ParentComplexType.AddProperty(property);

            // set other attributes of the property
            if (Nullable != null)
            {
                property.Nullable.Value = (Nullable.Value ? BoolOrNone.TrueValue : BoolOrNone.FalseValue);
            }

            XmlModelHelper.NormalizeAndResolve(property);
            _createdProperty = property;
        }

        /// <summary>
        ///     The Property created by this command
        /// </summary>
        internal Property Property
        {
            get { return _createdProperty; }
        }

        /// <summary>
        ///     Get ComplexType value from CreateComplexTypeCommand
        /// </summary>
        protected override void ProcessPreReqCommands()
        {
            if (ParentComplexType == null)
            {
                var prereq = GetPreReqCommand(CreateComplexTypeCommand.PrereqId) as CreateComplexTypeCommand;
                if (prereq != null)
                {
                    ParentComplexType = prereq.ComplexType;
                    CommandValidation.ValidateComplexType(ParentComplexType);
                }

                Debug.Assert(
                    ParentComplexType != null, typeof(CreateComplexTypePropertyCommand).Name + " command return null value of ComplexType");
            }
        }

        /// <summary>
        ///     Creates scalar property with a default, unique name and passed type
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="parentComplexType">parent for new property</param>
        /// <param name="type">type for new property</param>
        /// <returns></returns>
        internal static Property CreateDefaultProperty(CommandProcessorContext cpc, ComplexType parentComplexType, string type)
        {
            var name = ModelHelper.GetUniqueName(typeof(ConceptualProperty), parentComplexType, Property.DefaultPropertyName);
            var cmd = new CreateComplexTypePropertyCommand(name, parentComplexType, type, false);

            var cp = new CommandProcessor(cpc, cmd);
            cp.Invoke();

            return cmd.Property;
        }

        /// <summary>
        ///     Creates complex property with a default,unique name and passed complex type
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="parentComplexType">parent for new property</param>
        /// <param name="type">type for new property</param>
        /// <returns></returns>
        internal static Property CreateDefaultProperty(CommandProcessorContext cpc, ComplexType parentComplexType, ComplexType type)
        {
            var name = ModelHelper.GetUniqueName(
                typeof(ConceptualProperty), parentComplexType, ComplexConceptualProperty.DefaultComplexPropertyName);
            var cmd = new CreateComplexTypePropertyCommand(name, parentComplexType, type, false);

            var cp = new CommandProcessor(cpc, cmd);
            cp.Invoke();

            return cmd.Property;
        }
    }
}
