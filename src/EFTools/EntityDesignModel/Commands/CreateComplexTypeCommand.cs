// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateComplexTypeCommand : Command
    {
        internal static readonly string PrereqId = "CreateComplexTypeCommand";
        internal static readonly string ProposedNameProperty = "ProposedName";

        internal string Name { get; set; }
        internal bool UniquifyName { get; set; }
        private ComplexType _createdComplexType;

        /// <summary>
        ///     Creates a new entity type in either the conceptual model or the storage model.
        /// </summary>
        /// <param name="name">The name to use for this type</param>
        /// <param name="modelSpace">Either Conceptual or Storage</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateComplexTypeCommand(string name, bool uniquifyName)
            : base(PrereqId)
        {
            ValidateString(name);

            Name = name;
            UniquifyName = uniquifyName;

            WriteProperty(ProposedNameProperty, name);
        }

        internal CreateComplexTypeCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
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
                Name = ModelHelper.GetUniqueName(typeof(ComplexType), model, Name);
            }
            else
            {
                string msg = null;
                if (ModelHelper.IsUniqueName(typeof(ComplexType), model, Name, false, out msg) == false)
                {
                    throw new CommandValidationFailedException(msg);
                }
            }

            // create the new item in our model
            var complexType = new ComplexType(model, null);
            Debug.Assert(complexType != null, "complexType should not be null");
            if (complexType == null)
            {
                throw new ItemCreationFailureException();
            }

            // set the name, add it to the parent item
            complexType.LocalName.Value = Name;
            model.AddComplexType(complexType);

            XmlModelHelper.NormalizeAndResolve(complexType);

            _createdComplexType = complexType;
        }

        /// <summary>
        ///     The ComplexType that this command created
        /// </summary>
        internal ComplexType ComplexType
        {
            get { return _createdComplexType; }
        }

        /// <summary>
        ///     This helper function will create a complex type using default name.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes commited.
        /// </summary>
        /// <param name="cpc"></param>
        /// <returns>The new ComplexType</returns>
        internal static ComplexType CreateComplexTypeWithDefaultName(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the complex type to
            var model = artifact.ConceptualModel();
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            var complexTypeName = ModelHelper.GetUniqueNameWithNumber(typeof(ComplexType), model, Resources.Model_DefaultComplexTypeName);

            // go create it
            var cp = new CommandProcessor(cpc);
            var cmd = new CreateComplexTypeCommand(complexTypeName, false);
            cp.EnqueueCommand(cmd);
            cp.Invoke();
            return cmd.ComplexType;
        }
    }
}
