//****************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//****************************************************************************

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider
{
	/// <summary>
	/// Represents constant string values for all the supported data object
	/// types.  This list must be in sync with the data object support XML.
	/// </summary>
	internal static class SqlObjectTypes
	{
		public const string Root = "";
		public const string User = "User";
		public const string Table = "Table";
		public const string Column = "Column";
		public const string Index = "Index";
		public const string IndexColumn = "IndexColumn";
		public const string ForeignKey = "ForeignKey";
		public const string ForeignKeyColumn = "ForeignKeyColumn";
		public const string View = "View";
		public const string ViewColumn = "ViewColumn";
		public const string StoredProcedure = "StoredProcedure";
		public const string StoredProcedureParameter = "StoredProcedureParameter";
		public const string StoredProcedureColumn = "StoredProcedureColumn";
		public const string Function = "Function";
		public const string FunctionParameter = "FunctionParameter";
		public const string FunctionColumn = "FunctionColumn";
		public const string UserDefinedType = "UserDefinedType";
	}
}
