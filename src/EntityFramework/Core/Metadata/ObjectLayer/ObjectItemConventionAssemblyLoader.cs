// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Reflection;

    internal class ObjectItemConventionAssemblyLoader : ObjectItemAssemblyLoader
    {
        internal class ConventionOSpaceTypeFactory : OSpaceTypeFactory
        {
            private readonly ObjectItemConventionAssemblyLoader _loader;

            public ConventionOSpaceTypeFactory(ObjectItemConventionAssemblyLoader loader)
            {
                DebugCheck.NotNull(loader);

                _loader = loader;
            }

            public override List<Action> ReferenceResolutions
            {
                get { return _loader._referenceResolutions; }
            }

            public override void LogLoadMessage(string message, EdmType relatedType)
            {
                _loader.SessionData.LoadMessageLogger.LogLoadMessage(message, relatedType);
            }

            public override void LogError(string errorMessage, EdmType relatedType)
            {
                var message = _loader.SessionData.LoadMessageLogger
                                     .CreateErrorMessageWithTypeSpecificLoadLogs(errorMessage, relatedType);

                _loader.SessionData.EdmItemErrors.Add(new EdmItemError(message));
            }

            public override void TrackClosure(Type type)
            {
                _loader.TrackClosure(type);
            }

            public override Dictionary<EdmType, EdmType> CspaceToOspace
            {
                get { return _loader.SessionData.CspaceToOspace; }
            }

            public override Dictionary<string, EdmType> LoadedTypes
            {
                get { return _loader.SessionData.TypesInLoading; }
            }

            public override void AddToTypesInAssembly(EdmType type)
            {
                _loader.CacheEntry.TypesInAssembly.Add(type);
            }
        }

        public new virtual MutableAssemblyCacheEntry CacheEntry
        {
            get { return (MutableAssemblyCacheEntry)base.CacheEntry; }
        }

        private readonly List<Action> _referenceResolutions = new List<Action>();

        private readonly ConventionOSpaceTypeFactory _factory;

        internal ObjectItemConventionAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
            : base(assembly, new MutableAssemblyCacheEntry(), sessionData)
        {
            SessionData.RegisterForLevel1PostSessionProcessing(this);

            _factory = new ConventionOSpaceTypeFactory(this);
        }

        protected override void LoadTypesFromAssembly()
        {
            foreach (var type in SourceAssembly.GetAccessibleTypes())
            {
                EdmType cspaceType;
                if (TryGetCSpaceTypeMatch(type, out cspaceType))
                {
                    if (type.IsValueType()
                        && !type.IsEnum())
                    {
                        SessionData.LoadMessageLogger.LogLoadMessage(
                            Strings.Validator_OSpace_Convention_Struct(cspaceType.FullName, type.FullName), cspaceType);
                        continue;
                    }

                    var ospaceType = _factory.TryCreateType(type, cspaceType);
                    if (ospaceType != null)
                    {
                        Debug.Assert(
                            ospaceType is StructuralType || Helper.IsEnumType(ospaceType), "Only StructuralType or EnumType expected.");

                        CacheEntry.TypesInAssembly.Add(ospaceType);
                        // check for duplicates so we don't cause an ArgumentException, 
                        // Mapping will do the actual error for the duplicate type later
                        if (!SessionData.CspaceToOspace.ContainsKey(cspaceType))
                        {
                            SessionData.CspaceToOspace.Add(cspaceType, ospaceType);
                        }
                        else
                        {
                            // at this point there is already a Clr Type that is structurally matched to this CSpace type, we throw exception
                            var previousOSpaceType = SessionData.CspaceToOspace[cspaceType];
                            SessionData.EdmItemErrors.Add(
                                new EdmItemError(
                                    Strings.Validator_OSpace_Convention_AmbiguousClrType(
                                        cspaceType.Name, previousOSpaceType.ClrType.FullName, type.FullName)));
                        }
                    }
                }
            }

            if (SessionData.TypesInLoading.Count == 0)
            {
                Debug.Assert(CacheEntry.ClosureAssemblies.Count == 0, "How did we get closure assemblies?");

                // since we didn't find any types, don't lock into convention based
                SessionData.ObjectItemAssemblyLoaderFactory = null;
            }
        }

        protected override void AddToAssembliesLoaded()
        {
            SessionData.AssembliesLoaded.Add(SourceAssembly, CacheEntry);
        }

        private bool TryGetCSpaceTypeMatch(Type type, out EdmType cspaceType)
        {
            // brute force try and find a matching name
            KeyValuePair<EdmType, int> pair;
            if (SessionData.ConventionCSpaceTypeNames.TryGetValue(type.Name, out pair))
            {
                if (pair.Value == 1)
                {
                    // we found a type match
                    cspaceType = pair.Key;
                    return true;
                }
                else
                {
                    Debug.Assert(pair.Value > 1, "how did we get a negative count of types in the dictionary?");
                    SessionData.EdmItemErrors.Add(
                        new EdmItemError(Strings.Validator_OSpace_Convention_MultipleTypesWithSameName(type.Name)));
                }
            }

            cspaceType = null;
            return false;
        }

        internal override void OnLevel1SessionProcessing()
        {
            CreateRelationships();

            foreach (var resolve in _referenceResolutions)
            {
                resolve();
            }

            base.OnLevel1SessionProcessing();
        }

        internal virtual void TrackClosure(Type type)
        {
            if (SourceAssembly != type.Assembly()
                &&
                !CacheEntry.ClosureAssemblies.Contains(type.Assembly())
                &&
                !(type.IsGenericType() &&
                  (
                      EntityUtil.IsAnICollection(type) || // EntityCollection<>, List<>, ICollection<>
                      type.GetGenericTypeDefinition() == typeof(EntityReference<>) ||
                      type.GetGenericTypeDefinition() == typeof(Nullable<>)
                  )
                 )
                )
            {
                CacheEntry.ClosureAssemblies.Add(type.Assembly());
            }

            if (type.IsGenericType())
            {
                foreach (var genericArgument in type.GetGenericArguments())
                {
                    TrackClosure(genericArgument);
                }
            }
        }

        private void CreateRelationships()
        {
            if (SessionData.ConventionBasedRelationshipsAreLoaded)
            {
                return;
            }

            SessionData.ConventionBasedRelationshipsAreLoaded = true;

            _factory.CreateRelationships(SessionData.EdmItemCollection);
        }

        internal static bool SessionContainsConventionParameters(ObjectItemLoadingSessionData sessionData)
        {
            return sessionData.EdmItemCollection != null;
        }

        internal static ObjectItemAssemblyLoader Create(Assembly assembly, ObjectItemLoadingSessionData sessionData)
        {
            if (!ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
            {
                return new ObjectItemConventionAssemblyLoader(assembly, sessionData);
            }

            // we were loading in convention mode, and ran into an assembly that can't be loaded by convention
            sessionData.EdmItemErrors.Add(
                new EdmItemError(Strings.Validator_OSpace_Convention_AttributeAssemblyReferenced(assembly.FullName)));
            return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
        }
    }
}
