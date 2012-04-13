namespace SimpleModel
{
    using System.ComponentModel.DataAnnotations;

    public class Blog
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}