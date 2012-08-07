// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    internal class ModelCompatibilityChecker
    {
        public virtual bool CompatibleWithModel(
            InternalContext internalContext, ModelHashCalculator modelHashCalculator, bool throwIfNoMetadata)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(modelHashCalculator != null);

            if (internalContext.CodeFirstModel == null)
            {
                if (throwIfNoMetadata)
                {
                    throw Error.Database_NonCodeFirstCompatibilityCheck();
                }
                return true;
            }

            var model = internalContext.QueryForModel();
            if (model != null)
            {
                return internalContext.ModelMatches(model);
            }

            // Migrations history was not found in the database so fall back to doing a model hash compare
            // to deal with databases created using EF 4.1 and 4.2.
            var databaseModelHash = internalContext.QueryForModelHash();
            if (databaseModelHash == null)
            {
                if (throwIfNoMetadata)
                {
                    throw Error.Database_NoDatabaseMetadata();
                }
                return true;
            }

            return String.Equals(
                databaseModelHash, modelHashCalculator.Calculate(internalContext.CodeFirstModel),
                StringComparison.Ordinal);
        }
    }
}
