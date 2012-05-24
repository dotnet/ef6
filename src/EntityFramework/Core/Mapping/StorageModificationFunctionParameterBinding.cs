namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    /// <summary>
    /// Binds a modification function parameter to a member of the entity or association being modified.
    /// </summary>
    internal sealed class StorageModificationFunctionParameterBinding
    {
        internal StorageModificationFunctionParameterBinding(
            FunctionParameter parameter, StorageModificationFunctionMemberPath memberPath, bool isCurrent)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(memberPath != null);

            Parameter = parameter;
            MemberPath = memberPath;
            IsCurrent = isCurrent;
        }

        /// <summary>
        /// Gets the parameter taking the value.
        /// </summary>
        internal readonly FunctionParameter Parameter;

        /// <summary>
        /// Gets the path to the entity or association member defining the value.
        /// </summary>
        internal readonly StorageModificationFunctionMemberPath MemberPath;

        /// <summary>
        /// Gets a value indicating whether the current or original
        /// member value is being bound.
        /// </summary>
        internal readonly bool IsCurrent;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "@{0}->{1}{2}", Parameter, IsCurrent ? "+" : "-", MemberPath);
        }
    }
}
