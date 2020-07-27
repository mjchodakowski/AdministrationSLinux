using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class ConnectProfil: DbContext
    {
        public DbSet<ProfilPolaczenia> Profile{ get; set; }
    }
}