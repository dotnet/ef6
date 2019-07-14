// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Diagnostics;

    internal class FunctionDetailsV3RowView : FunctionDetailsRowView
    {
        private const int ProcCatalogIndex = 0;
        private const int ProcSchemaIndex = 1;
        private const int ProcNameIndex = 2;
        private const int ProcRetTypeIndex = 3;
        private const int ProcIsAggregateIndex = 4;
        private const int ProcIsComposableIndex = 5;
        private const int ProcIsBuiltinIndex = 6;
        private const int ProcIsNiladicIndex = 7;
        private const int ProcIsTvfIndex = 8;
        private const int ParamNameIndex = 9;
        private const int ParamTypeIndex = 10;
        private const int ParamDirectionIndex = 11;

        public FunctionDetailsV3RowView(object[] values)
            : base(values)
        {
            Debug.Assert(values.Length == 12, "12 columns expected for V3");
        }

        public override string Catalog
        {
            get { return ConvertDBNull<string>(Values[ProcCatalogIndex]); }
        }

        public override string Schema
        {
            get { return ConvertDBNull<string>(Values[ProcSchemaIndex]); }
        }

        public override string ProcedureName
        {
            get { return ConvertDBNull<string>(Values[ProcNameIndex]); }
        }

        public override string ReturnType
        {
            get { return ConvertDBNull<string>(Values[ProcRetTypeIndex]); }
        }

        public override bool IsIsAggregate
        {
            get { return ConvertDBNull<bool>(Values[ProcIsAggregateIndex]); }
        }

        public override bool IsBuiltIn
        {
            get { return ConvertDBNull<bool>(Values[ProcIsBuiltinIndex]); }
        }

        public override bool IsComposable
        {
            get { return ConvertDBNull<bool>(Values[ProcIsComposableIndex]); }
        }

        public override bool IsNiladic
        {
            get { return ConvertDBNull<bool>(Values[ProcIsNiladicIndex]); }
        }

        public override bool IsTvf
        {
            get { return ConvertDBNull<bool>(Values[ProcIsTvfIndex]); }
        }

        public override string ParameterName
        {
            get { return ConvertDBNull<string>(Values[ParamNameIndex]); }
        }

        public override bool IsParameterNameNull
        {
            get { return Convert.IsDBNull(Values[ParamNameIndex]); }
        }

        public override string ParameterType
        {
            get { return ConvertDBNull<string>(Values[ParamTypeIndex]); }
        }

        public override bool IsParameterTypeNull
        {
            get { return Convert.IsDBNull(Values[ParamTypeIndex]); }
        }

        public override string ProcParameterMode
        {
            get { return ConvertDBNull<string>(Values[ParamDirectionIndex]); }
        }

        public override bool IsParameterModeNull
        {
            get { return Convert.IsDBNull(Values[ParamDirectionIndex]); }
        }
    }
}
