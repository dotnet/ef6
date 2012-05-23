namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;

    /// <summary>
    /// Same as a ValRef in SqlServer. I just like changing names :-)
    /// </summary>
    internal abstract class Var
    {
        private readonly int m_id;
        private readonly VarType m_varType;
        private readonly TypeUsage m_type;

        internal Var(int id, VarType varType, TypeUsage type)
        {
            m_id = id;
            m_varType = varType;
            m_type = type;
        }

        /// <summary>
        /// Id of this var
        /// </summary>
        internal int Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Kind of Var
        /// </summary>
        internal VarType VarType
        {
            get { return m_varType; }
        }

        /// <summary>
        /// Datatype of this Var
        /// </summary>
        internal TypeUsage Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Try to get the name of this Var. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal virtual bool TryGetName(out string name)
        {
            name = null;
            return false;
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}", Id);
            ;
        }
    }
}
