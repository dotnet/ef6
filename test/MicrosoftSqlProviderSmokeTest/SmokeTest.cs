using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace MicrsoftSqlProviderSmokeTest
{
    public class SmokeTest
    {
        [Fact]
        public void Microsoft_Data_SqlClient_Provider_Works()
        {
            var connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=School;Integrated Security=True;Encrypt=false";

            using (var ctx = new SchoolContext(new SqlConnection(connectionString)))
            {
                ctx.Database.ExecuteSqlCommand("SELECT 1");
                
                var students = ctx.Students.Where(s => "erik" == System.Data.Entity.SqlServer.SqlFunctions.UserName()).ToList();              
                
                var stud = new Student() { StudentName = "Bill" };

                ctx.Students.Add(stud);
                var result = ctx.SaveChanges();

                Assert.True(ctx.Database.Connection as SqlConnection != null);
                Assert.Equal(1, result);
            }

            using (var ctx = new SchoolContext(connectionString))
            {
                var students = ctx.Students.ToList();

                Assert.True(students.Count > 0);
                Assert.True(ctx.Database.Connection as SqlConnection != null);
            }
        }

        [Fact]
        public void Microsoft_Data_SqlClient_Types_Are_Present_And_Correct()
        {
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlAzureExecutionStrategy));
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlConnectionFactory));
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlDbConfiguration));
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlProviderServices));
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlServerMigrationSqlGenerator));
            Assert.NotNull(typeof(System.Data.Entity.SqlServer.MicrosoftSqlSpatialServices));
        }
    }

    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public byte[] Photo { get; set; }
        public decimal Height { get; set; }
        public float Weight { get; set; }

        public Grade Grade { get; set; }
    }

    public class Grade
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public string Section { get; set; }

        public ICollection<Student> Students { get; set; }
    }

    [DbConfigurationType(typeof(System.Data.Entity.SqlServer.MicrosoftSqlDbConfiguration))]
    public class SchoolContext : DbContext
    {
        public SchoolContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<SchoolContext>());
            Database.Log = Console.WriteLine;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        public SchoolContext(SqlConnection connection) : base(connection, true)
        { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
    }
}
