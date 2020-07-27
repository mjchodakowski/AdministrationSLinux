using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Test_2.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Test_2.DAL
{
    public class ProfileUserConnectionLinux : DbContext
    {
        public ProfileUserConnectionLinux() :base("ProfileUserConnectionLinux") {}
        public DbSet<ProfileUser> Profile { get; set; }


    }
}