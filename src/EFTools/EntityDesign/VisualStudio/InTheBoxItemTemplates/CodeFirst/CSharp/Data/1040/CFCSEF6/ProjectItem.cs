namespace $rootnamespace$
{
	using System;
    using System.Data.Entity;
    using System.Linq;

    public class $safeitemname$ : DbContext
    {
		$ctorcomment$
        public $safeitemname$()
            : base("name=$safeitemname$")
        {
        }

		$dbsetcomment$

        // public DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}