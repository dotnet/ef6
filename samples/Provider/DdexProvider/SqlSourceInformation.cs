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

using System;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider
{
	/// <summary>
	/// Represents a custom data source information class that is able to
	/// provide data source information values that require some form of
	/// computation, perhaps based on an active connection.
	/// </summary>
	internal class SqlSourceInformation : AdoDotNetSourceInformation
	{
		#region Constructors

		public SqlSourceInformation()
		{
			AddProperty(DefaultSchema);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// RetrieveValue is called once per property that was identified
		/// as existing but without a value (specified in the constructor).
		/// For the purposes of this sample, only one property needs to be
		/// computed - DefaultSchema.  To retrieve this value a SQL statement
		/// is executed.
		/// </summary>
		protected override object RetrieveValue(string propertyName)
		{
			if (propertyName.Equals(DefaultSchema,
					StringComparison.OrdinalIgnoreCase))
			{
				if (Site.State != DataConnectionState.Open)
				{
					Site.Open();
				}
				SqlConnection conn = Connection as SqlConnection;
				Debug.Assert(conn != null, "Invalid provider object.");
				if (conn != null)
				{
					SqlCommand comm = conn.CreateCommand();
					try
					{
						comm.CommandText = "SELECT SCHEMA_NAME()";
						return comm.ExecuteScalar() as string;
					}
					catch (SqlException)
					{
						// We let the base class apply default behavior
					}
				}
			}
			return base.RetrieveValue(propertyName);
		}

		#endregion
	}
}
