// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;

    internal class ModificationStoredProceduresConfiguration
    {
        private ModificationStoredProcedureConfiguration _insertModificationStoredProcedureConfiguration;
        private ModificationStoredProcedureConfiguration _updateModificationStoredProcedureConfiguration;
        private ModificationStoredProcedureConfiguration _deleteModificationStoredProcedureConfiguration;

        public ModificationStoredProceduresConfiguration()
        {
        }

        private ModificationStoredProceduresConfiguration(ModificationStoredProceduresConfiguration source)
        {
            DebugCheck.NotNull(source);

            if (source._insertModificationStoredProcedureConfiguration != null)
            {
                _insertModificationStoredProcedureConfiguration = source._insertModificationStoredProcedureConfiguration.Clone();
            }

            if (source._updateModificationStoredProcedureConfiguration != null)
            {
                _updateModificationStoredProcedureConfiguration = source._updateModificationStoredProcedureConfiguration.Clone();
            }

            if (source._deleteModificationStoredProcedureConfiguration != null)
            {
                _deleteModificationStoredProcedureConfiguration = source._deleteModificationStoredProcedureConfiguration.Clone();
            }
        }

        public virtual ModificationStoredProceduresConfiguration Clone()
        {
            return new ModificationStoredProceduresConfiguration(this);
        }

        public virtual void Insert(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
        {
            DebugCheck.NotNull(modificationStoredProcedureConfiguration);

            _insertModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
        }

        public virtual void Update(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
        {
            DebugCheck.NotNull(modificationStoredProcedureConfiguration);

            _updateModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
        }

        public virtual void Delete(ModificationStoredProcedureConfiguration modificationStoredProcedureConfiguration)
        {
            DebugCheck.NotNull(modificationStoredProcedureConfiguration);

            _deleteModificationStoredProcedureConfiguration = modificationStoredProcedureConfiguration;
        }

        public ModificationStoredProcedureConfiguration InsertModificationStoredProcedureConfiguration
        {
            get { return _insertModificationStoredProcedureConfiguration; }
        }

        public ModificationStoredProcedureConfiguration UpdateModificationStoredProcedureConfiguration
        {
            get { return _updateModificationStoredProcedureConfiguration; }
        }

        public ModificationStoredProcedureConfiguration DeleteModificationStoredProcedureConfiguration
        {
            get { return _deleteModificationStoredProcedureConfiguration; }
        }

        public virtual void Configure(
            StorageEntityTypeModificationFunctionMapping modificationStoredProcedureMapping, 
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(modificationStoredProcedureMapping);
            DebugCheck.NotNull(providerManifest);

            if (_insertModificationStoredProcedureConfiguration != null)
            {
                _insertModificationStoredProcedureConfiguration
                    .Configure(modificationStoredProcedureMapping.InsertFunctionMapping, providerManifest);
            }

            if (_updateModificationStoredProcedureConfiguration != null)
            {
                _updateModificationStoredProcedureConfiguration
                    .Configure(modificationStoredProcedureMapping.UpdateFunctionMapping, providerManifest);
            }

            if (_deleteModificationStoredProcedureConfiguration != null)
            {
                _deleteModificationStoredProcedureConfiguration
                    .Configure(modificationStoredProcedureMapping.DeleteFunctionMapping, providerManifest);
            }
        }

        public void Configure(
            StorageAssociationSetModificationFunctionMapping modificationStoredProcedureMapping, 
            DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(modificationStoredProcedureMapping);
            DebugCheck.NotNull(providerManifest);

            if (_insertModificationStoredProcedureConfiguration != null)
            {
                _insertModificationStoredProcedureConfiguration
                    .Configure(modificationStoredProcedureMapping.InsertFunctionMapping, providerManifest);
            }

            if (_deleteModificationStoredProcedureConfiguration != null)
            {
                _deleteModificationStoredProcedureConfiguration
                    .Configure(modificationStoredProcedureMapping.DeleteFunctionMapping, providerManifest);
            }
        }

        public bool IsCompatibleWith(ModificationStoredProceduresConfiguration other)
        {
            DebugCheck.NotNull(other);

            if ((_insertModificationStoredProcedureConfiguration != null)
                && (other._insertModificationStoredProcedureConfiguration != null)
                && !_insertModificationStoredProcedureConfiguration.IsCompatibleWith(other._insertModificationStoredProcedureConfiguration))
            {
                return false;
            }

            if ((_deleteModificationStoredProcedureConfiguration != null)
                && (other._deleteModificationStoredProcedureConfiguration != null)
                && !_deleteModificationStoredProcedureConfiguration.IsCompatibleWith(other._deleteModificationStoredProcedureConfiguration))
            {
                return false;
            }

            return true;
        }

        public void Merge(ModificationStoredProceduresConfiguration modificationStoredProceduresConfiguration, bool allowOverride)
        {
            DebugCheck.NotNull(modificationStoredProceduresConfiguration);

            if (_insertModificationStoredProcedureConfiguration == null)
            {
                _insertModificationStoredProcedureConfiguration
                    = modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration;
            }
            else if (modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration != null)
            {
                _insertModificationStoredProcedureConfiguration
                    .Merge(modificationStoredProceduresConfiguration.InsertModificationStoredProcedureConfiguration, allowOverride);
            }

            if (_updateModificationStoredProcedureConfiguration == null)
            {
                _updateModificationStoredProcedureConfiguration
                    = modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration;
            }
            else if (modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration != null)
            {
                _updateModificationStoredProcedureConfiguration
                    .Merge(modificationStoredProceduresConfiguration.UpdateModificationStoredProcedureConfiguration, allowOverride);
            }

            if (_deleteModificationStoredProcedureConfiguration == null)
            {
                _deleteModificationStoredProcedureConfiguration
                    = modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration;
            }
            else if (modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration != null)
            {
                _deleteModificationStoredProcedureConfiguration
                    .Merge(modificationStoredProceduresConfiguration.DeleteModificationStoredProcedureConfiguration, allowOverride);
            }
        }
    }
}
