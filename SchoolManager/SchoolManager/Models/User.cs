using SchoolManager.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public UserTypes Type { get; set; }
    }
}
