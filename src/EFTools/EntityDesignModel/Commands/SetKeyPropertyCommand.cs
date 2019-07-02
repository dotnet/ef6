// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    /// <summary>
    ///     Use this command to change whether a property is or isn't a key in the containing entity.
    /// </summary>
    internal class SetKeyPropertyCommand : Command
    {
        public Property Property { get; set; }
        internal bool IsKey { get; set; }
        internal bool IsRemovingDerivedKeyForInheritance { get; private set; }
        internal bool IsRemovingKeyForDelete { get; private set; }
        private readonly bool _deletePrincipalRCRefs = true;

        internal SetKeyPropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Based on the isKey flag, makes the property part of the key or removes it from the key
        /// </summary>
        /// <param name="property">A valid property</param>
        /// <param name="isKey">Flag whether to make the property a key or not</param>
        internal SetKeyPropertyCommand(Property property, bool isKey, bool isRemovingKeyForDelete = false)
            : this(property, isKey, true, false, isRemovingKeyForDelete)
        {
        }

        internal SetKeyPropertyCommand(
            Property property, bool isKey, bool deletePrincipalRCRefs, bool isRemovingDerivedKeyForInheritance = false,
            bool isRemovingKeyForDelete = false)
        {
            CommandValidation.ValidateEntityProperty(property);
            Property = property;
            IsKey = isKey;
            _deletePrincipalRCRefs = deletePrincipalRCRefs;
            IsRemovingDerivedKeyForInheritance = isRemovingDerivedKeyForInheritance;
            IsRemovingKeyForDelete = isRemovingKeyForDelete;
        }

        /// <summary>
        ///     Used to set a new property as the key.
        /// </summary>
        /// <param name="prereq">Must be a non-null command creating the property</param>
        /// <param name="isKey">Flag whether to make the property a key or not</param>
        internal SetKeyPropertyCommand(CreatePropertyCommand prereq, bool isKey)
        {
            ValidatePrereqCommand(prereq);

            IsKey = isKey;
            _deletePrincipalRCRefs = true;

            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (Property == null)
            {
                var prereq = GetPreReqCommand(CreatePropertyCommand.PrereqId) as CreatePropertyCommand;
                if (prereq != null)
                {
                    Property = prereq.CreatedProperty;
                    CommandValidation.ValidateEntityProperty(Property);
                }

                Debug.Assert(Property != null, "We didn't get a good property out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(Property != null, "InvokeInternal is called when property is null.");
            if (Property == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Property is null.");
            }

            if (Property.IsKeyProperty == IsKey)
            {
                // no change needed
                return;
            }

            if (IsKey)
            {
                if (Property.EntityType.Key == null)
                {
                    Property.EntityType.Key = new Key(Property.EntityType, null);
                }

                // make the key property non-nullable
                Property.Nullable.Value = BoolOrNone.FalseValue;
                Property.EntityType.Key.AddPropertyRef(Property);

                // normalize & resolve the key property
                XmlModelHelper.NormalizeAndResolve(Property.EntityType.Key);
            }
            else
            {
                var keyElement = Property.EntityType.Key;
                Debug.Assert(keyElement != null, "keyElement should not be null");

                if (keyElement != null)
                {
                    keyElement.RemovePropertyRef(Property);

                    if (!keyElement.Children.Any())
                    {
                        keyElement.Delete();
                    }
                    else
                    {
                        XmlModelHelper.NormalizeAndResolve(keyElement);
                    }
                }

                // if we are changing from key to non-key and this key is referenced by a principal end of 
                // a ref constraint, then we want to delete that entry in the ref constraint
                if (IsKey == false && _deletePrincipalRCRefs)
                {
                    foreach (var pref in Property.GetAntiDependenciesOfType<PropertyRef>())
                    {
                        if (pref != null)
                        {
                            var role = pref.Parent as ReferentialConstraintRole;
                            if (role != null)
                            {
                                var rc = role.Parent as ReferentialConstraint;
                                if (rc != null
                                    && rc.Principal == role)
                                {
                                    // property ref on a principal end, so delete it
                                    DeleteEFElementCommand.DeleteInTransaction(cpc, pref);
                                }
                            }
                        }
                    }
                }
            }

            var cet = Property.EntityType as ConceptualEntityType;
            if (cet != null)
            {
                PropagateViewKeysToStorageModel.AddRule(cpc, cet);
            }
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            // in the conceptual model, changing key states will impact MSL generated any
            // inferred ref constraints
            if (Property.EntityModel.IsCSDL)
            {
                EnforceEntitySetMappingRules.AddRule(cpc, Property.EntityType);
                InferReferentialConstraints.AddRule(cpc, Property.EntityType);

                // Add the integrity check to propagate the StoreGeneratedPattern value to the
                // S-side (may be altered by property being/not being a key) unless we are part
                // of an Update Model txn in which case there is no need as the whole artifact has
                // this integrity check applied by UpdateModelFromDatabaseCommand
                if (EfiTransactionOriginator.UpdateModelFromDatabaseId != cpc.OriginatorId)
                {
                    var cProp = Property as ConceptualProperty;
                    Debug.Assert(
                        cProp != null, "expected _property of type ConceptualProperty, instead got type " + Property.GetType().FullName);
                    if (cProp != null)
                    {
                        PropagateStoreGeneratedPatternToStorageModel.AddRule(cpc, cProp, true);
                    }
                }
            }

            base.PostInvoke(cpc);
        }
    }
}
