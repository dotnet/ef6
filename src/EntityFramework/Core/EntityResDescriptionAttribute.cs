// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.ComponentModel;
    using System.Data.Entity.Resources;

    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum
        | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
        | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate
        | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
    internal sealed class EntityResDescriptionAttribute : DescriptionAttribute
    {
        private bool _replaced;

        public override string Description
        {
            get
            {
                if (!_replaced)
                {
                    _replaced = true;
                    DescriptionValue = EntityRes.GetString(base.Description);
                }
                return base.Description;
            }
        }

        public EntityResDescriptionAttribute(string description)
            : base(description)
        {
        }
    }
}
