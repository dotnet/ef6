// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    /// <summary>
    ///     Use this command to set the StoreGeneratedPattern property on a new property.
    /// </summary>
    internal class SetStoreGeneratedPatternCommand : Command
    {
        public Property Property { get; set; }
        internal string SgpValue { get; set; }

        public SetStoreGeneratedPatternCommand()
        {
        }

        internal SetStoreGeneratedPatternCommand(Property property, string value)
        {
            CommandValidation.ValidateProperty(property);

            Property = property;
            SgpValue = value;
        }

        /// <summary>
        ///     Used to set the StoreGeneratedPattern property on a new property.
        /// </summary>
        /// <param name="prereq">Must be a non-null command creating the property</param>
        /// <param name="isKey">Flag whether to make the property a key or not</param>
        internal SetStoreGeneratedPatternCommand(CreatePropertyCommand prereq, string value)
        {
            ValidatePrereqCommand(prereq);

            SgpValue = value;

            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (Property == null)
            {
                var prereq = GetPreReqCommand(CreatePropertyCommand.PrereqId) as CreatePropertyCommand;
                if (prereq != null)
                {
                    // must be ConceptualProperty to have StoreGeneratedPattern
                    Property = prereq.CreatedProperty as ConceptualProperty;
                    if (Property != null)
                    {
                        CommandValidation.ValidateEntityProperty(Property);
                    }
                }

                Debug.Assert(Property != null, "We didn't get a good ConceptualProperty out of the Command.");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(Property != null, "InvokeInternal is called when Property is null.");
            if (Property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Property is null.");
            }

            var cmd = new UpdateDefaultableValueCommand<string>(Property.StoreGeneratedPattern, SgpValue);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);

            // ensure view keys are propagated from C-side to S-side
            var cet = Property.EntityType as ConceptualEntityType;
            if (cet != null)
            {
                PropagateViewKeysToStorageModel.AddRule(cpc, cet);
            }

            // ensure StoreGeneratedPattern is propagated from C-side to S-side
            // unless we are part of an Update Model txn in which case there is no need
            // as the whole artifact has this integrity check applied by UpdateModelFromDatabaseCommand
            if (EfiTransactionOriginator.UpdateModelFromDatabaseId != cpc.OriginatorId)
            {
                var cProp = Property as ConceptualProperty;
                Debug.Assert(cProp != null, "expected Property of type ConceptualProperty, instead got type " + Property.GetType().FullName);
                if (cProp != null)
                {
                    PropagateStoreGeneratedPatternToStorageModel.AddRule(cpc, cProp, true);
                }
            }
        }
    }
}
