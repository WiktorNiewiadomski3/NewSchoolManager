﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

#nullable disable

namespace SchoolManager.Models
{
    public partial class Class
    {
        public Class()
        {
            Students = new HashSet<Student>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? TeacherId { get; set; }

        public virtual Teacher Teacher { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}