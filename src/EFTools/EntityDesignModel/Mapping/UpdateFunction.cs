// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;

    internal class UpdateFunction : ModificationFunction
    {
        internal static readonly string ElementName = "UpdateFunction";

        internal UpdateFunction(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(parent is ModificationFunctionMapping, "parent should be a ModificationFunctionMapping");
            _functionType = ModificationFunctionType.Update;
        }

        private string DisplayNameInternal(bool localize)
        {
            string resource;
            if (localize)
            {
                resource = Resources.MappingModel_UpdateFunctionDisplayName;
            }
            else
            {
                resource = "{0} (UpdateFunction)";
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                resource,
                FunctionName.RefName);
        }

        internal override string DisplayName
        {
            get { return DisplayNameInternal(true); }
        }

        internal override string NonLocalizedDisplayName
        {
            get { return DisplayNameInternal(false); }
        }
    }
}
