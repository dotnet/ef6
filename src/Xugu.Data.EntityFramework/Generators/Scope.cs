// Copyright © 2008, 2018, Oracle and/or its affiliates. All rights reserved.
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
using System.Diagnostics;

namespace Xugu.Data.EntityFramework
{
  class Scope
  {
    private Dictionary<string, InputFragment> scopeTable = new Dictionary<string, InputFragment>();

    public void Add(string name, InputFragment fragment)
    {
      scopeTable.Add(name, fragment);
    }

    public void Remove( string Name, InputFragment fragment)
    {
      if (fragment == null) return;
      if (Name != null)
        scopeTable.Remove(Name);

      if (fragment is SelectStatement)
        Remove((fragment as SelectStatement).From);
      else if (fragment is JoinFragment)
      {
        JoinFragment j = fragment as JoinFragment;
        Remove(j.Left);
        Remove(j.Right);
      }
      else if (fragment is UnionFragment)
      {
        UnionFragment u = fragment as UnionFragment;
        Remove(u.Left);
        Remove(u.Right);
      }
    }

    public void Remove(InputFragment fragment)
    {
      if( fragment == null ) return;
      Remove(fragment.Name, fragment);
    }

    public InputFragment GetFragment(string name)
    {
      if (!scopeTable.ContainsKey(name))
        return null;
      return scopeTable[name];
    }

    public InputFragment FindInputFromProperties(PropertyFragment fragment)
    {
      Debug.Assert(fragment != null);
      PropertyFragment propertyFragment = fragment as PropertyFragment;
      Debug.Assert(propertyFragment != null);

      if (propertyFragment.Properties.Count >= 2)
      {
        for (int x = propertyFragment.Properties.Count - 2; x >= 0; x--)
        {
          string reference = propertyFragment.Properties[x];
          if (reference == null) continue;
          InputFragment input = GetFragment(reference);
          if (input == null) continue;
          if (input.Scoped) return input;
          if (input is SelectStatement)
            return (input as SelectStatement).From;
          continue;
        }
      }
      Debug.Fail("Should have found an input");
      return null;
    }
  }

  /// <summary>
  /// Specifies the operation types supported.
  /// </summary>
  public enum OpType : int
  {
    Join = 1,
    Union = 2
  }
}
