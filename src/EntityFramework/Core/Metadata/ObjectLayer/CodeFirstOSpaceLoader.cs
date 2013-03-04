// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal class CodeFirstOSpaceLoader
    {
        private readonly CodeFirstOSpaceTypeFactory _typeFactory;

        public CodeFirstOSpaceLoader(CodeFirstOSpaceTypeFactory typeFactory = null)
        {
            _typeFactory = typeFactory ?? new CodeFirstOSpaceTypeFactory();
        }

        public void LoadTypes(EdmItemCollection edmItemCollection, ObjectItemCollection objectItemCollection)
        {
            DebugCheck.NotNull(edmItemCollection);
            DebugCheck.NotNull(objectItemCollection);

            foreach (var cSpaceType in edmItemCollection.OfType<EdmType>().Where(
                t => t.BuiltInTypeKind == BuiltInTypeKind.EntityType
                     || t.BuiltInTypeKind == BuiltInTypeKind.EnumType
                     || t.BuiltInTypeKind == BuiltInTypeKind.ComplexType))
            {
                var clrType = cSpaceType.GetClrType();
                if (clrType != null)
                {
                    var oSpaceType = _typeFactory.TryCreateType(clrType, cSpaceType);
                    if (oSpaceType != null)
                    {
                        Debug.Assert(!_typeFactory.CspaceToOspace.ContainsKey(cSpaceType));
                        _typeFactory.CspaceToOspace.Add(cSpaceType, oSpaceType);
                    }
                }
                else
                {
                    Debug.Assert(!(cSpaceType is EntityType || cSpaceType is ComplexType || cSpaceType is EnumType));
                }
            }

            _typeFactory.CreateRelationships(edmItemCollection);

            foreach (var resolve in _typeFactory.ReferenceResolutions)
            {
                resolve();
            }

            foreach (var edmType in _typeFactory.LoadedTypes.Values)
            {
                edmType.SetReadOnly();
            }

            objectItemCollection.AddLoadedTypes(_typeFactory.LoadedTypes);
            objectItemCollection.OSpaceTypesLoaded = true;
        }
    }
}
