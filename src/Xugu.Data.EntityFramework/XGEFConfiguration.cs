// Copyright (c) 2013, 2019, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of XG hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// XG.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of XG Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using XuguClient;
using System.Data.Entity.Migrations;
using System.Reflection;

namespace Xugu.Data.EntityFramework
{
    /// <summary>
    /// Defines the configuration of an application to be used with Entity Framework. 
    /// </summary>
    public class XGEFConfiguration : DbConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XGEFConfiguration"/> class.
        /// </summary>
        public XGEFConfiguration()
        {
            

            AddDependencyResolver(new XGDependencyResolver());

            SetProviderFactory(XGProviderInvariantName.ProviderName, new XGProviderFactory());
            SetProviderServices("Xugu.Data.EntityFramework", XGProviderServices.Instance);
            SetDefaultConnectionFactory(new XGConnectionFactory());
            SetMigrationSqlGenerator(XGProviderInvariantName.ProviderName, () => new XGMigrationSqlGenerator());
            SetProviderFactoryResolver(new XGProviderFactoryResolver());
            SetManifestTokenResolver(new XGManifestTokenResolver());
            SetHistoryContext(XGProviderInvariantName.ProviderName, (existingConnection, defaultSchema) => new XGHistoryContext(existingConnection, "SYSDBA"));
            //      //CURRENTLY IS NOT SUPPORTED WORK WITH TRANSACTIONS AND EXECUTION STRATEGY AT THE SAME TIME: http://msdn.microsoft.com/en-US/data/dn307226
            //      //IF WE SET THE EXECUTION STRATEGY HERE THAT WILL AFFECT THE USERS WHEN THEY TRY TO USE TRANSACTIONS, FOR THAT REASON EXECUTION STRATEGY WILL BE ENABLED ON DEMAND BY THEM
            //      //SetExecutionStrategy(XGProviderInvariantName.ProviderName, () => { return new XGExecutionStrategy(); });
        }
    }
}
