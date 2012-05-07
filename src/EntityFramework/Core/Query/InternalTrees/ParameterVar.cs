namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Describes a query parameter
    /// </summary>
    internal sealed class ParameterVar : Var
    {
        private readonly string m_paramName;

        internal ParameterVar(int id, TypeUsage type, string paramName)
            : base(id, VarType.Parameter, type)
        {
            m_paramName = paramName;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        internal string ParameterName
        {
            get { return m_paramName; }
        }

        /// <summary>
        /// Get the name of this Var
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal override bool TryGetName(out string name)
        {
            name = ParameterName;
            return true;
        }
    }
}