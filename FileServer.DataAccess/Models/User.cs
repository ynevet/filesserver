using System;

namespace FileServer.DataAccess.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime AddedAt { get; set; }
    }
}