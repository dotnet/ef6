// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Internal helper class for query
    /// </summary>
    internal abstract class Perspective
    {
        /// <summary>
        ///     Creates a new instance of perspective class so that query can work
        ///     ignorant of all spaces
        /// </summary>
        /// <param name="metadataWorkspace"> runtime metadata container </param>
        /// <param name="targetDataspace"> target dataspace for the perspective </param>
        internal Perspective(
            MetadataWorkspace metadataWorkspace,
            DataSpace targetDataspace)
        {
            DebugCheck.NotNull(metadataWorkspace);

            m_metadataWorkspace = metadataWorkspace;
            m_targetDataspace = targetDataspace;
        }

        private readonly MetadataWorkspace m_metadataWorkspace;
        private readonly DataSpace m_targetDataspace;

        /// <summary>
        ///     Given the type in the target space and the member name in the source space,
        ///     get the corresponding member in the target space
        ///     For e.g.  consider a Conceptual Type 'Foo' with a member 'Bar' and a CLR type
        ///     'XFoo' with a member 'YBar'. If one has a reference to Foo one can
        ///     invoke GetMember(Foo,"YBar") to retrieve the member metadata for bar
        /// </summary>
        /// <param name="type"> The type in the target perspective </param>
        /// <param name="memberName"> the name of the member in the source perspective </param>
        /// <param name="ignoreCase"> Whether to do case-sensitive member look up or not </param>
        /// <param name="outMember"> returns the member in target space, if a match is found </param>
        internal virtual bool TryGetMember(StructuralType type, String memberName, bool ignoreCase, out EdmMember outMember)
        {
            DebugCheck.NotNull(type);
            Check.NotEmpty(memberName, "memberName");
            outMember = null;
            return type.Members.TryGetValue(memberName, ignoreCase, out outMember);
        }

        internal virtual bool TryGetEnumMember(EnumType type, String memberName, bool ignoreCase, out EnumMember outMember)
        {
            DebugCheck.NotNull(type);
            Check.NotEmpty(memberName, "memberName");
            outMember = null;
            return type.Members.TryGetValue(memberName, ignoreCase, out outMember);
        }

        /// <summary>
        ///     Returns the extent in the target space, for the given entity container.
        /// </summary>
        /// <param name="entityContainer"> name of the entity container in target space </param>
        /// <param name="extentName"> name of the extent </param>
        /// <param name="ignoreCase"> Whether to do case-sensitive member look up or not </param>
        /// <param name="outSet"> extent in target space, if a match is found </param>
        /// <returns> returns true, if a match is found otherwise returns false </returns>
        internal virtual bool TryGetExtent(EntityContainer entityContainer, String extentName, bool ignoreCase, out EntitySetBase outSet)
        {
            // There are no entity containers in the OSpace. So there is no mapping involved.
            // Hence the name should be a valid name in the CSpace.
            return entityContainer.BaseEntitySets.TryGetValue(extentName, ignoreCase, out outSet);
        }

        /// <summary>
        ///     Returns the function import in the target space, for the given entity container.
        /// </summary>
        internal virtual bool TryGetFunctionImport(
            EntityContainer entityContainer, String functionImportName, bool ignoreCase, out EdmFunction functionImport)
        {
            // There are no entity containers in the OSpace. So there is no mapping involved.
            // Hence the name should be a valid name in the CSpace.
            functionImport = null;
            if (ignoreCase)
            {
                functionImport =
                    entityContainer.FunctionImports.Where(
                        fi => String.Equals(fi.Name, functionImportName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            }
            else
            {
                functionImport = entityContainer.FunctionImports.Where(fi => fi.Name == functionImportName).SingleOrDefault();
            }
            return functionImport != null;
        }

        /// <summary>
        ///     Get the default entity container
        ///     returns null for any perspective other
        ///     than the CLR perspective
        /// </summary>
        /// <returns> The default container </returns>
        internal virtual EntityContainer GetDefaultContainer()
        {
            return null;
        }

        /// <summary>
        ///     Get an entity container based upon the strong name of the container
        ///     If no entity container is found, returns null, else returns the first one///
        /// </summary>
        /// <param name="name"> name of the entity container </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <param name="entityContainer"> returns the entity container if a match is found </param>
        /// <returns> returns true if a match is found, otherwise false </returns>
        internal virtual bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
        {
            return MetadataWorkspace.TryGetEntityContainer(name, ignoreCase, TargetDataspace, out entityContainer);
        }

        /// <summary>
        ///     Gets a type with the given name in the target space.
        /// </summary>
        /// <param name="fullName"> full name of the type </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <param name="typeUsage"> TypeUsage for the type </param>
        /// <returns> returns true if a match was found, otherwise false </returns>
        internal abstract bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage);

        /// <summary>
        ///     Returns overloads of a function with the given name in the target space.
        /// </summary>
        /// <param name="namespaceName"> namespace of the function </param>
        /// <param name="functionName"> name of the function </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <param name="functionOverloads"> function overloads </param>
        /// <returns> returns true if a match was found, otherwise false </returns>
        internal bool TryGetFunctionByName(
            string namespaceName, string functionName, bool ignoreCase, out IList<EdmFunction> functionOverloads)
        {
            Check.NotEmpty(namespaceName, "namespaceName");
            Check.NotEmpty(functionName, "functionName");

            var fullName = namespaceName + "." + functionName;

            // First look for a model-defined function in the target space.
            var itemCollection = m_metadataWorkspace.GetItemCollection(m_targetDataspace);
            IList<EdmFunction> overloads =
                m_targetDataspace == DataSpace.SSpace
                    ? ((StoreItemCollection)itemCollection).GetCTypeFunctions(fullName, ignoreCase)
                    : itemCollection.GetFunctions(fullName, ignoreCase);

            if (m_targetDataspace == DataSpace.CSpace)
            {
                // Then look for a function import.
                if (overloads == null
                    || overloads.Count == 0)
                {
                    EntityContainer entityContainer;
                    if (TryGetEntityContainer(namespaceName, /*ignoreCase:*/ false, out entityContainer))
                    {
                        EdmFunction functionImport;
                        if (TryGetFunctionImport(entityContainer, functionName, /*ignoreCase:*/ false, out functionImport))
                        {
                            overloads = new[] { functionImport };
                        }
                    }
                }

                // Last, look in SSpace.
                if (overloads == null
                    || overloads.Count == 0)
                {
                    ItemCollection storeItemCollection;
                    if (m_metadataWorkspace.TryGetItemCollection(DataSpace.SSpace, out storeItemCollection))
                    {
                        overloads = ((StoreItemCollection)storeItemCollection).GetCTypeFunctions(fullName, ignoreCase);
                    }
                }
            }

            functionOverloads = (overloads != null && overloads.Count > 0) ? overloads : null;
            return functionOverloads != null;
        }

        /// <summary>
        ///     Return the metadata workspace
        /// </summary>
        internal MetadataWorkspace MetadataWorkspace
        {
            get { return m_metadataWorkspace; }
        }

        /// <summary>
        ///     returns the primitive type for a given primitive type kind.
        /// </summary>
        /// <param name="primitiveTypeKind"> </param>
        /// <param name="primitiveType"> </param>
        /// <returns> </returns>
        internal virtual bool TryGetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, out PrimitiveType primitiveType)
        {
            primitiveType = m_metadataWorkspace.GetMappedPrimitiveType(primitiveTypeKind, DataSpace.CSpace);

            return (null != primitiveType);
        }

        //
        // This property will be needed to construct keys for transient types
        //
        /// <summary>
        ///     Returns the target dataspace for this perspective
        /// </summary>
        internal DataSpace TargetDataspace
        {
            get { return m_targetDataspace; }
        }
    }
}
