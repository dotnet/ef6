// Copyright (c) 2014, 2022, Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of Xugu hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// Xugu.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of Xugu Connector/NET, is also subject to the
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.Tests
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime PubDate { get; set; }
        public Author Author { get; set; }
        public int Pages { get; set; }
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Book> Books { get; set; }
        public Address Address { get; set; }
    }

    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinAge { get; set; }
        public float Weight { get; set; }
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
    }

    public class Child
    {
        public string ChildId { get; set; }
        public string Name { get; set; }
        public Guid Label { get; set; }
        public string BirthTimeString { get; private set; }
        [NotMapped]
        public TimeSpan BirthTime
        {
            get => TimeSpan.Parse(BirthTimeString);
            set => BirthTimeString = value.ToString();
        }
    }

    public class ContractAuthor
    {
        public int Id { get; set; }
        public Author Author { get; set; }
        public DateTime StartDate { get; set; }
    }

    [ComplexType]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateBegan { get; set; }
        public int NumEmployees { get; set; }
        public Address Address { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class DefaultContext : DbContext
    {
        public DefaultContext(string connStr) : base(connStr)
        {
            Database.SetInitializer<DefaultContext>(null);
            //Database.Connection.Open();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ContractAuthor> ContractAuthors { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<LongDataTest> LongDataTests { get; set; }
    }

    public class BlogContext : DbContext
    {
        public BlogContext(string connStr) : base(connStr)
        {
            //Database.SetInitializer<BlogContext>(null);
        }

        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(e => e.Name)
                .IsUnicode(false);
        }
    }

    [Table("usertable", Schema = "blogcontext")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [StringLength(45)]
        public string Name { get; set; }
    }

    public class LongDataTest
    {
        public int Id { get; set; }

        [StringLength(15)]
        public string Data { get; set; }
    }
}