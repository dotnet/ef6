// Copyright © 2008, 2017, Oracle and/or its affiliates. All rights reserved.
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
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using System.Text.RegularExpressions;
using XuguClient;

namespace Xugu.Data.EntityFramework
{
    /// <summary>
    /// Used for creating connections in Code First 4.3.
    /// </summary>
    public class XGConnectionFactory : IDbConnectionFactory
    {
        string baseConnString;

        public XGConnectionFactory()
        {
        }

        public XGConnectionFactory(string connStr)
        {
            baseConnString = connStr;
        }

        public DbConnection CreateConnection(string connectionString)
        {
            using (XGConnection c = new XGConnection())
            {
                string dbName = GetDBName(connectionString);
                c.ConnectionString = connectionString;
                try
                {
                    c.Open();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("[E34305]"))
                    {
                        c.ConnectionString = c.ConnectionString.Replace("DB=" + dbName, "DB=" + "SYSTEM");//GetConfigConnection("XuguClient");
                        c.Open();
                        string fullQuery = String.Format("USE SYSTEM;CREATE DATABASE IF NOT EXISTS `{0}`; USE `{0}`;", dbName);
                        XGCommand s = new XGCommand(fullQuery, c);
                        s.ExecuteNonQuery();
                    }
                }
                c.Close();
            }
            return new XGConnection(connectionString);
        }

        private string GetDBName(string connectionString)
        {
            string pattern = @"DB=([^;]+)";

            Match match = Regex.Match(connectionString, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception("No DB name found in connection string");
            }
        }

        public static string GetConfigConnection(string key)
        {
            string connStr = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            if (connStr == null)
            {
                throw new Exception("No connection string found in config file");
            }
            return connStr;
        }
    }
}