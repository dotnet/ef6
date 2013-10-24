// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    /// Binds a modification function parameter to a member of the entity or association being modified.
    /// </summary>
    public sealed class ModificationFunctionParameterBinding : MappingItem
    {
        private readonly FunctionParameter _parameter;
        private readonly ModificationFunctionMemberPath _memberPath;
        private readonly bool _isCurrent;

        /// <summary>
        /// Initializes a new ModificationFunctionParameterBinding instance.
        /// </summary>
        /// <param name="parameter">The parameter taking the value.</param>
        /// <param name="memberPath">The path to the entity or association member defining the value.</param>
        /// <param name="isCurrent">A flag indicating whether the current or original member value is being bound.</param>
        public ModificationFunctionParameterBinding(
            FunctionParameter parameter, ModificationFunctionMemberPath memberPath, bool isCurrent)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(memberPath, "memberPath");

            _parameter = parameter;
            _memberPath = memberPath;
            _isCurrent = isCurrent;
        }

        /// <summary>
        /// Gets the parameter taking the value.
        /// </summary>
        public FunctionParameter Parameter
        {
            get { return _parameter; }
        }

        /// <summary>
        /// Gets the path to the entity or association member defining the value.
        /// </summary>
        public ModificationFunctionMemberPath MemberPath
        {
            get { return _memberPath; }
        }

        /// <summary>
        /// Gets a flag indicating whether the current or original
        /// member value is being bound.
        /// </summary>
        public bool IsCurrent
        {
            get { return _isCurrent; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "@{0}->{1}{2}", Parameter, IsCurrent ? "+" : "-", MemberPath);
        }

        internal override void SetReadOnly()
        {
            SetReadOnly(_memberPath);

            base.SetReadOnly();
        }
    }
}
