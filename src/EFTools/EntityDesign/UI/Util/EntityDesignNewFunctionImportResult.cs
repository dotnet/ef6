// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Util
{
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal struct EntityDesignNewFunctionImportResult
    {
        public DialogResult DialogResult { get; set; }
        public Function Function { get; set; }
        public string FunctionName { get; set; }
        public bool IsComposable { get; set; }
        public object ReturnType { get; set; }
        public IDataSchemaProcedure Schema { get; set; }
    }
}
