// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    /// This class is used for doing reverse-lookup of metadata when only a CLR type is known.
    /// It should never be used for POCO or proxy types, but may still be called for types that inherit
    /// from EntityObject.
    /// </summary>
    internal class ExpensiveOSpaceLoader
    {
        public virtual Dictionary<string, EdmType> LoadTypesExpensiveWay(Assembly assembly)
        {
            DebugCheck.NotNull(assembly);

            Dictionary<string, EdmType> typesInLoading;
            List<EdmItemError> errors;
            var knownAssemblies = new KnownAssembliesSet();
            AssemblyCache.LoadAssembly(
                assembly, false /*loadAllReferencedAssemblies*/,
                knownAssemblies, out typesInLoading, out errors);

            // Check for errors
            if (errors.Count != 0)
            {
                throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(errors));
            }

            return typesInLoading;
        }

        public virtual AssociationType GetRelationshipTypeExpensiveWay(Type entityClrType, string relationshipName)
        {
            DebugCheck.NotNull(entityClrType);
            DebugCheck.NotEmpty(relationshipName);

            var typesInLoading = LoadTypesExpensiveWay(entityClrType.Assembly);
            if (typesInLoading != null)
            {
                EdmType edmType;
                // Look in typesInLoading for relationship type
                if (typesInLoading.TryGetValue(relationshipName, out edmType)
                    && Helper.IsRelationshipType(edmType))
                {
                    return (AssociationType)edmType;
                }
            }
            return null;
        }

        public virtual IEnumerable<AssociationType> GetAllRelationshipTypesExpensiveWay(Assembly assembly)
        {
            DebugCheck.NotNull(assembly);

            var typesInLoading = LoadTypesExpensiveWay(assembly);
            if (typesInLoading != null)
            {
                // Iterate through the EdmTypes looking for AssociationTypes
                foreach (var edmType in typesInLoading.Values)
                {
                    if (Helper.IsAssociationType(edmType))
                    {
                        yield return (AssociationType)edmType;
                    }
                }
            }
        }
    }
}
