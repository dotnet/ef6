namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Holds the view generated for a given OFTYPE(Extent, Type) combination.
    /// </summary>
    internal sealed class GeneratedView : InternalBase
    {
        #region Factory

        /// <summary>
        /// Creates generated view object for the combination of the <paramref name="extent"/> and the <paramref name="type"/>. 
        /// This constructor is used for regular cell-based view generation.
        /// </summary>
        internal static GeneratedView CreateGeneratedView(
            EntitySetBase extent,
            EdmType type,
            DbQueryCommandTree commandTree,
            string eSQL,
            StorageMappingItemCollection mappingItemCollection,
            ConfigViewGenerator config)
        {
            // If config.GenerateEsql is specified, eSQL must be non-null.
            // If config.GenerateEsql is false, commandTree is non-null except the case when loading pre-compiled eSQL views.
            Debug.Assert(!config.GenerateEsql || !String.IsNullOrEmpty(eSQL), "eSQL must be specified");

            DiscriminatorMap discriminatorMap = null;
            if (commandTree != null)
            {
                commandTree = ViewSimplifier.SimplifyView(extent, commandTree);

                // See if the view matches the "discriminated" pattern (allows simplification of generated store commands)
                if (extent.BuiltInTypeKind
                    == BuiltInTypeKind.EntitySet)
                {
                    if (DiscriminatorMap.TryCreateDiscriminatorMap((EntitySet)extent, commandTree.Query, out discriminatorMap))
                    {
                        Debug.Assert(discriminatorMap != null, "discriminatorMap == null after it has been created");
                    }
                }
            }

            return new GeneratedView(extent, type, commandTree, eSQL, discriminatorMap, mappingItemCollection, config);
        }

        /// <summary>
        /// Creates generated view object for the combination of the <paramref name="extent"/> and the <paramref name="type"/>. 
        /// This constructor is used for FK association sets only.
        /// </summary>
        internal static GeneratedView CreateGeneratedViewForFKAssociationSet(
            EntitySetBase extent,
            EdmType type,
            DbQueryCommandTree commandTree,
            StorageMappingItemCollection mappingItemCollection,
            ConfigViewGenerator config)
        {
            return new GeneratedView(extent, type, commandTree, null, null, mappingItemCollection, config);
        }

        /// <summary>
        /// Creates generated view object for the combination of the <paramref name="setMapping"/>.Set and the <paramref name="type"/>. 
        /// This constructor is used for user-defined query views only.
        /// </summary>
        internal static bool TryParseUserSpecifiedView(
            StorageSetMapping setMapping,
            EntityTypeBase type,
            string eSQL,
            bool includeSubtypes,
            StorageMappingItemCollection mappingItemCollection,
            ConfigViewGenerator config,
            /*out*/ IList<EdmSchemaError> errors,
            out GeneratedView generatedView)
        {
            var failed = false;

            DbQueryCommandTree commandTree;
            DiscriminatorMap discriminatorMap;
            Exception parserException;
            if (
                !TryParseView(
                    eSQL, true, setMapping.Set, mappingItemCollection, config, out commandTree, out discriminatorMap, out parserException))
            {
                var error = new EdmSchemaError(
                    Strings.Mapping_Invalid_QueryView2(setMapping.Set.Name, parserException.Message),
                    (int)StorageMappingErrorCode.InvalidQueryView, EdmSchemaErrorSeverity.Error,
                    setMapping.EntityContainerMapping.SourceLocation, setMapping.StartLineNumber, setMapping.StartLinePosition,
                    parserException);
                errors.Add(error);
                failed = true;
            }
            else
            {
                Debug.Assert(commandTree != null, "commandTree not set after parsing the view");

                // Verify that all expressions appearing in the view are supported.
                foreach (var error in ViewValidator.ValidateQueryView(commandTree, setMapping, type, includeSubtypes))
                {
                    errors.Add(error);
                    failed = true;
                }

                // Verify that the result type of the query view is assignable to the element type of the entityset
                var queryResultType = (commandTree.Query.ResultType.EdmType) as CollectionType;
                if ((queryResultType == null)
                    || (!setMapping.Set.ElementType.IsAssignableFrom(queryResultType.TypeUsage.EdmType)))
                {
                    var error = new EdmSchemaError(
                        Strings.Mapping_Invalid_QueryView_Type(setMapping.Set.Name),
                        (int)StorageMappingErrorCode.InvalidQueryViewResultType, EdmSchemaErrorSeverity.Error,
                        setMapping.EntityContainerMapping.SourceLocation, setMapping.StartLineNumber, setMapping.StartLinePosition);
                    errors.Add(error);
                    failed = true;
                }
            }

            if (!failed)
            {
                generatedView = new GeneratedView(setMapping.Set, type, commandTree, eSQL, discriminatorMap, mappingItemCollection, config);
                return true;
            }
            else
            {
                generatedView = null;
                return false;
            }
        }

        private GeneratedView(
            EntitySetBase extent,
            EdmType type,
            DbQueryCommandTree commandTree,
            string eSQL,
            DiscriminatorMap discriminatorMap,
            StorageMappingItemCollection mappingItemCollection,
            ConfigViewGenerator config)
        {
            // At least one of the commandTree or eSQL must be specified. 
            // Both are specified in the case of user-defined views.
            Debug.Assert(commandTree != null || !String.IsNullOrEmpty(eSQL), "commandTree or eSQL must be specified");

            m_extent = extent;
            m_type = type;
            m_commandTree = commandTree;
            m_eSQL = eSQL;
            m_discriminatorMap = discriminatorMap;
            m_mappingItemCollection = mappingItemCollection;
            m_config = config;

            if (m_config.IsViewTracing)
            {
                var trace = new StringBuilder(1024);
                ToCompactString(trace);
                Helpers.FormatTraceLine("CQL view for {0}", trace.ToString());
            }
        }

        #endregion

        #region Fields

        private readonly EntitySetBase m_extent;
        private readonly EdmType m_type;
        private DbQueryCommandTree m_commandTree; //We cache CQTs for Update Views sicne that is the one update stack works of.
        private readonly string m_eSQL;
        private Node m_internalTreeNode; //we cache IQTs for Query Views since that is the one query stack works of.
        private DiscriminatorMap m_discriminatorMap;
        private readonly StorageMappingItemCollection m_mappingItemCollection;
        private readonly ConfigViewGenerator m_config;

        #endregion

        #region Properties

        internal string eSQL
        {
            get { return m_eSQL; }
        }

        #endregion

        #region Methods

        internal DbQueryCommandTree GetCommandTree()
        {
            if (m_commandTree == null)
            {
                Debug.Assert(!String.IsNullOrEmpty(m_eSQL), "m_eSQL must be initialized");

                Exception parserException;
                if (TryParseView(
                    m_eSQL, false, m_extent, m_mappingItemCollection, m_config, out m_commandTree, out m_discriminatorMap,
                    out parserException))
                {
                    Debug.Assert(m_commandTree != null, "m_commandTree not set after parsing the view");
                    return m_commandTree;
                }
                else
                {
                    throw new MappingException(Strings.Mapping_Invalid_QueryView(m_extent.Name, parserException.Message));
                }
            }
            return m_commandTree;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "projectOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal Node GetInternalTree(Command targetIqtCommand)
        {
            Debug.Assert(m_extent.EntityContainer.DataSpace == DataSpace.CSpace, "Internal Tree should be asked only for query view");
            if (m_internalTreeNode == null)
            {
                var tree = GetCommandTree();
                // Convert this into an ITree first
                var itree = ITreeGenerator.Generate(tree, m_discriminatorMap);
                // Pull out the root physical project-op, and copy this itree into our own itree
                PlanCompiler.Assert(
                    itree.Root.Op.OpType == OpType.PhysicalProject,
                    "Expected a physical projectOp at the root of the tree - found " + itree.Root.Op.OpType);
                // #554756: VarVec enumerators are not cached on the shared Command instance.
                itree.DisableVarVecEnumCaching();
                m_internalTreeNode = itree.Root.Child0;
            }
            Debug.Assert(m_internalTreeNode != null, "m_internalTreeNode != null");
            return OpCopier.Copy(targetIqtCommand, m_internalTreeNode);
        }

        /// <summary>
        /// Given an extent and its corresponding view, invokes the parser to check if the view definition is syntactically correct.
        /// Iff parsing succeeds: <paramref name="commandTree"/> and <paramref name="discriminatorMap"/> are set to the parse result and method returns true,
        /// otherwise if parser has thrown a catchable exception, it is returned via <paramref name="parserException"/> parameter, 
        /// otherwise exception is re-thrown.
        /// </summary>
        private static bool TryParseView(
            string eSQL,
            bool isUserSpecified,
            EntitySetBase extent,
            StorageMappingItemCollection mappingItemCollection,
            ConfigViewGenerator config,
            out DbQueryCommandTree commandTree,
            out DiscriminatorMap discriminatorMap,
            out Exception parserException)
        {
            commandTree = null;
            discriminatorMap = null;
            parserException = null;

            // We do not catch any internal exceptions any more
            config.StartSingleWatch(PerfType.ViewParsing);
            try
            {
                // If it is a user specified view, allow all queries. Otherwise parse the view in the restricted mode.
                var compilationMode = ParserOptions.CompilationMode.RestrictedViewGenerationMode;
                if (isUserSpecified)
                {
                    compilationMode = ParserOptions.CompilationMode.UserViewGenerationMode;
                }

                Debug.Assert(!String.IsNullOrEmpty(eSQL), "eSQL query is not specified");
                commandTree = (DbQueryCommandTree)ExternalCalls.CompileView(eSQL, mappingItemCollection, compilationMode);

                // For non user-specified views, perform simplification.
                if (!isUserSpecified)
                {
                    commandTree = ViewSimplifier.SimplifyView(extent, commandTree);
                }

                // See if the view matches the "discriminated" pattern (allows simplification of generated store commands)
                if (extent.BuiltInTypeKind
                    == BuiltInTypeKind.EntitySet)
                {
                    if (DiscriminatorMap.TryCreateDiscriminatorMap((EntitySet)extent, commandTree.Query, out discriminatorMap))
                    {
                        Debug.Assert(discriminatorMap != null, "discriminatorMap == null after it has been created");
                    }
                }
            }
            catch (Exception e)
            {
                // Catching all the exception types since Query parser seems to be throwing veriety of
                // exceptions - EntityException, ArgumentException, ArgumentNullException etc.
                if (e.IsCatchableExceptionType())
                {
                    parserException = e;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                config.StopSingleWatch(PerfType.ViewParsing);
            }

            Debug.Assert(commandTree != null || parserException != null, "Either commandTree or parserException is expected.");
            // Note: m_commandTree might have been initialized by a previous call to this method, so in consequent calls it might occur that
            // both m_commandTree and parserException are not null - this would mean that the last parse attempt failed, but m_commandTree value is 
            // preserved from the previous call.

            return parserException == null;
        }

        #endregion

        #region String Methods

        internal override void ToCompactString(StringBuilder builder)
        {
            var ofTypeView = m_type != m_extent.ElementType;

            if (ofTypeView)
            {
                builder.Append("OFTYPE(");
            }
            builder.AppendFormat("{0}.{1}", m_extent.EntityContainer.Name, m_extent.Name);
            if (ofTypeView)
            {
                builder.Append(", ").Append(m_type.Name).Append(')');
            }
            builder.AppendLine(" = ");

            if (!String.IsNullOrEmpty(m_eSQL))
            {
                builder.Append(m_eSQL);
            }
            else
            {
                builder.Append(m_commandTree.Print());
            }
        }

        #endregion
    }
}
