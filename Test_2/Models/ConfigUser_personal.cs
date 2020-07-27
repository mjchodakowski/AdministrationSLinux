using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class ConfigUser_personal
    {
        public Guid UserId { get; set; }
        public string Nazwa_użytkownika { get; set; }
        public string Haslo { get; set; }
        public string UID { get; set; }
        public string GID { get; set; }
        public string Komentarz { get; set; }
        public string Katalog_domowy { get; set; }
        public string Polecenie_logowania { get; set; }
        public string EditKey { get; set; }
    }
}