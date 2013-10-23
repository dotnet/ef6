// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeletePropertyCommand : DeleteEFElementCommand
    {
        internal string DeletedPropertyOwningEntityName { get; private set; }
        internal string DeletedPropertyName { get; private set; }

        /// <summary>
        ///     Property used to specify that a delete should only occur in the Conceptual layer. This can be used in cases such as where TPT inheritance removes
        ///     a conceptual key property on dervied types but wants to keep the store key property, or when a refactor a property into a complex type.
        /// </summary>
        internal bool IsConceptualOnlyDelete { get; set; }

        internal DeletePropertyCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Deletes the passed in Property
        /// </summary>
        /// <param name="property"></param>
        internal DeletePropertyCommand(Property property)
            : base(property)
        {
            CommandValidation.ValidateProperty(property);
            SaveDeletedInformation();
        }

        public Property Property
        {
            get
            {
                var elem = EFElement as Property;
                Debug.Assert(elem != null, "underlying element does not exist or is not a Property");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
            set { EFElement = value; }
        }

        private void SaveDeletedInformation()
        {
            DeletedPropertyOwningEntityName = (Property.EntityType == null ? null : Property.EntityType.Name.Value);
            DeletedPropertyName = Property.Name.Value;
        }

        /// <summary>
        ///     We override this method because we need to do some extra things before
        ///     the normal PreInvoke gets called and our antiDeps are removed
        /// </summary>
        /// <param name="cpc"></param>
        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // Save off the deleted entity type name and property name. We do this
            // here as well just in case this is a late-bound command.
            SaveDeletedInformation();

            if (Property.IsEntityTypeProperty
                && Property.IsKeyProperty)
            {
                // Remove PropertyRef in Key for this property
                // Only invoke that for Entity property
                var setKey = new SetKeyPropertyCommand(Property, false, true);
                CommandProcessor.InvokeSingleCommand(cpc, setKey);
            }
            base.PreInvoke(cpc);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            base.InvokeInternal(cpc);
        }

        protected override void RemoveAntiDeps(CommandProcessorContext cpc)
        {
            foreach (var antiDep in Property.GetAntiDependenciesOfType<EFElement>())
            {
                // if this is a property in a dependent end of a ref constraint, we don't delete it
                var pref = antiDep as PropertyRef;
                if (pref != null)
                {
                    var role = pref.Parent as ReferentialConstraintRole;
                    if (role != null)
                    {
                        var rc = role.Parent as ReferentialConstraint;
                        if (rc != null
                            && rc.Dependent == role)
                        {
                            // property ref on a dependent end, don't delete it
                            continue;
                        }
                    }
                }
                DeleteInTransaction(cpc, antiDep);
            }
        }
    }
}
