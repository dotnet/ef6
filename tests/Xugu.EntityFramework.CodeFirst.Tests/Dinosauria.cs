// Copyright (c) 2014, 2019, Oracle and/or its affiliates. All rights reserved.
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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    /*
     * This data model tests very long names to break FK limit of 64 chars.
     * Also uses Table per Type inheritance (TPT).
     * */
    public class Animalia_Chordata_Dinosauria_Eusaurischia_Theropoda
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [MaxLength(40)]
        public string Name { get; set; }
    }

    [Table("Tyrannosauridaes")]
    public class Tyrannosauridae : Animalia_Chordata_Dinosauria_Eusaurischia_Theropoda
    {
        public string SpecieName { get; set; }
        public float Weight { get; set; }
    }

    [Table("Oviraptorosaurias")]
    public class Oviraptorosauria : Animalia_Chordata_Dinosauria_Eusaurischia_Theropoda
    {
        public string SpecieName { get; set; }
        public int EggsPerYear { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class DinosauriaDBContext : DbContext
    {
        public DbSet<Animalia_Chordata_Dinosauria_Eusaurischia_Theropoda> dinos { get; set; }

        public DinosauriaDBContext() : base(CodeFirstFixture.GetEFConnectionString<DinosauriaDBContext>())
        {
            Database.SetInitializer<DinosauriaDBContext>(new DinosauriaDBInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Entity<Tyrannosauridae>().ToTable("Tyrannosauridaes");
            modelBuilder.Entity<Oviraptorosauria>().ToTable("Oviraptorosaurias");
        }
    }

    public class DinosauriaDBInitializer : DropCreateDatabaseReallyAlways<DinosauriaDBContext>
    {
    }
}
