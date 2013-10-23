// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Legacy = System.Data.Common;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System.Data.Entity.Core.Common;

    internal class LegacyDbCommandDefinitionWrapper : DbCommandDefinition
    {
        private readonly Legacy.DbCommandDefinition _wrappedDbCommandDefinition;

        public LegacyDbCommandDefinitionWrapper(Legacy.DbCommandDefinition wrappedDbCommandDefinition)
        {
            _wrappedDbCommandDefinition = wrappedDbCommandDefinition;
        }

        public override Legacy.DbCommand CreateCommand()
        {
            return _wrappedDbCommandDefinition.CreateCommand();
        }
    }
}
