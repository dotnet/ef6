using System;
using System.Collections.Generic;
using System.Data.Entity;
using Xunit;

namespace SqlProviderSmokeTextNetFx
{

    public class Test1
    {
        [Fact]
        public void SmokeTest1()
        {
            // https://www.entityframeworktutorial.net/code-first/simple-code-first-example.aspx

            using (var ctx = new SchoolContext())
            {
                var stud = new Student() { StudentName = "Bill" };

                ctx.Students.Add(stud);
                ctx.SaveChanges();
            }
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

    public class SchoolContext : DbContext
    {
        public SchoolContext() : base()
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
    }

    public class MyConfiguration : DbConfiguration
    {
        public MyConfiguration()
        {
            SetProviderServices("Microsoft.Data.SqlClient",
                System.Data.Entity.SqlServer.MicrosoftSqlProviderServices.Instance);
        }
    }
}
