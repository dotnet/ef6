// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>An immutable class that implements the basic functionality for the Query, Insert, Update, Delete, and function invocation command tree types. </summary>
    public abstract class DbCommandTree
    {
        // Metadata collection
        private readonly MetadataWorkspace _metadata;
        private readonly DataSpace _dataSpace;

        internal DbCommandTree()
        {
        }

        /// <summary>
        /// Initializes a new command tree with a given metadata workspace.
        /// </summary>
        /// <param name="metadata"> The metadata workspace against which the command tree should operate. </param>
        /// <param name="dataSpace"> The logical 'space' that metadata in the expressions used in this command tree must belong to. </param>
        internal DbCommandTree(MetadataWorkspace metadata, DataSpace dataSpace)
        {
            // Ensure the metadata workspace is non-null
            DebugCheck.NotNull(metadata);

            // Ensure that the data space value is valid
            if (!IsValidDataSpace(dataSpace))
            {
                throw new ArgumentException(Strings.Cqt_CommandTree_InvalidDataSpace, "dataSpace");
            }

            _metadata = metadata;
            _dataSpace = dataSpace;
        }

        /// <summary>
        /// Gets the name and corresponding type of each parameter that can be referenced within this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCommandTree" />
        /// .
        /// </summary>
        /// <returns>
        /// The name and corresponding type of each parameter that can be referenced within this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbCommandTree" />
        /// .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<KeyValuePair<string, TypeUsage>> Parameters
        {
            get { return GetParameters(); }
        }

        #region Internal Implementation

        /// <summary>
        /// Gets the kind of this command tree.
        /// </summary>
        public abstract DbCommandTreeKind CommandTreeKind { get; }

        /// <summary>
        /// Gets the name and type of each parameter declared on the command tree.
        /// </summary>
        internal abstract IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters();

        /// <summary>
        /// Gets the metadata workspace used by this command tree.
        /// </summary>
        public virtual MetadataWorkspace MetadataWorkspace
        {
            get { return _metadata; }
        }

        /// <summary>
        /// Gets the data space in which metadata used by this command tree must reside.
        /// </summary>
        public virtual DataSpace DataSpace
        {
            get { return _dataSpace; }
        }

        #region Dump/Print Support

        internal void Dump(ExpressionDumper dumper)
        {
            //
            // Dump information about this command tree to the specified ExpressionDumper
            //
            // First dump standard information - the DataSpace of the command tree and its parameters
            //
            var attrs = new Dictionary<string, object>();
            attrs.Add("DataSpace", DataSpace);
            dumper.Begin(GetType().Name, attrs);

            //
            // The name and type of each Parameter in turn is added to the output
            //
            dumper.Begin("Parameters", null);
            foreach (var param in Parameters)
            {
                var paramAttrs = new Dictionary<string, object>();
                paramAttrs.Add("Name", param.Key);
                dumper.Begin("Parameter", paramAttrs);
                dumper.Dump(param.Value, "ParameterType");
                dumper.End("Parameter");
            }
            dumper.End("Parameters");

            //
            // Delegate to the derived type's implementation that dumps the structure of the command tree
            //
            DumpStructure(dumper);

            //
            // Matching call to End to correspond with the call to Begin above
            //
            dumper.End(GetType().Name);
        }

        internal abstract void DumpStructure(ExpressionDumper dumper);

#if DEBUG
        internal string DumpXml()
        {
            //
            // This is a convenience method that dumps the command tree in an XML format.
            // This is intended primarily as a debugging aid to allow inspection of the tree structure.
            //
            // Create a new MemoryStream that the XML dumper should write to.
            //
            using (var stream = new MemoryStream())
            {
                //
                // Create the dumper
                //
                var dumper = new XmlExpressionDumper(stream);

                //
                // Dump this tree and then close the XML dumper so that the end document tag is written
                // and the output is flushed to the stream.
                //
                Dump(dumper);
                dumper.Close();

                //
                // Construct a string from the resulting memory stream and return it to the caller
                //
                return XmlExpressionDumper.DefaultEncoding.GetString(stream.ToArray());
            }
        }
#endif

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this command.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this command.
        /// </returns>
        public override string ToString()
        {
            return Print();
        }

        internal string Print()
        {
            return PrintTree(new ExpressionPrinter());
        }

        internal abstract string PrintTree(ExpressionPrinter printer);

        #endregion

        internal static bool IsValidDataSpace(DataSpace dataSpace)
        {
            return (DataSpace.OSpace == dataSpace ||
                    DataSpace.CSpace == dataSpace ||
                    DataSpace.SSpace == dataSpace);
        }

        internal static bool IsValidParameterName(string name)
        {
            return (!string.IsNullOrWhiteSpace(name)
                    && name.IsValidUndottedName());
        }

        #endregion
    }
}
