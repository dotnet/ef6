// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping.ChildCollectionBuilders
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This class encapsulates the logic needed to build up the full list of potential children for an
    ///     association mapping.  The builder will determine what children this AssociationSetEnd could have and then
    ///     call either the BuildNew() or BuildExisting() methods.  To use this class, you need to derive from it
    ///     and override those two methods.
    /// </summary>
    internal class AssociationSetEndMappingBuilder
    {
        private readonly AssociationSetEnd _setEnd;
        private readonly StorageEntityType _storageEntityType;

        internal AssociationSetEndMappingBuilder(AssociationSetEnd setEnd, StorageEntityType storageEntityType)
        {
            Debug.Assert(setEnd != null, "setEnd should not be null");
            Debug.Assert(storageEntityType != null, "storageEntityType should not be null");

            _setEnd = setEnd;
            _storageEntityType = storageEntityType;
        }

        internal Association Association
        {
            get
            {
                if (AssociationSetEnd != null)
                {
                    return AssociationEnd.Parent as Association;
                }
                return null;
            }
        }

        internal AssociationEnd AssociationEnd
        {
            get { return _setEnd.Role.Target; }
        }

        internal AssociationSet AssociationSet
        {
            get { return _setEnd.Parent as AssociationSet; }
        }

        internal AssociationSetEnd AssociationSetEnd
        {
            get { return _setEnd; }
        }

        internal ConceptualEntityType ConceptualEntityType
        {
            get
            {
                if (AssociationEnd != null)
                {
                    var et = AssociationEnd.Type.Target;
                    var cet = et as ConceptualEntityType;
                    Debug.Assert(et == null || cet != null, "EntityType is not a ConceptualEntityType");
                    return cet;
                }
                return null;
            }
        }

        internal StorageEntityType StorageEntityType
        {
            get { return _storageEntityType; }
        }

        internal void Build(CommandProcessorContext cpc)
        {
            var associationSet = AssociationSetEnd.Parent as AssociationSet;

            EndProperty endProperty = null;
            foreach (var val in AssociationSetEnd.EndProperties)
            {
                if (val.AssociationSetMapping == associationSet.AssociationSetMapping)
                {
                    endProperty = val;
                    break;
                }
            }

            // put all of entity keys in a list
            var keys = new List<Property>();
            foreach (var property in ConceptualEntityType.ResolvableTopMostBaseType.Properties())
            {
                if (property.IsKeyProperty)
                {
                    keys.Add(property);
                }
            }

            // loop through all of the keys, adding view model elements for each
            foreach (var key in keys)
            {
                ScalarProperty existingScalarProperty = null;

                // for each column, see if we are already have a scalar property
                var antiDeps = key.GetAntiDependenciesOfType<ScalarProperty>();
                foreach (var scalarProperty in antiDeps)
                {
                    // if we find one, validate it
                    if (scalarProperty != null
                        && scalarProperty.Parent is EndProperty
                        && scalarProperty.Name.Status == BindingStatus.Known
                        && scalarProperty.Name.Target is Property
                        && scalarProperty.Name.Target.Parent is EntityType)
                    {
                        // see if its already mapped by this type
                        if (endProperty == scalarProperty.Parent)
                        {
                            // we are already mapping this 
                            existingScalarProperty = scalarProperty;
                            break;
                        }
                    }
                }

                if (existingScalarProperty == null)
                {
                    // we didn't find one
                    BuildNew(cpc, key.LocalName.Value, key.DisplayName);
                }
                else if (existingScalarProperty != null
                         && existingScalarProperty.Parent is EndProperty)
                {
                    // we found an existing mapping
                    BuildExisting(cpc, existingScalarProperty);
                }
            }
        }

        protected virtual void BuildNew(CommandProcessorContext cpc, string propertyName, string propertyType)
        {
        }

        protected virtual void BuildExisting(CommandProcessorContext cpc, ScalarProperty scalarProperty)
        {
        }
    }
}
