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
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;


namespace Xugu.Data.EntityFramework
{
  class FunctionGenerator : SqlGenerator
  {
    public CommandType CommandType { get; private set; }

    public override string GenerateSQL(DbCommandTree commandTree)
    {
      DbFunctionCommandTree tree = (commandTree as DbFunctionCommandTree);
      EdmFunction function = tree.EdmFunction;
      CommandType = CommandType.StoredProcedure;

      string cmdText = (string)function.MetadataProperties["CommandTextAttribute"].Value;
      if (String.IsNullOrEmpty(cmdText))
      {
        string schema = (string)function.MetadataProperties["Schema"].Value;
        if (String.IsNullOrEmpty(schema))
          schema = function.NamespaceName;

        string functionName = (string)function.MetadataProperties["StoreFunctionNameAttribute"].Value;
        if (String.IsNullOrEmpty(functionName))
          functionName = function.Name;

        return String.Format("`{0}`", functionName);
      }
      else
      {
        CommandType = CommandType.Text;
        return cmdText;
      }
    }
  }
}
