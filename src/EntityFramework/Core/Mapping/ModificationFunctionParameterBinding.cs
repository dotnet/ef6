// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    // <summary>
    // Binds a modification function parameter to a member of the entity or association being modified.
    // </summary>
    internal sealed class ModificationFunctionParameterBinding : MappingItem
    {
        internal ModificationFunctionParameterBinding(
            FunctionParameter parameter, ModificationFunctionMemberPath memberPath, bool isCurrent)
        {
            DebugCheck.NotNull(parameter);
            DebugCheck.NotNull(memberPath);

            Parameter = parameter;
            MemberPath = memberPath;
            IsCurrent = isCurrent;
        }

        // <summary>
        // Gets the parameter taking the value.
        // </summary>
        internal readonly FunctionParameter Parameter;

        // <summary>
        // Gets the path to the entity or association member defining the value.
        // </summary>
        internal readonly ModificationFunctionMemberPath MemberPath;

        // <summary>
        // Gets a value indicating whether the current or original
        // member value is being bound.
        // </summary>
        internal readonly bool IsCurrent;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "@{0}->{1}{2}", Parameter, IsCurrent ? "+" : "-", MemberPath);
        }
    }
}
