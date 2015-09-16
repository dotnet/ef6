using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;


namespace EFDesigner.E2ETests
{

    public class MyConf : DbConfiguration
    {
        public MyConf()
        {
#if (VS14 || VS15)
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
#else
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("v11.0"));
#endif
            this.SetDatabaseInitializer<SchoolEntities>(new Initializer1());
        }
    }
    public class Course
    {
        public Course()
        {
            this.StudentGrades = new HashSet<StudentGrade>();
            this.People = new HashSet<Person>();
        }

        public int CourseID { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }
        public int DepartmentID { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<StudentGrade> StudentGrades { get; set; }
        public virtual ICollection<Person> People { get; set; }
    }

    public class Department
    {
        public Department()
        {
            this.Courses = new HashSet<Course>();
        }

        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public decimal Budget { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<int> Administrator { get; set; }
        public Nullable<decimal> Price { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
    }

    public class OfficeAssignment
    {
        [Key]
        public int InstructorID { get; set; }
        public string Location { get; set; }
        public byte[] Timestamp { get; set; }

        public virtual Person Person { get; set; }
    }

    public class OnlineCourse : Course
    {
        public string URL { get; set; }
    }

    public class OnsiteCourse : Course
    {
        public string Location { get; set; }
        public string Days { get; set; }
        public System.DateTime Time { get; set; }
    }

    public class Person
    {
        public Person()
        {
            this.StudentGrades = new HashSet<StudentGrade>();
            this.Courses = new HashSet<Course>();
        }

        public int PersonID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Nullable<System.DateTime> HireDate { get; set; }
        public Nullable<System.DateTime> EnrollmentDate { get; set; }
        public string Discriminator { get; set; }

        public virtual OfficeAssignment OfficeAssignment { get; set; }
        public virtual ICollection<StudentGrade> StudentGrades { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }

    public class StudentGrade
    {
        [Key]
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
        public Nullable<decimal> Grade { get; set; }

        public virtual Course Course { get; set; }
        public virtual Person Person { get; set; }
    }

    public class GetStudentGrades_Result
    {
        public int EnrollmentID { get; set; }
        public Nullable<decimal> Grade { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
    }

    public class SchoolEntities : DbContext
    {
        public SchoolEntities()
            :base("School")
        {   
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>()
                .HasOptional(p => p.OfficeAssignment)
                .WithRequired(p => p.Person);
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<StudentGrade> StudentGrades { get; set; }

    }

    public class Initializer1 : DropCreateDatabaseAlways<SchoolEntities>
    {
    }
}
