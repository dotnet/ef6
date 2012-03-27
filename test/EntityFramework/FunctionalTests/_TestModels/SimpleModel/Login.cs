namespace SimpleModel
{
    using System;

    public class Login
    {
        public Login() { }
        public Login(Guid id) { Id = id; }
        public Guid Id { get; set; }
        public string Username { get; set; }
    }
}
