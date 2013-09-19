// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;

    internal class CodeFirstOSpaceTypeFactory : OSpaceTypeFactory
    {
        private readonly List<Action> _referenceResolutions = new List<Action>();
        private readonly Dictionary<EdmType, EdmType> _cspaceToOspace = new Dictionary<EdmType, EdmType>();
        private readonly Dictionary<string, EdmType> _loadedTypes = new Dictionary<string, EdmType>();

        public override List<Action> ReferenceResolutions
        {
            get { return _referenceResolutions; }
        }

        public override void LogLoadMessage(string message, EdmType relatedType)
        {
            // No message logging for Code First
        }

        public override void LogError(string errorMessage, EdmType relatedType)
        {
            // This is unlikely to happen since CLR types were explicitly configured by Code First
            throw new MetadataException(Strings.InvalidSchemaEncountered(errorMessage));
        }

        public override void TrackClosure(Type type)
        {
            // Nothing to do for Code First loading
        }

        public override Dictionary<EdmType, EdmType> CspaceToOspace
        {
            get { return _cspaceToOspace; }
        }

        public override Dictionary<string, EdmType> LoadedTypes
        {
            get { return _loadedTypes; }
        }

        public override void AddToTypesInAssembly(EdmType type)
        {
            // No need to collect types in assembly when using Code First
        }
    }
}
