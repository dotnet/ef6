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
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [Table("pagina")]
    public class pagina
    {
        [Key]
        public long nCdPagina { get; set; }
        public long nCdVisitante { get; set; }
        public string sDsUrlReferencia { get; set; }
        public string sDsPalavraChave { get; set; }
        public string sDsTitulo { get; set; }

        [ForeignKey("nCdVisitante")]
        public visitante visitante { get; set; }
    }

    public class retorno
    {
        //[Key]
        public long Key { get; set; }
        public int Online { get; set; }
    }

    [Table("site")]
    public class site
    {
        [Key]
        public long nCdSite { get; set; }
        public string sDsTitulo { get; set; }
        public string sDsUrl { get; set; }
        public DateTime tDtCadastro { get; set; }
    }

    [Table("visitante")]
    public class visitante
    {
        [Key]
        public long nCdVisitante { get; set; }
        public long nCdSite { get; set; }
        [Column(TypeName = "varchar")]
        public string sDsIp { get; set; }
        public DateTime tDtCadastro { get; set; }
        public DateTime tDtAtualizacao { get; set; }

        [ForeignKey("nCdSite")]
        public site site { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class SiteDbContext : DbContext
    {
        public DbSet<visitante> Visitante { get; set; }
        public DbSet<site> Site { get; set; }
        //public DbSet<retorno> Retorno { get; set; }
        public DbSet<pagina> Pagina { get; set; }

        public SiteDbContext() : base(CodeFirstFixture.GetEFConnectionString<SiteDbContext>())
        {
            Database.SetInitializer<SiteDbContext>(new SiteDbInitializer());
            Database.SetInitializer<SiteDbContext>(new MigrateDatabaseToLatestVersion<SiteDbContext, Configuration<SiteDbContext>>());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Entity<visitante>().ToTable("visitante", "SYSDBA");
            modelBuilder.Entity<site>().ToTable("site", "SYSDBA");
            modelBuilder.Entity<pagina>().ToTable("pagina", "SYSDBA");
        }
    }

    public class SiteDbInitializer : DropCreateDatabaseReallyAlways<SiteDbContext>
    {
    }
}
