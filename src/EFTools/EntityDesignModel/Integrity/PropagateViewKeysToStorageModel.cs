// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This class should be registered in a transaction where mappings are changed.  This will
    ///     push targeted updates to view keys from the conceptual model to the storage model.
    /// </summary>
    internal class PropagateViewKeysToStorageModel : IIntegrityCheck
    {
        private readonly CommandProcessorContext _cpc;
        private readonly ConceptualEntityType _conceptualEntityType;
        private readonly IEnumerable<ConceptualEntityType> _conceptualEntityTypes;
        private readonly HashSet<EntityType> _views = new HashSet<EntityType>();

        internal PropagateViewKeysToStorageModel(CommandProcessorContext cpc, ConceptualEntityType entityType)
        {
            _cpc = cpc;
            _conceptualEntityType = entityType;
        }

        internal PropagateViewKeysToStorageModel(CommandProcessorContext cpc, IEnumerable<ConceptualEntityType> entityTypes)
        {
            _cpc = cpc;
            _conceptualEntityTypes = entityTypes;
        }

        public bool IsEqual(IIntegrityCheck otherCheck)
        {
            var typedOtherCheck = otherCheck as PropagateViewKeysToStorageModel;
            if (typedOtherCheck != null)
            {
                if (typedOtherCheck._conceptualEntityType == _conceptualEntityType)
                {
                    return true;
                }
                else if (typedOtherCheck._conceptualEntityTypes != null
                         && _conceptualEntityTypes != null
                         && new HashSet<EntityType>(_conceptualEntityTypes).SetEquals(typedOtherCheck._conceptualEntityTypes))
                {
                    return true;
                }
            }

            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Invoke()
        {
            try
            {
                if (_conceptualEntityType != null)
                {
                    PropagateToStorageEntity(_conceptualEntityType);
                }
                else if (_conceptualEntityTypes != null)
                {
                    PropagateToStorageEntities();
                }
                UnsetUnmappedKeyColumnsFromViews();
            }
            catch
            {
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, ConceptualEntityType element)
        {
            if (element != null)
            {
                IIntegrityCheck check = new PropagateViewKeysToStorageModel(cpc, element);
                cpc.AddIntegrityCheck(check);
            }
        }

        internal static void AddRule(CommandProcessorContext cpc, IEnumerable<ConceptualEntityType> elements)
        {
            if (elements != null)
            {
                IIntegrityCheck check = new PropagateViewKeysToStorageModel(cpc, elements);
                cpc.AddIntegrityCheck(check);
            }
        }

        private void PropagateToStorageEntities()
        {
            foreach (var entityType in _conceptualEntityTypes)
            {
                PropagateToStorageEntity(entityType);
            }
        }

        private void PropagateToStorageEntity(ConceptualEntityType entityType)
        {
            if (entityType.HasResolvableBaseType)
            {
                foreach (var key in entityType.ResolvableTopMostBaseType.ResolvableKeys)
                {
                    PropagateToStorageColumn(key);
                }
            }

            foreach (var property in entityType.Properties())
            {
                PropagateToStorageColumn(property);
            }
        }

        private void PropagateToStorageColumn(Property property)
        {
            foreach (var sp in property.GetAntiDependenciesOfType<ScalarProperty>())
            {
                // don't pick up ScalarProperty elements inside an AssociationSetMapping
                if (sp.GetParentOfType(typeof(MappingFragment)) != null)
                {
                    var column = sp.ColumnName.Target;
                    if (column != null)
                    {
                        PropagateKeyToStorageColumn(property, column);
                    }
                }
            }
        }

        private void PropagateKeyToStorageColumn(Property property, Property column)
        {
            var table = column.EntityType;
            if (table != null)
            {
                // if we are mapped to a view or a defining query then proceed with key checking
                var ses = table.EntitySet as StorageEntitySet;
                if (ses != null
                    && (ses.DefiningQuery != null || ses.StoreSchemaGeneratorTypeIsView))
                {
                    // cache this view off to process later
                    if (!_views.Contains(table))
                    {
                        _views.Add(table);
                    }

                    bool? setKey = null;

                    if (property.IsKeyProperty)
                    {
                        // this column should be a key
                        if (!column.IsKeyProperty)
                        {
                            setKey = true;
                        }
                    }
                    else
                    {
                        // this column should not be a key
                        if (column.IsKeyProperty)
                        {
                            setKey = false;
                        }
                    }

                    if (setKey != null)
                    {
                        var command = new SetKeyPropertyCommand(column, (bool)setKey);
                        CommandProcessor.InvokeSingleCommand(_cpc, command);
                    }
                }
            }
        }

        // the key scenario that this is wanting to cover is where ModelGen has included a discriminator
        // column in the inferred set of keys in TPH; this column won't be mapped and can't be part of the key
        private void UnsetUnmappedKeyColumnsFromViews()
        {
            foreach (var view in _views)
            {
                foreach (var column in view.ResolvableKeys)
                {
                    var mappings = column.GetAntiDependenciesOfType<ScalarProperty>();
                    if (mappings.Count == 0)
                    {
                        var command = new SetKeyPropertyCommand(column, false);
                        CommandProcessor.InvokeSingleCommand(_cpc, command);
                    }
                }
            }
        }
    }
}
