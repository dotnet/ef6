// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     This class is here to represent the extra node we want into the UI, namely
    ///     a child of the table, and a sibling of the list of conditions.  It shares a reference
    ///     to the s-side entity with its parent.
    /// </summary>
    [TreeGridDesignerRootBranch(typeof(ColumnMappingsBranch))]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 1)]
    internal class MappingColumnMappings : MappingEntityMappingRoot
    {
        private IList<MappingScalarProperty> _scalarProperties;

        public MappingColumnMappings(EditingContext context, EntityType storageEntityType, MappingEFElement parent)
            : base(context, storageEntityType, parent)
        {
        }

        internal EntityType StorageEntityType
        {
            get { return ModelItem as EntityType; }
        }

        internal override string Name
        {
            get { return Resources.MappingDetails_ColumnMappingsName; }
        }

        /// <summary>
        ///     We override this property because we don't want to use the base setter; otherwise
        ///     we'll replace the XRef for the storage entity so that it points here and not to the
        ///     MappingStorageEntityType (our parent).
        /// </summary>
        internal override EFElement ModelItem
        {
            get { return _modelItem; }
            set
            {
                _modelItem = value;
                _isDisposed = false;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal IList<MappingScalarProperty> ScalarProperties
        {
            get
            {
                _scalarProperties = new List<MappingScalarProperty>();

                if (StorageEntityType != null)
                {
                    var entityType = MappingConceptualEntityType.ConceptualEntityType;
                    var mappingStrategy = ModelHelper.DetermineCurrentInheritanceStrategy(entityType);

                    // prime the list with all columns in the table (do not remove any columns
                    // used in conditions - design decision to allow the user to see the error message
                    // and respond rather than forcing them to unmap in order to re-map)
                    var storageProperties = new List<Property>();
                    storageProperties.AddRange(StorageEntityType.Properties());

                    // loop through all of the 'columns' in the s-side entity
                    foreach (var storageProp in storageProperties)
                    {
                        var skipProperty = false;
                        ScalarProperty existingScalarProperty = null;

                        ScalarProperty scalarPropMappedToCurrentEntity;
                        ScalarProperty nearestScalarPropMappedToAncestorEntity;
                        GetInheritanceScalarPropsForStorageProp(
                            storageProp, entityType, out scalarPropMappedToCurrentEntity, out nearestScalarPropMappedToAncestorEntity);

                        switch (mappingStrategy)
                        {
                            case InheritanceMappingStrategy.TablePerHierarchy:
                                if (nearestScalarPropMappedToAncestorEntity != null)
                                {
                                    // there is a ScalarProperty for storageProp in the ancestor EntityType and there may or may not be
                                    // a mapping for the same storageProp in the current EntityType - for TPH we do not display columns
                                    // assigned to ancestors - so skip this property
                                    skipProperty = true;
                                    existingScalarProperty = null;
                                }
                                else if (scalarPropMappedToCurrentEntity == null)
                                {
                                    // there is no ScalarProperty for storageProp in either the current or any ancestor EntityType
                                    // so assign a dummy MSP
                                    skipProperty = false;
                                    existingScalarProperty = null;
                                }
                                else
                                {
                                    // there is a ScalarProperty for storageProp in the current EntityType and this does
                                    // not override that for any ancestor EntityType - so assign an MSP using scalarPropMappedToCurrentEntity 
                                    skipProperty = false;
                                    existingScalarProperty = scalarPropMappedToCurrentEntity;
                                }

                                break; // end case InheritanceMappingStrategy.TablePerHierarchy

                            case InheritanceMappingStrategy.TablePerType:
                                if (nearestScalarPropMappedToAncestorEntity != null)
                                {
                                    // there is a ScalarProperty for storageProp in the ancestor EntityType and there may or may not
                                    // be an overriding ScalarProperty for the same storageProp in the current EntityType - for TPT
                                    // we only display columns assigned to ancestors if this is a key column, otherwise skip this property
                                    EntityType topMostBaseType = entityType.ResolvableTopMostBaseType;
                                    var entityProperty = GetMappedProperty(nearestScalarPropMappedToAncestorEntity);
                                    if (false == topMostBaseType.ResolvableKeys.Contains(entityProperty))
                                    {
                                        skipProperty = true;
                                        existingScalarProperty = null;
                                    }
                                    else
                                    {
                                        // TPT but we have an ancestor mapped to the same table - this implies
                                        // S-side conditions which will imply a Default ETM (when EnforceEntitySetMappingRules
                                        // runs). Hence this ScalarProperty is different from nearestScalarPropMappedToAncestorEntity
                                        // but may or may not yet be in existence. Assign scalarPropMappedToCurrentEntity as the
                                        // ScalarProperty to be mapped - if it exists then will map using that, otherwise it will
                                        // assign null and hence cause a dummy MSP to be created to allow the user to map within
                                        // the correct ETM.
                                        skipProperty = false;
                                        existingScalarProperty = scalarPropMappedToCurrentEntity;
                                    }
                                }
                                else if (scalarPropMappedToCurrentEntity == null)
                                {
                                    // there is no ScalarProperty for storageProp in either the current or any ancestor EntityType
                                    // so assign a dummy MSP
                                    skipProperty = false;
                                    existingScalarProperty = null;
                                }
                                else
                                {
                                    // there is a ScalarProperty for storageProp in the current EntityType and this does
                                    // not override that for any ancestor EntityType - so assign an MSP using scalarPropMappedToCurrentEntity 
                                    skipProperty = false;
                                    existingScalarProperty = scalarPropMappedToCurrentEntity;
                                }

                                break; // end case InheritanceMappingStrategy.TablePerType

                            case InheritanceMappingStrategy.NoInheritance:
                                if (nearestScalarPropMappedToAncestorEntity != null)
                                {
                                    Debug.Fail(
                                        "for S-side property " + storageProp +
                                        " identified inheritance strategy as " + InheritanceMappingStrategy.NoInheritance
                                        + " but found ScalarProperty for ancestor " + nearestScalarPropMappedToAncestorEntity);
                                }

                                if (scalarPropMappedToCurrentEntity == null)
                                {
                                    // there is no ScalarProperty for storageProp in the current EntityType's mappings
                                    // so assign a dummy MSP
                                    skipProperty = false;
                                    existingScalarProperty = null;
                                }
                                else
                                {
                                    // there is a ScalarProperty for storageProp within the current EntityType's mappings
                                    // so assign an MSP using scalarPropMappedToCurrentEntity
                                    skipProperty = false;
                                    existingScalarProperty = scalarPropMappedToCurrentEntity;
                                }

                                break; // end case InheritanceMappingStrategy.NoInheritance

                            default:
                                Debug.Fail("for S-side property " + storageProp + ", unable to use inheritance strategy " + mappingStrategy);

                                break; // end case default
                        }

                        if (skipProperty)
                        {
                            continue;
                        }

                        // if we didn't find a ScalarProperty to map to, then create a dummy row
                        // with just the column info, otherwise create a normal MSP mapped to the
                        // existing ScalarProperty
                        if (existingScalarProperty == null)
                        {
                            var msp = new MappingScalarProperty(_context, null, this);
                            msp.ColumnName = storageProp.LocalName.Value;
                            msp.ColumnType = storageProp.TypeName;
                            msp.IsKeyColumn = storageProp.IsKeyProperty;
                            _scalarProperties.Add(msp);
                        }
                        else
                        {
                            var msp =
                                (MappingScalarProperty)ModelToMappingModelXRef.GetNewOrExisting(_context, existingScalarProperty, this);
                            _scalarProperties.Add(msp);
                        }
                    }
                }

                return _scalarProperties;
            }
        }

        private static void GetInheritanceScalarPropsForStorageProp(
            Property storageProp, ConceptualEntityType etmEntityType, out ScalarProperty scalarPropMappedToCurrentEntity,
            out ScalarProperty nearestScalarPropMappedToAncestorEntity)
        {
            scalarPropMappedToCurrentEntity = null;
            nearestScalarPropMappedToAncestorEntity = null;

            // assign the ScalarProp which maps storageProp within ETM where the EntityType is etmEntityType (can be null)
            scalarPropMappedToCurrentEntity = storageProp.GetAntiDependenciesOfType<ScalarProperty>().
                FirstOrDefault<ScalarProperty>(scalarProp => scalarProp.FirstBoundConceptualEntityType == etmEntityType);

            // assign the ScalarProp which maps storageProp within ETM for nearest ancestor of etmEntityType (can be null)
            foreach (var cet in etmEntityType.ResolvableBaseTypes)
            {
                nearestScalarPropMappedToAncestorEntity = storageProp.GetAntiDependenciesOfType<ScalarProperty>().
                    FirstOrDefault<ScalarProperty>(scalarProp => scalarProp.FirstBoundConceptualEntityType == cet);
                if (nearestScalarPropMappedToAncestorEntity != null)
                {
                    return;
                }
            }
        }

        private static Property GetMappedProperty(ScalarProperty scalarProperty)
        {
            Property entityProperty = null;
            var complexProperty = scalarProperty.GetTopMostComplexProperty();
            if (complexProperty == null)
            {
                entityProperty = scalarProperty.Name.Target;
            }
            else
            {
                entityProperty = complexProperty.Name.Target;
            }

            return entityProperty;
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var child in ScalarProperties)
            {
                _children.Add(child);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingScalarProperty;
            Debug.Assert(child != null, "Unknown child being deleted");
            if (child != null)
            {
                _children.Remove(child);
                return;
            }

            base.OnChildDeleted(melem);
        }
    }
}
