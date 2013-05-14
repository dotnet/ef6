// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;

    /// <summary>
    ///     Same as a ValRef in SqlServer.
    /// </summary>
    internal abstract class Var
    {
        private readonly int _id;
        private readonly VarType _varType;
        private readonly TypeUsage _type;

        internal Var(int id, VarType varType, TypeUsage type)
        {
            _id = id;
            _varType = varType;
            _type = type;
        }

        /// <summary>
        ///     Id of this var
        /// </summary>
        internal int Id
        {
            get { return _id; }
        }

        /// <summary>
        ///     Kind of Var
        /// </summary>
        internal VarType VarType
        {
            get { return _varType; }
        }

        /// <summary>
        ///     Datatype of this Var
        /// </summary>
        internal TypeUsage Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Try to get the name of this Var.
        /// </summary>
        internal virtual bool TryGetName(out string name)
        {
            name = null;
            return false;
        }

        /// <summary>
        ///     Debugging support
        /// </summary>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}", Id);
            ;
        }
    }
}
