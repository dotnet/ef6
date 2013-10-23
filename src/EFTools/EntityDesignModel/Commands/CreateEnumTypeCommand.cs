// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Create an enum type in Conceptual Model
    /// </summary>
    internal class CreateEnumTypeCommand : Command
    {
        public static readonly string PrereqId = "CreateEnumTypeCommand";

        private EnumType _createdEnumType;
        public bool UniquifyName { get; set; }
        public string Name { get; private set; }
        public string UnderlyingType { get; private set; }
        public bool IsFlag { get; private set; }
        public string ExternalTypeName { get; private set; }

        public CreateEnumTypeCommand(string name, string underlyingType)
            : this(name, underlyingType, String.Empty, false, true)
        {
        }

        public CreateEnumTypeCommand(string name, string underlyingType, string externalTypeName, bool isFlag, bool uniquifyName)
            : base(PrereqId)
        {
            ValidateString(name);
            Name = name;
            UnderlyingType = underlyingType;
            ExternalTypeName = externalTypeName;
            IsFlag = isFlag;
            UniquifyName = uniquifyName;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the entity to
            var model = artifact.ConceptualModel();
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            // check for uniqueness
            if (UniquifyName)
            {
                Name = ModelHelper.GetUniqueName(typeof(EnumType), model, Name);
            }
            else
            {
                string msg = null;
                if (ModelHelper.IsUniqueName(typeof(EnumType), model, Name, true, out msg) == false)
                {
                    throw new CommandValidationFailedException(msg);
                }
            }

            if (String.IsNullOrWhiteSpace(UnderlyingType) == false
                && ModelHelper.UnderlyingEnumTypes.Count(t => String.CompareOrdinal(t.Name, UnderlyingType) == 0) == 0)
            {
                throw new CommandValidationFailedException(
                    String.Format(CultureInfo.CurrentCulture, Resources.Incorrect_Enum_UnderlyingType, UnderlyingType));
            }

            // create the new item in our model
            var enumType = new EnumType(model, null);
            Debug.Assert(enumType != null, "enumType should not be null");
            if (enumType == null)
            {
                throw new ItemCreationFailureException();
            }

            // set the name, add it to the parent item
            enumType.LocalName.Value = Name;

            if (String.IsNullOrWhiteSpace(UnderlyingType) == false
                && String.CompareOrdinal(enumType.UnderlyingType.DefaultValue, UnderlyingType) != 0)
            {
                enumType.UnderlyingType.Value = UnderlyingType;
            }
            if (enumType.IsFlags.DefaultValue != IsFlag)
            {
                enumType.IsFlags.Value = IsFlag;
            }
            if (String.IsNullOrWhiteSpace(ExternalTypeName) == false)
            {
                enumType.ExternalTypeName.Value = ExternalTypeName;
            }

            model.AddEnumType(enumType);

            XmlModelHelper.NormalizeAndResolve(enumType);
            _createdEnumType = enumType;
        }

        /// <summary>
        ///     The EnumType that this command created
        /// </summary>
        public EnumType EnumType
        {
            get { return _createdEnumType; }
        }

        /// <summary>
        ///     This helper function will create an Enum type using default name.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <returns>The new EnumType</returns>
        public static EnumType CreateEnumTypeWithDefaultName(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the complex type to
            var model = artifact.ConceptualModel();
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            var enumTypeName = ModelHelper.GetUniqueNameWithNumber(typeof(EnumType), model, Resources.Model_DefaultEnumTypeName);

            // go create it
            var cp = new CommandProcessor(cpc);
            var cmd = new CreateEnumTypeCommand(enumTypeName, null);
            cp.EnqueueCommand(cmd);

            cp.Invoke();
            return cmd.EnumType;
        }
    }
}
