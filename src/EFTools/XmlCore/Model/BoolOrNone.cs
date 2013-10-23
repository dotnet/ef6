// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     Class that represents a choice between true, false or '(None)' i.e. "not set"
    /// </summary>
    [Serializable]
    internal class BoolOrNone : StringOrPrimitive<bool>
    {
        internal static readonly BoolOrNone NoneValue = new BoolOrNone(Resources.NoneDisplayValueUsedForUX);
        internal static readonly BoolOrNone TrueValue = new BoolOrNone(true);
        internal static readonly BoolOrNone FalseValue = new BoolOrNone(false);

        private BoolOrNone(bool primitiveValue)
            : base(primitiveValue)
        {
            // only use static values above
        }

        private BoolOrNone(string stringVal)
            : base(stringVal)
        {
            // only use static values above
        }
    }
}
