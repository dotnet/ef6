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
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider.UnitTests
{
	[TestClass]
	public class SqlSourceInformationTests
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
			_object = new SqlSourceInformation();
			_object.Site = _connectionMock as IVsDataConnection;
		}

		#endregion

		#region Item Indexer Tests

		//
		// All the Item Indexer tests require the default instance of SQL
		// Server Express Edition to be installed (named SQLEXPRESS).
		//

		[TestMethod]
		public void ItemIndexerTest()
		{
			// Prepare to call method under test
			BaseMock connectionSupportMock = new GenericMockFactory(
				"DataConnectionSupport",
				new Type[] { typeof(IVsDataConnectionSupport) })
				.GetInstance();
			SqlConnection conn = new SqlConnection(
				"Data Source=.\\SQLEXPRESS;" +
				"Integrated Security=true;" +
				"Initial Catalog=master");
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
			_connectionMock.AddMethodCallback(
				typeof(IServiceProvider).FullName + ".GetService",
				delegate(object sender, CallbackArgs e)
				{
					Type type = e.GetParameter(0) as Type;
					if (type == typeof(IVsDataConnectionSupport))
					{
						e.ReturnValue = connectionSupportMock;
					}
				}
			);
			connectionSupportMock.AddMethodCallback(
				typeof(IVsDataConnectionSupport).FullName + ".get_ProviderObject",
				delegate(object sender, CallbackArgs e)
				{
					e.ReturnValue = conn;
				}
			);

			// Call method under test
			object result = _object[DataSourceInformation.DefaultSchema];

			// Verify results
			Assert.AreEqual("dbo", result);
		}

		#endregion

		#region Private Fields

		private SqlSourceInformation _object;
		private static BaseMock _connectionMock;

		#endregion
	}
}
