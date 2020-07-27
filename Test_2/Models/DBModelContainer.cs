using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class DBModelContainer: DbContext
    {
        public DbSet<ConnectModels> ConnectionUsers { get; set; }
        public DbSet<TempUser> UserTemps { get; set; }
    }
}