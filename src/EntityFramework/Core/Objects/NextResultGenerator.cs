// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal class NextResultGenerator
    {
        private readonly EntityCommand _entityCommand;
        private readonly ReadOnlyMetadataCollection<EntitySet> _entitySets;
        private readonly ObjectContext _context;
        private readonly EdmType[] _edmTypes;
        private readonly int _resultSetIndex;
        private readonly MergeOption _mergeOption;

        internal NextResultGenerator(
            ObjectContext context, EntityCommand entityCommand, EdmType[] edmTypes, ReadOnlyMetadataCollection<EntitySet> entitySets,
            MergeOption mergeOption, int resultSetIndex)
        {
            _context = context;
            _entityCommand = entityCommand;
            _entitySets = entitySets;
            _edmTypes = edmTypes;
            _resultSetIndex = resultSetIndex;
            _mergeOption = mergeOption;
        }

        internal ObjectResult<TElement> GetNextResult<TElement>(DbDataReader storeReader, bool shouldReleaseConnection)
        {
            var isNextResult = false;
            try
            {
                isNextResult = storeReader.NextResult();
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, e);
                }
                throw;
            }

            if (isNextResult)
            {
                var edmType = _edmTypes[_resultSetIndex];
                MetadataHelper.CheckFunctionImportReturnType<TElement>(edmType, _context.MetadataWorkspace);
                return _context.MaterializedDataRecord<TElement>(
                    _entityCommand, storeReader, _resultSetIndex, _entitySets, _edmTypes, _mergeOption, /*useSpatialReader:*/ true, shouldReleaseConnection);
            }
            else
            {
                return null;
            }
        }
    }
}
