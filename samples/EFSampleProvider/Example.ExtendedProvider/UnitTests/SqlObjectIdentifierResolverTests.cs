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
using System.Collections.Generic;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider.UnitTests
{
	[TestClass]
	public class SqlObjectIdentifierResolverTests
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
			_object = new SqlObjectIdentifierResolver();
			_object.Site = _connectionMock as IVsDataConnection;
		}

		#endregion

		#region ExpandIdentifier Tests

		[TestMethod]
		public void ExpandIdentifierTest()
		{
			// Prepare to call method under test
			BaseMock objectTypeMock = new GenericMockFactory("DataObjectType",
				new Type[] { typeof(IVsDataObjectType) }).GetInstance();
			BaseMock objectSupportModelMock = new GenericMockFactory(
				"DataObjectSupportModel",
				new Type[] { typeof(IVsDataObjectSupportModel) })
				.GetInstance();
			BaseMock sourceInformationMock = new GenericMockFactory(
				"DataSourceInformation",
				new Type[] { typeof(IVsDataSourceInformation) })
				.GetInstance();
			_connectionMock.AddMethodCallback(
				typeof(IServiceProvider).FullName + ".GetService",
				delegate(object sender, CallbackArgs e)
				{
					Type type = e.GetParameter(0) as Type;
					if (type == typeof(IVsDataObjectSupportModel))
					{
						e.ReturnValue = objectSupportModelMock;
					}
					if (type == typeof(IVsDataSourceInformation))
					{
						e.ReturnValue = sourceInformationMock;
					}
				}
			);
			objectSupportModelMock.AddMethodCallback(
				typeof(IVsDataObjectSupportModel).FullName + ".get_Types",
				delegate(object sender, CallbackArgs e)
				{
					IDictionary<string, IVsDataObjectType> returnValue =
						new Dictionary<string, IVsDataObjectType>(
							StringComparer.OrdinalIgnoreCase);
					returnValue.Add("Test",
						objectTypeMock as IVsDataObjectType);
					e.ReturnValue = returnValue;
				}
			);
			objectTypeMock.AddMethodCallback(
				typeof(IVsDataObjectType).FullName + ".get_Identifier",
				delegate(object sender, CallbackArgs e)
				{
					// The actual values of the identifier do not matter;
					// only the count of the collection is used by the code
					e.ReturnValue = new IVsDataObjectTypeMember[3];
				}
			);
			sourceInformationMock.AddMethodCallback(
				typeof(IVsDataSourceInformation).FullName + ".get_Item",
				delegate(object sender, CallbackArgs e)
				{
					string propertyName = e.GetParameter(0) as string;
					if (propertyName == DataSourceInformation.DefaultCatalog)
					{
						e.ReturnValue = "Northwind";
					}
					if (propertyName == DataSourceInformation.DefaultSchema)
					{
						e.ReturnValue = "dbo";
					}
				}
			);

			// Call method under test
			object[] result = _object.ExpandIdentifier(
				"Test", new object[] { "Customers" });

			// Verify results
			CollectionAssert.AreEqual(
				new object[] { "Northwind", "dbo", "Customers" },
				result);
		}

		#endregion

		#region Private Fields

		private SqlObjectIdentifierResolver _object;
		private static BaseMock _connectionMock;

		#endregion
	}
}
