using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Test_2.Models;

namespace Test_2.DAL
{
    public class serverFileBD : DbContext
    {
        public serverFileBD() : base("ServerNameFile") { }
        public DbSet<ServerFile> Servers { get; set; }
    }
}