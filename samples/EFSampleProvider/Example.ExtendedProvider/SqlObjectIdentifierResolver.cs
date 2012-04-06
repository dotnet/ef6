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
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider
{
	/// <summary>
	/// Represents a custom data object identifier resolver that correctly
	/// expands an identifier to its complete form.  This is required for
	/// certain built in data design scenarios that are initialized with only
	/// a partial identifier and then try to match this identifier with an
	/// object from the server.
	/// </summary>
	internal class SqlObjectIdentifierResolver : DataObjectIdentifierResolver
	{
		#region Public Methods

		/// <summary>
		/// SQL Server connections are always within the context of a current
		/// database and default schema.  This method expands identifiers
		/// that are missing database or schema parts by adding the defaults
		/// appropriately.
		/// </summary>
		public override object[] ExpandIdentifier(
			string typeName, object[] partialIdentifier)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}

			// Find the type in the data object support model
			IVsDataObjectType type = null;
			IVsDataObjectSupportModel objectSupportModel = Site.GetService(
				typeof(IVsDataObjectSupportModel)) as IVsDataObjectSupportModel;
			Debug.Assert(objectSupportModel != null);
			if (objectSupportModel != null &&
				objectSupportModel.Types.ContainsKey(typeName))
			{
				type = objectSupportModel.Types[typeName];
			}
			if (type == null)
			{
				throw new ArgumentException("Invalid type " + typeName + ".");
			}

			// Create an identifier array of the correct full length
			object[] identifier = new object[type.Identifier.Count];

			// If the input identifier is not null, copy it to the full
			// identifier array.  If the input identifier's length is less
			// than the full length we assume the more specific parts are
			// specified and thus copy into the rightmost portion of the
			// full identifier array.
			if (partialIdentifier != null)
			{
				if (partialIdentifier.Length > type.Identifier.Count)
				{
					throw new ArgumentException("Invalid partial identifier.");
				}
				partialIdentifier.CopyTo(identifier,
					type.Identifier.Count - partialIdentifier.Length);
			}

			// Get the data source information service
			IVsDataSourceInformation sourceInformation =
				Site.GetService(typeof(IVsDataSourceInformation))
					as IVsDataSourceInformation;
			Debug.Assert(sourceInformation != null);
			if (sourceInformation == null)
			{
				// This should never occur
				return identifier;
			}

			// Now expand the identifier as required
			if (type.Identifier.Count > 0)
			{
				// Fill in the current database if not specified
				if (!(identifier[0] is string))
				{
					identifier[0] = sourceInformation[
						DataSourceInformation.DefaultCatalog] as string;
				}
			}
			if (type.Identifier.Count > 1)
			{
				// Fill in the default schema if not specified
				if (!(identifier[1] is string))
				{
					identifier[1] = sourceInformation[
						DataSourceInformation.DefaultSchema] as string;
				}
			}

			return identifier;
		}

		#endregion
	}
}
