// Copyright (c) 2013, 2019, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
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
using System.Data.Entity;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class Harbor
    {
        public int HarborId { get; set; }
        public virtual ICollection<Ship> Ships { get; set; }

        public string Description { get; set; }
    }

    public class Ship
    {
        public int ShipId { get; set; }
        public int HarborId { get; set; }
        public virtual Harbor Harbor { get; set; }
        public virtual ICollection<CrewMember> CrewMembers { get; set; }

        public string Description { get; set; }
    }

    public class CrewMember
    {
        public int CrewMemberId { get; set; }
        public int ShipId { get; set; }
        public virtual Ship Ship { get; set; }
        public int RankId { get; set; }
        public virtual Rank Rank { get; set; }
        public int ClearanceId { get; set; }
        public virtual Clearance Clearance { get; set; }

        public string Description { get; set; }
    }

    public class Rank
    {
        public int RankId { get; set; }
        public virtual ICollection<CrewMember> CrewMembers { get; set; }

        public string Description { get; set; }
    }

    public class Clearance
    {
        public int ClearanceId { get; set; }
        public virtual ICollection<CrewMember> CrewMembers { get; set; }

        public string Description { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class ShipContext : DbContext
    {
        public DbSet<Harbor> Harbors { get; set; }
        public DbSet<Ship> Ships { get; set; }
        public DbSet<CrewMember> CrewMembers { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<Clearance> Clearances { get; set; }

        public ShipContext() : base(CodeFirstFixture.GetEFConnectionString<ShipContext>())
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<ShipContext>());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
    }
}
