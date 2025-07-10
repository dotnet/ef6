// Copyright (c) 2021 Oracle and/or its affiliates.
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

using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    //ContextForNormalFk
    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class ContextForNormalFk : DbContext
    {
        public ContextForNormalFk() : base(CodeFirstFixture.GetEFConnectionString<ContextForNormalFk>())
        {
            Database.SetInitializer<ContextForNormalFk>(new DropCreateDatabaseAlways<ContextForNormalFk>());
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Configurations.AddFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Permiso>().HasKey(c => c.Id);
            modelBuilder.Entity<Permiso>().Property(c => c.Nombre).IsRequired().HasMaxLength(50);
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; }
        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [StringLength(30)]
        public string Login { get; set; }
        [Required]
        [StringLength(64)]
        public string Senha { get; set; }
        [Required]
        public bool Activo { get; set; }
        [ConcurrencyCheck]
        public int Version { get; set; }
    }

    [Table("perfiles")]
    public class Perfil
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [StringLength(100)]
        public string Descripcion { get; set; }
        [ConcurrencyCheck]
        public int Version { get; set; }
        public virtual ICollection<Usuario> Usuarios { get; set; }
        public virtual ICollection<Permiso> Permisos { get; set; }
    }


    [Table("permisos")]
    public class Permiso
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Version { get; set; }
    }

    //ContextForLongFk
    public class Usuario0123456789012345567890123456789012345678901234567890
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(30)]
        public string Login { get; set; }

        [Required]
        [StringLength(64)]
        public string Senha { get; set; }

        [Required]
        public bool Activo { get; set; }

        //[Timestamp]
        [ConcurrencyCheck]
        public int Version { get; set; }
    }

    public class Perfil0123456789012345567890123456789012345678901234567890
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(100)]
        public string Descripcion { get; set; }

        //[Timestamp]
        [ConcurrencyCheck]
        public int Version { get; set; }

        public virtual ICollection<Usuario0123456789012345567890123456789012345678901234567890> Usuarios { get; set; }
        public virtual ICollection<Permiso0123456789012345567890123456789012345678901234567890> Permisos { get; set; }
    }

    public class Permiso0123456789012345567890123456789012345678901234567890
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(100)]
        public string Descripcion { get; set; }

        [ConcurrencyCheck]
        public int Vesion { get; set; }
    }

    public class ContextForLongFk : DbContext
    {
        public ContextForLongFk() : base(CodeFirstFixture.GetEFConnectionString<ContextForLongFk>())
        {
            Database.SetInitializer<ContextForLongFk>(new DropCreateDatabaseAlways<ContextForLongFk>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<Usuario0123456789012345567890123456789012345678901234567890> Usuarios { get; set; }
        public DbSet<Perfil0123456789012345567890123456789012345678901234567890> Perfiles { get; set; }
        public DbSet<Permiso0123456789012345567890123456789012345678901234567890> Permisos { get; set; }
    }
}
