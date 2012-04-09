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
using System.Data.SqlClient;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider.UnitTests
{
	[TestClass]
	public class SqlObjectSelectorTests
	{
		#region Initialize Methods

		[ClassInitialize]
		public static void InitializeClass(TestContext testContext)
		{
			_connectionMock = new GenericMockFactory("DataConnection",
				new Type[] { typeof(IVsDataConnection) }).GetInstance();
		}

		[TestInitialize]
		public void Initialize()
		{
			_object = new SqlObjectSelector();
			_object.Site = _connectionMock as IVsDataConnection;
		}

		#endregion

		#region SelectObjects Tests

		//
		// All the SelectObjects tests require the default instance of SQL
		// Server Express Edition to be installed (named SQLEXPRESS).
		//

		[TestMethod]
		public void SelectObjectsTest()
		{
			// Prepare to call method under test
			SqlConnection conn = new SqlConnection(
				"Data Source=.\\SQLEXPRESS;" +
				"Integrated Security=true;" +
				"Initial Catalog=master");
			_connectionMock.AddMethodCallback(
				typeof(IVsDataConnection).FullName + ".GetLockedProviderObject",
				delegate(object sender, CallbackArgs e)
				{
					e.ReturnValue = conn;
				}
			);
			_connectionMock.AddMethodCallback(
				typeof(IVsDataConnection).FullName + ".get_State",
				delegate(object sender, CallbackArgs e)
				{
					e.ReturnValue = (DataConnectionState)(int)conn.State;
				}
			);
			_connectionMock.AddMethodCallback(
				typeof(IVsDataConnection).FullName + ".Open",
				delegate(object sender, CallbackArgs e)
				{
					conn.Open();
				}
			);

			// Call method under test
			IVsDataReader result = _object.SelectObjects(
				SqlObjectTypes.Root, null, null);

			// Verify some results
			using (result)
			{
				Assert.IsTrue(result.Read());
				Assert.AreEqual("SQLEXPRESS", result.GetItem("Instance"));
				Assert.AreEqual("master", result.GetItem("Database"));
			}

			// Call method under test
			result = _object.SelectObjects(SqlObjectTypes.Index,
				new object[] { "master", "dbo", "sysobjects" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(SqlObjectTypes.IndexColumn,
				new object[] { "master", "dbo", "sysobjects" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(SqlObjectTypes.ForeignKey,
				new object[] { "master", "dbo", "sysobjects" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(SqlObjectTypes.ForeignKeyColumn,
				new object[] { "master", "dbo", "sysobjects" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(SqlObjectTypes.StoredProcedure,
				new object[] { "master", "dbo", "sp_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(
				SqlObjectTypes.StoredProcedureParameter,
				new object[] { "master", "dbo", "sp_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(
				SqlObjectTypes.StoredProcedureColumn,
				new object[] { "master", "dbo", "sp_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(
				SqlObjectTypes.Function,
				new object[] { "master", "dbo", "fn_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(
				SqlObjectTypes.FunctionParameter,
				new object[] { "master", "dbo", "fn_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}

			// Call method under test
			result = _object.SelectObjects(
				SqlObjectTypes.FunctionColumn,
				new object[] { "master", "dbo", "fn_nonexistent" }, null);

			// Verify results
			using (result)
			{
				Assert.IsFalse(result.Read());
			}
		}

		#endregion

		#region Private Fields

		private SqlObjectSelector _object;
		private static BaseMock _connectionMock;

		#endregion
	}
}
