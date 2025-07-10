using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class BlogContext : DbContext
    {
        public BlogContext() : base(CodeFirstFixture.GetEFConnectionString<BlogContext>())
        {
        }

        public DbSet<Blog> Blog { get; set; }

        //public override int SaveChanges()
        //{
        //    //Database.Connection.Close();
        //    //Database.Connection.Open();
        //    //return base.SaveChanges();
        //    using (var transaction = Database.Connection.BeginTransaction())
        //    {
        //        int result = base.SaveChanges();
        //        transaction.Commit();
        //        return result;
        //    }

        //}
    }

    [Table("blogtable", Schema = "blogschema")]
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }
        public string Title { get; set; }
    }
}
